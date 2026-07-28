# SettingsLib Design and Model

A clean, standalone BepInEx 6 plugin that exposes a generic `SettingsMenu` API for every BepInEx config type, treats keybinds as a dual-key specialization, and injects per-mod grouped settings rows into the native Gunfire Reborn `PC_Panel_setting` UI using native templates, native tabs, and custom tabs created on demand.

## Project Goals

1. **Seamless integration**: Modded settings must be visually and functionally indistinguishable from vanilla settings. This means matching fonts, colors, sizes, layout, navigation, and interaction patterns exactly. A user should not be able to tell which settings are modded and which are vanilla.
2. **No vanilla savefile poisoning**: All mod setting values are stored exclusively in BepInEx `ConfigFile` entries (per-mod `.cfg` files under `BepInEx/config/`). The mod never writes to, modifies, or interferes with the game's native save system. This is the highest priority invariant — any change that risks touching vanilla save data is rejected.
3. **Native parity via property capture, not cloning**: Custom tabs, rows, group headers, and toggles are built from scratch as new GameObjects. Pure data properties (fonts, colors, font sizes, outline widths, sprite references, RectTransform dimensions) are captured from vanilla UI elements at runtime via the `StyleCatalog` and dedicated `*Capture` classes. This hybrid approach ensures visual parity without the risks of cloning vanilla GameObjects, which can inherit serialized references to vanilla save/persistence systems and cause savefile poisoning. Only data-only properties are copied — never components with logic, events, or callbacks. Custom tabs use `M1Toggle` (not `Button`) and register with the vanilla `M1ToggleGroup` for native navigation.

## Core Principles

- **No copying of `SettingsLib_old`**: every line is written from scratch for clarity and maintainability.
- **Composition over inheritance**: domain objects share behavior through interfaces and records; UI row controllers are thin and specialized.
- **Nullable reference types enabled**; no underscore-prefixed identifiers.
- **HarmonyX patches are triggers only**: real UI logic lives in injectors and controllers.
- **Native parity**: fonts, colors, layout, and sprites from the native settings menu are captured and reused. All custom UI elements are built from scratch — never cloned from vanilla GameObjects — to avoid inheriting serialized references to vanilla persistence/save systems.
- **No vanilla savefile poisoning**: all mod config values live in BepInEx `ConfigFile` entries; the mod never touches the game's native save system.
- **Edge-triggered keybind dispatch**: callbacks fire once on key-down, and are suppressed while the settings menu is open.

## Domain Model

```csharp
public readonly record struct SettingLocation(string Tab, string Group, string? SubGroup = null);

public interface ISettingEntry
{
    string Id { get; }
    ConfigEntryBase Config { get; }
    string Label { get; }
    SettingLocation Location { get; }
}

public sealed class SettingEntry<T> : ISettingEntry
{
    public string Id { get; }
    public ConfigEntry<T> Config { get; }
    public string Label { get; }
    public SettingLocation Location { get; }
    public Action<T>? OnValueChanged { get; }

    ConfigEntryBase ISettingEntry.Config => Config;
}

public sealed class KeybindEntry : ISettingEntry
{
    public string Id { get; }
    public ConfigEntry<KeyCode> Primary { get; }
    public ConfigEntry<KeyCode>? Secondary { get; }
    public string Label { get; }
    public SettingLocation Location { get; }
    public Action? OnPressed { get; }
    public Action? OnReleased { get; }

    ConfigEntryBase ISettingEntry.Config => Primary;
}

public sealed record KeybindRegistration(ConfigEntry<KeyCode> Primary, ConfigEntry<KeyCode>? Secondary);
```

- `SettingLocation` is the value object that decides tab, group header, and optional sub-group styling.
- `SettingsRegistry` owns the authoritative list of `ISettingEntry` instances and provides lookups by `Tab`, `Group`, and `SubGroup`.
- `ISettingEntry` is intentionally non-generic so keybinds and scalar settings coexist in the same registry.

## Public API

The public API is in the `SettingsLib.Public` namespace and is now expressed through `SettingOptions<T>` and `KeybindOptions` records. Legacy string/`SettingsTab` overloads still exist but are marked `[Obsolete]`.

```csharp
public static class SettingsMenu
{
    // Generic scalar / enum / string setting. Creates the ConfigEntry and registers the UI row.
    public static ConfigEntry<T> Register<T>(ConfigFile configFile, SettingOptions<T> options);

    // Register an already-created ConfigEntry.
    public static ConfigEntry<T> Register<T>(ConfigEntry<T> config, SettingOptions<T> options);

    // Dual-key keybind registration. Creates the ConfigEntries and registers the UI row.
    public static KeybindRegistration RegisterKeybind(ConfigFile configFile, KeybindOptions options);

    // Keybind registration from existing ConfigEntries. Secondary can be null.
    public static KeybindRegistration RegisterKeybind(
        ConfigEntry<KeyCode> primary,
        ConfigEntry<KeyCode>? secondary,
        KeybindOptions options);
}

public sealed record SettingOptions<T>(
    string Section,
    string Key,
    T DefaultValue,
    string Description,
    string Label,
    string Tab,
    string Group,
    string? SubGroup = null,
    Action<T>? OnValueChanged = null,
    AcceptableValueBase? AcceptableValues = null)
{
    public SettingOptions<T> WithTab(SettingsTab tab) => this with { Tab = tab.ToNativeName() };
}

public sealed record KeybindOptions(
    string Section,
    string Key,
    KeyCode DefaultPrimary,
    string Description,
    string Label,
    string Tab,
    string Group,
    string? SubGroup = null,
    KeyCode? DefaultSecondary = null,
    Action? OnPressed = null,
    Action? OnReleased = null)
{
    public KeybindOptions WithTab(SettingsTab tab) => this with { Tab = tab.ToNativeName() };
}
```

- `tab` may be a native tab name (`Game Settings`, `Mouse/Keyboard`, `Video`, `Audio`, `Controller`) or a custom name that becomes a new tab. Use `WithTab(SettingsTab)` for the enum values.
- `group` is normally the mod name; one group header is created per mod per tab.
- `subGroup` is optional and rendered with a visually different header.

## Dual Keybinding and BepInEx Config

BepInEx stores one value per `ConfigEntry`. A dual keybind is therefore represented by **two independent `ConfigEntry<KeyCode>` instances** owned by the same `KeybindEntry`:

- `Primary` is always created.
- `Secondary` is optional and is `null` when the mod does not want a second binding.

### Creating the ConfigEntries

When `SettingsMenu.RegisterKeybind(ConfigFile, KeybindOptions)` is called, it binds them as follows:

```csharp
var primary = configFile.Bind(section, key, defaultPrimary, configDescription);
var secondary = defaultSecondary.HasValue
    ? configFile.Bind(section, $"{key}Secondary", defaultSecondary.Value, configDescription)
    : null;
```

The secondary key lives in the same BepInEx section with a `Secondary` suffix so it is human-readable in the config file and automatically persisted by BepInEx.

### Reflecting keybind changes back to config

`KeybindRow` renders two capture buttons: one for `Primary` and, when `Secondary` is non-null, one for `Secondary`. When the user clicks a button and presses a new key:

1. The row enters capture mode and consumes the next key-down event.
2. It sets the matching `ConfigEntry<KeyCode>.Value` to the captured `KeyCode`.
3. It calls `ConfigFile.Save()` so the change is written to disk immediately.
4. It refreshes the button text using a friendly key name.
5. It shows the vanilla keybind toast via `IKeybindRowServices.ShowKeybindToast`.

Because the `ConfigEntry` is the source of truth, the row also subscribes to `ConfigEntry.SettingChanged` so external edits (manual config file changes or other mods calling `config.Value = ...`) update the UI immediately.

### Input dispatch

`InputDispatcher` polls `KeybindEntry.Primary.Value` and `KeybindEntry.Secondary?.Value` each frame. Only the active key state matters; the callback does not care which entry triggered it.

## Runtime Architecture

```
Plugin (BasePlugin)
  Load()
    -> Harmony("zeprus.gunfire.settingslib").PatchAll()
    -> AddComponent<SettingsMenuManager>()

SettingsMenuManager (MonoBehaviour)
  Awake()  -> register IL2CPP types; create PanelTracker, SettingsRegistry, UIFinder,
              TabManager, RowFactory, SettingsInjector, InputDispatcher;
              subscribe events; on critical failure log and disable
  Update() -> TrackPanel
              InitializeUIIfNeeded
              RebuildIfRequested
              UpdateRowsAndState
              ValidateTabState
              PollInputAndToast
  OnDestroy() -> unsubscribe events; reset state
  OnPanelOpened()  -> RequestRebuild
  OnPanelClosed()  -> SettingsPanelState.IsBlockingClose = false; injector.Clear();
                      tabManager.OnPanelClosed(); uiFinder.Reset(); panelTracker.Reset()
  OnEntryRegistered() -> RequestRebuild
  ShowKeybindToast() -> toastManager.Show(...)

Core/Entries
  ISettingEntry         -> contract for scalar and keybind entries
  IKeybindEntry         -> keybind-specific contract
  SettingEntry<T>       -> scalar setting
  KeybindEntry          -> dual-key keybind

Core/State
  SettingsRegistry      -> register + query ISettingEntry; lazy Default fallback
  SettingLocation       -> tab/group/sub-group value object
  SettingsPanelState    -> shared static state: IsCapturing, IsBlockingClose,
                           KeybindPanelRefreshed event
  PanelTracker          -> tracks whether PC_Panel_setting is open via SettingsPanelStateListener
  RebuildCoordinator    -> debounces and triggers panel rebuilds
  InputDispatcher       -> polls keybinds, suppresses callbacks in menus
  TransformFinder       -> safe hierarchy search helper (avoids Il2Cpp foreach on Transform)
  SettingsPanelStateListener -> IL2CPP MonoBehaviour on the panel that raises open/closed events

UI/Services
  UIFinder              -> lazy cached transforms / components by path; triggers StyleCatalog.CaptureFrom
  TextAppearanceApplier -> shared helper that applies a TextAppearance to a TextMeshProUGUI
  PanelLocator          -> walks up the hierarchy to find PC_Panel_setting from a child transform
  ToastManager          -> displays and hides the vanilla keybind toast
  WwiseAudio            -> helper for posting captured Wwise event IDs
  IKeybindRowServices   -> interface the manager implements so keybind rows can show toasts

UI/Resolvers
  RowTypeResolver       -> maps ISettingEntry to RowType (Toggle, Slider, Dropdown, InputField, Keybind)
  AcceptableValueResolver -> extracts lists and ranges from AcceptableValueBase via reflection
  OptionResolver        -> resolves the list of values a dropdown row should display

UI/Managers
  TabManager            -> native/custom tab lifecycle; M1Toggle integration; delegates to Tabs/ components
  SettingsInjector      -> orchestrates group/sub-group/row construction; calls RowFactory and GroupBuilder

UI/Factories
  RowFactory            -> maps SettingType to RowType; resolves captured audio IDs; delegates to Builders then Controllers

UI/Builders (pure GameObject construction, no logic or data binding)
  RowElementBuilder     -> shared helpers: CreateRowRoot, CreateTitle, CreateItem,
                           CreateKeybindButton, CreateObject, SetRect, AddText, AddImage
                           + Metrics constants (layout dimensions, control colors)
  GroupBuilder          -> creates and caches group containers and sub-group headers
  SliderElementBuilder  -> Build(name, RowStyle, SliderStyle, parent) -> Transform
  ToggleElementBuilder  -> Build(name, RowStyle, ToggleStyle, parent) -> Transform
  KeybindElementBuilder -> Build(name, RowStyle, KeybindButtonStyle, parent) -> Transform
  DropdownElementBuilder -> Build(name, RowStyle, DropdownStyle, parent) -> Transform
  InputElementBuilder   -> Build(name, RowStyle, InputStyle?, parent) -> Transform
  TabButtonBuilder      -> custom tab button construction; consumed by CustomTabFactory
  GroupContainerBuilder -> Build(name, title, RowStyle, GroupHeaderStyle, parent) -> Transform
  VanillaComponentApplier -> attaches DYSelect, AkGameObj, and AkTriggerMouseClick to controls

UI/Controllers (row controllers with ConfigEntry two-way binding, implement ISettingRow)
  SettingRowBase        -> Bind/Unbind/Refresh lifecycle; cached ValueChanged delegate
  KeybindRow            -> binds a KeybindEntry; uses KeybindCaptureController and KeybindCoverMask
  KeybindCaptureController -> polls input and drives the WaitRelease/WaitPress/ReleaseNextFrame state machine
  KeybindCoverMask      -> manages the vanilla covery_mask during keybind capture
  SliderRow             -> float/int range; cached onSliderChanged delegate; posts Wwise audio throttled
  ToggleRow             -> bool on/off; cached onToggled delegate; posts Wwise audio on change
  DropdownRow           -> enum / acceptable-value list; uses OptionResolver; posts Wwise audio
  InputFieldRow         -> string (TMP_InputField); cached onEndEdit delegate; posts Wwise audio on focus
  RowHoverHandler       -> MonoBehaviour on each row root; tints background Image and writes
                           the setting description to the vanilla setting_desc/desc text

UI/Tabs (custom tab bar support)
  CustomTabRegistry     -> owns all custom tab state by name
  CustomTab             -> Button/Content/M1Toggle tuple
  CustomTabFactory      -> creates M1Toggle tab buttons and content panels
  NativeTabResolver     -> maps native tab names to existing vanilla tab objects
  TabActivationController -> toggles custom/native tab content visibility and sets ScrollRect.content
  TabBarViewportFactory -> creates and parents the masked SL_TabViewport
  TabBarController      -> thin orchestrator over the tab bar collaborators
  TabButtonCollection   -> ordered TabButton list, active toggle, index lookup, onValueChanged wiring
  TabBarView            -> viewport, content RectTransform, ScrollRect, layout groups, source tab hiding
  TabBarScrollAnimator  -> ScrollTo, ScrollToStart, and per-frame scroll lerp
  TabBarVisuals         -> selected/unselected visual sync
  TabBarNavigator       -> NavigateNext/Previous, wrapping, arrow button audio
  TabBarLayout          -> shared helper for tab bar edge/paging/wrap calculations

UI/Styles
  StyleCatalog          -> aggregates all captured sub-styles; orchestrates CaptureFrom(panelRoot)
  TextAppearance        -> shared value object: font, material, fontSize, fontSizeMin/Max,
                           color, alignment, fontStyle, outlineWidth, wordWrapping,
                           autoSizing, overflowMode; From(TextMeshProUGUI, fallbackFontSize) factory
  RectData              -> anchor/pivot/size/position for captured RectTransforms
  *StyleCapture         -> static classes that capture vanilla UI properties into *Style records
  *Style                -> readonly record structs that hold pure data (Row, Tab, GroupHeader,
                           KeybindButton, Toggle, Slider, Dropdown, Input)

Patches
  KeyBoardPanelManagerPatch     -> Postfix on ShowShowItem to raise SettingsPanelState.KeybindPanelRefreshed
  ClosePanelBlockerPatch        -> Prefix on settings-panel back buttons; blocks when capturing or blocking close
```

## Style System

`StyleCatalog.CaptureFrom(panelRoot)` is the single entry point for visual property capture. It uses focused `*Capture` static classes, one per UI element:

- `RowStyleCapture`
- `TabStyleCapture`
- `GroupHeaderStyleCapture`
- `KeybindButtonStyleCapture`
- `SliderStyleCapture`
- `ToggleStyleCapture`
- `DropdownStyleCapture`
- `InputStyleCapture`

Each capture class scans the vanilla hierarchy, reads data-only properties from components (`TextMeshProUGUI`, `Image`, `Button`, `Toggle`, `Slider`, `TMP_Dropdown`, `AkEvent`), and returns a `readonly record struct`. No components with logic, events, or callbacks are copied.

`TextAppearance` is a shared value object used across all style records. `RectData` captures anchor, pivot, size, and position for layout values.

`StyleCatalog` stores the resulting records and passes `RowStyle.Title` and `RowStyle.BackgroundSprite` as fallbacks to dependent captures so no style record is created with null fonts or sprites.

## UI Integration

### Panel lifecycle

A `SettingsPanelStateListener` MonoBehaviour in `Core/State` is added to `PC_Panel_setting` once it is discovered. Its `OnEnable` / `OnDisable` events are wired by `PanelTracker` to set `IsOpen` and notify `SettingsMenuManager` to inject or tear down mod rows.

`KeyBoardPanelManagerPatch` is a safety net: when the native keybind panel refreshes, a Postfix on `ShowShowItem` raises `SettingsPanelState.KeybindPanelRefreshed` so `SettingsMenuManager` can request a rebuild.

### Row construction

`SettingsInjector.Rebuild` iterates over registered tabs. For each tab it asks `TabManager.GetOrCreateContentForTab` for a content panel, then builds groups and rows inside that panel.

`GroupBuilder` creates the group and optional sub-group containers. `RowFactory.CreateRow` determines the `RowType` from `ISettingEntry.Config.SettingType` and `AcceptableValues`, then delegates to the appropriate builder in `UI/Builders/`. Builders are pure construction — they create GameObjects, set RectTransforms, and attach Unity components, but contain no `ConfigEntry` or `ISettingEntry` references. The resulting `Transform` is passed to the matching controller in `UI/Controllers/`, which handles two-way data binding, event subscription, and value syncing.

`RowElementBuilder` provides shared helpers (`CreateRowRoot`, `CreateTitle`, `CreateItem`, `CreateKeybindButton`, `CreateObject`, `SetRect`, `AddText`, `AddImage`) and the `Metrics` nested class with named constants for layout dimensions and control colors. `AddText` takes a `TextAppearance` as the single source of truth for the text component's appearance.

`RowHoverHandler` is a registered IL2CPP `MonoBehaviour` on each row root. It detects pointer hover, tints the row's background `Image`, and writes the setting's description to the vanilla `setting_desc/desc` `TextMeshProUGUI`. `SettingsInjector.UpdateRows` polls all rows' hover state and updates the description text exactly once per frame.

### Tab construction

A custom tab is a settings tab that behaves and looks like the vanilla tabs in `tab_switch`. Any distinct non-native tab name gets its own custom tab.

- Custom tabs visually and functionally mirror vanilla tabs.
- Custom tab state, builders, and GameObjects are managed by `TabManager`. Other systems request content through `TabManager.GetOrCreateContentForTab` and never interact with custom tab GameObjects directly.
- Custom tab buttons are `M1Toggle` instances created by `TabButtonBuilder` and registered with the vanilla `M1ToggleGroup` on `tab_switch`.
- Each custom tab has its own `Content_SL_<name>` panel under `setting_scroll/Viewport`. `TabActivationController` activates the matching panel and sets `ScrollRect.content` when a custom tab is selected.
- The `M1ToggleGroup` handles left/right navigation between all registered tabs. Custom tabs are appended after the vanilla tabs.
- `TabBarViewportFactory` creates a masked `SL_TabViewport` around `tab_switch`. `TabBarView` reparents the tab bar into it and refreshes sizing each session.
- `TabBarScrollAnimator` translates the tab bar so the active tab stays visible while clamping to the first/last tab. `TabBarNavigator` drives `NavigateNext`/`NavigatePrevious` with wrap-around.
- `TabManager` destroys all custom tab GameObjects and clears its internal caches when the panel closes, then recreates the tabs from `SettingsRegistry` each time the panel reopens. This keeps references valid whether the panel is disabled or fully recreated.
- Custom content panels are inserted just before the vanilla `covery_mask` so the keybind-capture overlay renders on top of them.
- `TabStyleCapture` looks up `tab_switch` by name through `TransformFinder` so it still succeeds after the tab bar has been reparented into `SL_TabViewport`.

### Input Dispatch

`InputDispatcher.Poll` runs inside `SettingsMenuManager.PollInputAndToast`:

1. If the settings panel is open, skip all callbacks.
2. For each `KeybindEntry`, check `Input.GetKeyDown(Primary)` and `Input.GetKeyDown(Secondary)`.
3. On key-down, invoke `OnPressed` once.
4. If `OnReleased` is set, invoke it once on `Input.GetKeyUp`.

This is edge-triggered and single-frame, so keybinds do not fire while held and do not fire while the settings menu is open.

## Project Layout

```
Mods/SettingsLib/
├── SettingsLib.csproj
├── README.md
├── DESIGN.md
├── ReviewPlan.md
└── src/
    ├── Plugin.cs
    ├── SettingsMenuManager.cs
    ├── Public/
    │   ├── SettingsMenu.cs
    │   ├── SettingOptions.cs
    │   ├── KeybindOptions.cs
    │   ├── SettingsTab.cs
    │   └── KeybindRegistration.cs
    ├── Core/
    │   ├── Entries/
    │   │   ├── ISettingEntry.cs
    │   │   ├── IKeybindEntry.cs
    │   │   ├── SettingEntry.cs
    │   │   └── KeybindEntry.cs
    │   └── State/
    │       ├── SettingLocation.cs
    │       ├── SettingsRegistry.cs
    │       ├── InputDispatcher.cs
    │       ├── PanelTracker.cs
    │       ├── RebuildCoordinator.cs
    │       ├── SettingsPanelState.cs
    │       ├── SettingsPanelStateListener.cs
    │       └── TransformFinder.cs
    ├── UI/
    │   ├── Resolvers/
    │   │   ├── RowTypeResolver.cs
    │   │   ├── AcceptableValueResolver.cs
    │   │   └── OptionResolver.cs
    │   ├── Services/
    │   │   ├── UIFinder.cs
    │   │   ├── TextAppearanceApplier.cs
    │   │   ├── PanelLocator.cs
    │   │   ├── ToastManager.cs
    │   │   ├── WwiseAudio.cs
    │   │   └── IKeybindRowServices.cs
    │   ├── Managers/
    │   │   ├── TabManager.cs
    │   │   └── SettingsInjector.cs
    │   ├── Factories/
    │   │   └── RowFactory.cs
    │   ├── Models/
    │   │   └── RowType.cs
    │   ├── Builders/
    │   │   ├── RowElementBuilder.cs
    │   │   ├── GroupBuilder.cs
    │   │   ├── GroupContainerBuilder.cs
    │   │   ├── SliderElementBuilder.cs
    │   │   ├── ToggleElementBuilder.cs
    │   │   ├── KeybindElementBuilder.cs
    │   │   ├── DropdownElementBuilder.cs
    │   │   ├── InputElementBuilder.cs
    │   │   ├── TabButtonBuilder.cs
    │   │   └── VanillaComponentApplier.cs
    │   ├── Controllers/
    │   │   ├── ISettingRow.cs
    │   │   ├── SettingRowBase.cs
    │   │   ├── KeybindRow.cs
    │   │   ├── KeybindCaptureController.cs
    │   │   ├── KeybindCoverMask.cs
    │   │   ├── SliderRow.cs
    │   │   ├── ToggleRow.cs
    │   │   ├── DropdownRow.cs
    │   │   ├── InputFieldRow.cs
    │   │   └── RowHoverHandler.cs
    │   ├── Styles/
    │   │   ├── StyleCatalog.cs
    │   │   ├── TextAppearance.cs
    │   │   ├── RectData.cs
    │   │   ├── RowStyle.cs
    │   │   ├── RowStyleCapture.cs
    │   │   ├── TabStyle.cs
    │   │   ├── TabStyleCapture.cs
    │   │   ├── GroupHeaderStyle.cs
    │   │   ├── GroupHeaderStyleCapture.cs
    │   │   ├── KeybindButtonStyle.cs
    │   │   ├── KeybindButtonStyleCapture.cs
    │   │   ├── ToggleStyle.cs
    │   │   ├── ToggleStyleCapture.cs
    │   │   ├── SliderStyle.cs
    │   │   ├── SliderStyleCapture.cs
    │   │   ├── DropdownStyle.cs
    │   │   ├── DropdownStyleCapture.cs
    │   │   ├── DropdownItemStyle.cs
    │   │   ├── DropdownTemplateStyle.cs
    │   │   ├── DropdownScrollbarStyle.cs
    │   │   ├── InputStyle.cs
    │   │   ├── InputStyleCapture.cs
    │   │   └── ...
    │   └── Tabs/
    │       ├── CustomTab.cs
    │       ├── CustomTabRegistry.cs
    │       ├── CustomTabFactory.cs
    │       ├── NativeTabResolver.cs
    │       ├── TabActivationController.cs
    │       ├── TabBarController.cs
    │       ├── TabBarLayout.cs
    │       ├── TabBarNavigator.cs
    │       ├── TabBarScrollAnimator.cs
    │       ├── TabBarView.cs
    │       ├── TabBarViewportFactory.cs
    │       ├── TabBarVisuals.cs
    │       └── TabButtonCollection.cs
    └── Patches/
        ├── KeyBoardPanelManagerPatch.cs
        └── ClosePanelBlockerPatch.cs
```

## Technology Stack

- **Target framework**: `net6.0`
- **BepInEx**: `BepInEx.Unity.IL2CPP` 6.0.0-be.* with `BepInEx.PluginInfoProps`
- **Patching**: HarmonyX
- **Interop references**: `Assembly-CSharp`, `UnityEngine.CoreModule`, `UnityEngine.UI`, `UnityEngine.InputModule`, `Unity.TextMeshPro`, `Il2Cppmscorlib`
- **Language features**: C# 10+ records, `init` properties, nullable reference types, `required` where appropriate, `switch` expressions for tab/group mapping

## Risks

- Custom tab switching integrates with `M1Toggle`/`M1ToggleGroup` for native left/right navigation and mutual exclusivity. `TabBarScrollAnimator` keeps the active tab visible while respecting tab bar boundaries, and `TabManager` detects when native code re-activates a vanilla content panel and tears down custom tab state.
- `TMP_InputField` for strings needs to handle Chinese input and native on-screen keyboard gracefully; fallback is a simple `TMP_InputField` with `EndEdit` event.
- `Register<T>` with `enum` requires converting the selected dropdown index back to the enum value; `AcceptableValueList` and `AcceptableValueRange` are inspected at registration time.
- `PanelTracker.TryLocatePanel` relies on hardcoded hierarchy paths (`CUIManager/Canvas_PC(Clone)/SettingRoot/PC_Panel_setting`); game updates that rename nodes will produce warning logs but silent failure until paths are updated.
- Style capture depends on the vanilla settings panel hierarchy. If the vanilla layout changes, capture classes must be updated before any rows can be rendered.
