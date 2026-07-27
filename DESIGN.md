# SettingsLib Design and Model

A clean, standalone BepInEx 6 plugin that exposes a generic `SettingsMenu` API for every BepInEx config type, treats keybinds as a dual-key specialization, and injects per-mod grouped settings rows into the native Gunfire Reborn `PC_Panel_setting` UI using native templates, native tabs, and one optional custom tab per mod.

## Project Goals

1. **Seamless integration**: Modded settings must be visually and functionally indistinguishable from vanilla settings. This means matching fonts, colors, sizes, layout, navigation, and interaction patterns exactly. A user should not be able to tell which settings are modded and which are vanilla.
2. **No vanilla savefile poisoning**: All mod setting values are stored exclusively in BepInEx `ConfigFile` entries (per-mod `.cfg` files under `BepInEx/config/`). The mod never writes to, modifies, or interferes with the game's native save system. This is the highest priority invariant — any change that risks touching vanilla save data is rejected.
3. **Native parity via property capture, not cloning**: Custom tabs, rows, group headers, and toggles are built from scratch as new GameObjects. However, pure data properties (fonts, colors, font sizes, outline widths, sprite references, RectTransform dimensions) are captured from vanilla UI elements at runtime via the `StyleCatalog` and its sub-style records (`TextAppearance`, `RowStyle`, `TabStyle`, `GroupHeaderStyle`, `KeybindButtonStyle`). This hybrid approach ensures visual parity without the risks of cloning vanilla GameObjects, which can inherit serialized references to vanilla save/persistence systems and cause savefile poisoning. Only data-only properties are copied — never components with logic, events, or callbacks. Custom tabs use `M1Toggle` (not `Button`) and register with the vanilla `M1ToggleGroup` for native navigation.

## Core Principles

- **No copying of `SettingsLib_old`**: every line is written from scratch for clarity and maintainability.
- **Composition over inheritance**: domain objects share behavior through interfaces and records; UI row controllers are thin and specialized.
- **Nullable reference types enabled**; no underscore-prefixed identifiers.
- **HarmonyX patches are triggers only**: real UI logic lives in injectors and controllers.
- **Native parity**: fonts, colors, layout, and sprites from the native settings menu are captured and reused. All custom UI elements are built from scratch — never cloned from vanilla GameObjects — to avoid inheriting serialized references to vanilla persistence/save systems.
- **No vanilla savefile poisoning**: all mod config values live in BepInEx `ConfigFile` entries; the mod never touches the game's native save system.
- **Edge-triggered keybind dispatch**: callbacks fire once on key-down and are suppressed only while the settings menu is open.

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

## Dual Keybinding and BepInEx Config

BepInEx stores one value per `ConfigEntry`. A dual keybind is therefore represented by **two independent `ConfigEntry<KeyCode>` instances** owned by the same `KeybindEntry`:

- `Primary` is always created.
- `Secondary` is optional and is `null` when the mod does not want a second binding.

### Creating the ConfigEntries

When `SettingsMenu.RegisterKeybind` receives a `ConfigFile` rather than pre-created entries, it binds them as follows:

```csharp
var primary = configFile.Bind(section, $"{key}", defaultPrimary, description);
var secondary = defaultSecondary.HasValue
    ? configFile.Bind(section, $"{key}Secondary", defaultSecondary.Value, description)
    : null;
```

The secondary key lives in the same BepInEx section with a `Secondary` suffix so it is human-readable in the config file and automatically persisted by BepInEx.

### Reflecting keybind changes back to config

`KeybindRow` renders two capture buttons: one for `Primary` and, when `Secondary` is non-null, one for `Secondary`. When the user clicks a button and presses a new key:

1. The row enters capture mode and consumes the next key-down event.
2. It sets the matching `ConfigEntry<KeyCode>.Value` to the captured `KeyCode`.
3. It calls `ConfigFile.Save()` so the change is written to disk immediately.
4. It refreshes the button text using a friendly key name.

Because the `ConfigEntry` is the source of truth, the row also subscribes to `ConfigEntry.SettingChanged` so external edits (manual config file changes or other mods calling `config.Value = ...`) update the UI immediately.

### Input dispatch

`InputDispatcher` polls `KeybindEntry.Primary.Value` and `KeybindEntry.Secondary?.Value` each frame. Only the active key state matters; the callback does not care which entry triggered it.

## Public API

```csharp
public static class SettingsMenu
{
    // Generic scalar / enum / string setting. Creates the ConfigEntry and registers the UI row.
    public static ConfigEntry<T> Register<T>(
        ConfigFile configFile,
        string section,
        string key,
        T defaultValue,
        string description,
        string label,
        string tab,
        string group,
        string? subGroup = null,
        Action<T>? onValueChanged = null,
        AcceptableValueBase? acceptableValues = null);

    // Register an already-created ConfigEntry.
    public static ConfigEntry<T> Register<T>(
        ConfigEntry<T> config,
        string label,
        string tab,
        string group,
        string? subGroup = null,
        Action<T>? onValueChanged = null);

    // Dual-key keybind registration. Secondary key is optional.
    public static KeybindRegistration RegisterKeybind(
        ConfigFile configFile,
        string section,
        string key,
        KeyCode defaultPrimary,
        string description,
        string label,
        string tab,
        string group,
        string? subGroup = null,
        KeyCode? defaultSecondary = null,
        Action? onPressed = null,
        Action? onReleased = null);

    // Keybind registration from existing ConfigEntries. Secondary can be null.
    public static KeybindRegistration RegisterKeybind(
        ConfigEntry<KeyCode> primary,
        ConfigEntry<KeyCode>? secondary,
        string label,
        string tab,
        string group,
        string? subGroup = null,
        Action? onPressed = null,
        Action? onReleased = null);
}
```

- `tab` may be a native tab name (`Game Settings`, `Mouse/Keyboard`, `Video`, `Audio`, `Controller`) or a custom name that becomes a new tab.
- `group` is normally the mod name; one group header is created per mod per tab.
- `subGroup` is optional and rendered with a visually different header.

## Runtime Architecture

```
SettingsLibPlugin (BasePlugin)
  Load()
    -> Harmony("zeprus.gunfire.settingslib").PatchAll()
    -> AddComponent<SettingsMenuManager>()

SettingsMenuManager (MonoBehaviour)
  Awake()  -> create PanelTracker, SettingsRegistry, InputDispatcher, UIFinder,
              TabManager, RowFactory, SettingsInjector, ToastManager, RebuildCoordinator;
              subscribe events
  Update() -> PanelTracker tick; UIFinder.Initialize if needed; TabManager.OnUIReady;
              RebuildCoordinator.TryRebuild; SettingsInjector.UpdateRows();
              SettingsPanelState.IsCapturing = injector.IsCapturing;
              TabManager.ValidateActiveTab; InputDispatcher.Poll(); ToastManager.Update
  OnDestroy() -> unsubscribe events; PanelTracker.Reset()
  OnPanelOpened()  -> RebuildCoordinator.RequestRebuild()
  OnPanelClosed()  -> SettingsPanelState.IsBlockingClose = false; SettingsInjector.Clear();
                      TabManager.OnPanelClosed(); UIFinder.Reset(); PanelTracker.Reset()
  OnEntryRegistered() -> RebuildCoordinator.RequestRebuild()

Core
  SettingsRegistry      -> register + query ISettingEntry; lazy Default fallback
  SettingsPanelState    -> shared static state: IsCapturing, IsBlockingClose,
                           KeybindPanelRefreshed event
  PanelTracker          -> tracks whether PC_Panel_setting is open via SettingsPanelStateListener
  RebuildCoordinator    -> debounces and triggers panel rebuilds
  InputDispatcher       -> polls keybinds, suppresses callbacks in menus
  TransformFinder       -> safe hierarchy search helper (avoids Il2Cpp `foreach` on `Transform`)
  SettingsPanelStateListener -> IL2CPP MonoBehaviour on the panel that raises open/closed events

UI
  UIFinder              -> lazy cached transforms / components by path; style capture
  StyleCatalog          -> aggregates all captured sub-styles; orchestrates CaptureFrom(panelRoot)
  TextAppearance        -> shared value object: font, material, fontSize, fontSizeMin/Max,
                           color, alignment, fontStyle, outlineWidth, wordWrapping,
                           autoSizing, overflowMode; From(TextMeshProUGUI, fallbackFontSize) factory
  TextAppearanceApplier -> shared helper that applies a TextAppearance to a TextMeshProUGUI
  PanelLocator          -> walks up the hierarchy to find PC_Panel_setting from a child transform
  RowStyle              -> Title TextAppearance + background sprite/color/type + highlight color +
                           description text + layout dims (Layout nested class + Row/Title/Item RectData)
  TabStyle              -> two TextAppearance instances (Selected/Unselected) + width/height +
                           selected background sprite/RectData + click sound Wwise event ID
  GroupHeaderStyle      -> TextAppearance Header + spacing
  KeybindButtonStyle    -> Bound TextAppearance + unbound TextAppearance + TMP sprite asset +
                           background color/sprite/type + Button ColorBlock +
                           primary/secondary/item RectData + click sound Wwise event ID
  ToggleStyle           -> Toggle colors/sprite/state transition colors + graphic RectData +
                           click sound Wwise event ID
  SliderStyle           -> Slider colors + fill/background sprites + handle RectData +
                           wholeNumbers default + click/change sound Wwise event ID
  DropdownStyle         -> Caption TextAppearance + background color/sprite/type + arrow sprite +
                           item/template/scrollbar sub-styles + click sound Wwise event ID
  InputStyle            -> Text TextAppearance + caret selection color + background color/sprite/type +
                           placeholder TextAppearance + click sound Wwise event ID
  RowFactory            -> maps SettingType to RowType; resolves captured audio IDs; delegates to Builders then Controllers
  TabManager            -> native/custom tab lifecycle; M1Toggle + M1ToggleGroup integration;
                           delegates creation/scrolling to Tabs/ components and destroys/recreates
                           custom tab GameObjects on each panel session
  TabBarController      -> orchestrates custom tab bar viewport, sizing, scroll, and activation
  TabBarLayout          -> shared helper for tab bar edge/paging calculations
  GroupBuilder          -> creates and caches group containers and sub-group headers
  SettingsInjector      -> orchestrates UI construction; delegates group/sub-group layout to GroupBuilder
  ToastManager          -> displays and hides the vanilla keybind toast
  WwiseAudio            -> helper for posting captured Wwise event IDs; reads AkEvent.data.Id from vanilla controls

UI/Tabs (custom tab bar support)
  CustomTabRegistry     -> owns all custom tab state by name
  CustomTabFactory    -> creates M1Toggle tab buttons and content panels
  NativeTabResolver   -> maps native tab names to existing vanilla tab objects
  TabActivationController -> toggles custom/native tab content visibility
  TabBarViewportFactory -> creates and parents the masked SL_TabViewport
  TabBarSizeAdjuster    -> measures tab widths and resizes the tab bar container
  TabBarScrollController -> translates the active tab into view and clamps to first/last tab
  TabBarController      -> coordinates the above during tab activation

UI/Builders (pure GameObject construction, no logic or data binding)
  RowElementBuilder     -> shared helpers: CreateRowRoot, CreateTitle, CreateItem,
                           CreateKeybindButton, CreateObject, SetRect, AddText, AddImage
                           + Metrics constants (layout dimensions, control colors)
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
  DropdownRow           -> enum / acceptable-value list; cached onSelectionChanged delegate; posts Wwise audio
  InputFieldRow         -> string (TMP_InputField); cached onEndEdit delegate; posts Wwise audio on focus
  RowHoverHandler       -> MonoBehaviour on each row root; tints background Image and writes
                           the setting description to the vanilla setting_desc/desc text

Patches
  KeyBoardPanelManagerPatch     -> Postfix on ShowShowItem to raise SettingsPanelState.KeybindPanelRefreshed
  ClosePanelBlockerPatch        -> Prefix on settings-panel back buttons; blocks when capturing or blocking close
```

## UI Integration

### Panel lifecycle

A `SettingsPanelStateListener` MonoBehaviour in `Core` is added to `PC_Panel_setting` once it is discovered. Its `OnEnable` / `OnDisable` events are wired by `PanelTracker` to set `IsOpen` and notify `SettingsMenuManager` to inject or tear down mod rows.

`KeyBoardPanelManagerPatch` is a safety net: when the native keybind panel refreshes, a Postfix on `ShowItem`/`ShowShowItem` raises `SettingsPanelState.KeybindPanelRefreshed` so `SettingsMenuManager` can request a rebuild.

### Row construction

`RowFactory` determines the `RowType` from `ISettingEntry.Config.SettingType` and `AcceptableValues`, then delegates to the appropriate builder in `UI/Builders/`. Builders are pure construction — they create GameObjects, set RectTransforms, and attach Unity components, but contain no `ConfigEntry` or `ISettingEntry` references. The resulting `Transform` is passed to the matching controller in `UI/Controllers/`, which handles two-way data binding, event subscription, and value syncing.

`RowElementBuilder` provides shared helpers (`CreateRowRoot`, `CreateTitle`, `CreateItem`, `CreateKeybindButton`, `CreateObject`, `SetRect`, `AddText`, `AddImage`) and the `Metrics` nested class with named constants for layout dimensions and control colors. `AddText` takes a `TextAppearance` as the single source of truth for font, material, size, color, and outline. Each per-element builder receives only the sub-style it needs (e.g. `RowStyle` for most builders, `KeybindButtonStyle` for keybind buttons, `TabStyle` for tab buttons), keeping construction code DRY and decoupled.

| RowType | Builder | Controller | Config types |
|---------|---------|------------|--------------|
| `Keybind` | `KeybindElementBuilder` | `KeybindRow` | `KeyCode`, `KeybindEntry` |
| `Slider` | `SliderElementBuilder` | `SliderRow` | `int`, `float` with `AcceptableValueRange` |
| `Toggle` | `ToggleElementBuilder` | `ToggleRow` | `bool` |
| `Dropdown` | `DropdownElementBuilder` | `DropdownRow` | `enum`, `AcceptableValueList` |
| `InputField` | `InputElementBuilder` | `InputFieldRow` | `string`, `int`/`float` without range |

### Keybind row behavior

`KeybindRow` renders one or two capture buttons using `KeybindButtonStyle`. Bound keys are displayed as `<sprite name="sck{KeyCodeValue}">` with the vanilla `Sck_json` sprite asset and the captured bound-text font/material. Unbound keys display `None` with the vanilla unbound style (bold `NotoSerifSC-Black SDF` / `BlackNOoutline` material, auto-sized). The actual keybind state machine lives in `KeybindCaptureController`, and the vanilla `covery_mask` is managed by `KeybindCoverMask`. During capture the mask disables its `KeyboardSwapPanel_Logic`, shows the setting label and an `Esc` prompt, and temporarily disables the back button so `Esc` unbinds the key instead of closing the menu. After a successful bind/unbind, `SettingsMenuManager` uses `ToastManager` to show the vanilla `changebutton_tips/1` toast for two seconds.

### Tab and group layout

`TabManager` resolves `SettingLocation.Tab` to the vanilla content panels:

- `Game Settings` -> `Content_1`
- `Mouse/Keyboard` -> `Content_2`
- `Video` -> `Content_3`
- `Audio` -> `Content_4`
- `Controller` -> `Content_5`
- any unmatched name -> custom tab

Under each content panel, `SettingsInjector` creates one group header per `(Tab, Group)`. If a `SubGroup` is present, a smaller, differently colored sub-header is inserted under the group header and rows are placed under it. Row order inside a group is the registration order.

### Custom Tabs

A custom tab is a settings tab that behaves and looks like the vanilla tabs in `tab_switch`. There is one custom tab per registered mod that requests one, and it contains only the settings registered by that mod.

- Custom tabs must visually and functionally mirror vanilla tabs.
- Custom tab state, builders, and GameObjects are contained in their own package/namespace and are managed exclusively by `TabManager`. Other systems request content through `TabManager.GetContentForTab` and never interact with custom tab GameObjects directly.
- Custom tab buttons are `M1Toggle` instances created by `TabButtonBuilder` and registered with the vanilla `M1ToggleGroup` on `tab_switch`.
- Each custom tab has its own `Content_SL_<name>` panel under the `setting_scroll/Viewport`. `TabManager` activates the matching panel and sets the `ScrollRect.content` when a custom tab is selected.
- The `M1ToggleGroup` handles left/right navigation between all registered tabs. Custom tabs are appended after the vanilla tabs.
- `TabBarScroller` creates a masked `SL_TabViewport` around `tab_switch`, reparents the tab bar into it, and translates the bar so the active tab stays visible while clamping to the first/last tab. It detects and unwraps any pre-existing viewport wrapper from a previous panel session to avoid nested viewports.
- `TabManager` destroys all custom tab GameObjects and clears its internal caches when the panel closes, then recreates the tabs from `SettingsRegistry` each time the panel reopens. This keeps references valid whether the panel is disabled or fully recreated.
- Custom content panels are inserted just before the vanilla `covery_mask` so the keybind-capture overlay renders on top of them.
- `TabStyle.Capture` looks up `tab_switch` by name through `TransformFinder` so it still succeeds after the tab bar has been reparented into `SL_TabViewport`.

## Input Dispatch

`InputDispatcher` runs in `SettingsMenuManager.Update`:

1. If `PanelTracker.IsOpen` is true, skip all callbacks.
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
└── src/
    ├── Plugin.cs
    ├── SettingsMenuManager.cs
    ├── Public/
    │   ├── SettingsMenu.cs
    │   ├── SettingsTab.cs
    │   └── KeybindRegistration.cs
    ├── Core/
    │   ├── ISettingEntry.cs
    │   ├── IKeybindEntry.cs
    │   ├── SettingEntry.cs
    │   ├── KeybindEntry.cs
    │   ├── SettingLocation.cs
    │   ├── SettingsRegistry.cs
    │   ├── InputDispatcher.cs
    │   ├── PanelTracker.cs
    │   ├── RebuildCoordinator.cs
    │   ├── SettingsPanelState.cs
    │   ├── SettingsPanelStateListener.cs
    │   └── TransformFinder.cs
    ├── UI/
    │   ├── RowFactory.cs
    │   ├── RowType.cs
    │   ├── RowTypeResolver.cs
    │   ├── AcceptableValueResolver.cs
    │   ├── SettingsInjector.cs
    │   ├── TabManager.cs
    │   ├── TabBarLayout.cs
    │   ├── UIFinder.cs
    │   ├── GroupBuilder.cs
    │   ├── ToastManager.cs
    │   ├── TextAppearanceApplier.cs
    │   ├── PanelLocator.cs
    │   ├── WwiseAudio.cs
    │   ├── IKeybindRowServices.cs
    │   ├── Tabs/
    │   │   ├── CustomTab.cs
    │   │   ├── CustomTabRegistry.cs
    │   │   ├── CustomTabFactory.cs
    │   │   ├── NativeTabResolver.cs
    │   │   ├── TabActivationController.cs
    │   │   ├── TabBarController.cs
    │   │   ├── TabBarScrollController.cs
    │   │   ├── TabBarSizeAdjuster.cs
    │   │   └── TabBarViewportFactory.cs
    │   ├── Styles/
    │   │   ├── StyleCatalog.cs
    │   │   ├── TextAppearance.cs
    │   │   ├── RectData.cs
    │   │   ├── RowStyle.cs
    │   │   ├── TabStyle.cs
    │   │   ├── GroupHeaderStyle.cs
    │   │   ├── KeybindButtonStyle.cs
    │   │   ├── InputStyle.cs
    │   │   ├── DropdownStyle.cs
    │   │   ├── DropdownItemStyle.cs
    │   │   ├── DropdownTemplateStyle.cs
    │   │   ├── DropdownScrollbarStyle.cs
    │   │   ├── SliderStyle.cs
    │   │   └── ToggleStyle.cs
    │   ├── Builders/
    │   │   ├── RowElementBuilder.cs
    │   │   ├── SliderElementBuilder.cs
    │   │   ├── ToggleElementBuilder.cs
    │   │   ├── KeybindElementBuilder.cs
    │   │   ├── DropdownElementBuilder.cs
    │   │   ├── InputElementBuilder.cs
    │   │   ├── TabButtonBuilder.cs
    │   │   ├── GroupContainerBuilder.cs
    │   │   └── VanillaComponentApplier.cs
    │   └── Controllers/
    │       ├── ISettingRow.cs
    │       ├── SettingRowBase.cs
    │       ├── KeybindRow.cs
    │       ├── KeybindCaptureController.cs
    │       ├── KeybindCoverMask.cs
    │       ├── SliderRow.cs
    │       ├── ToggleRow.cs
    │       ├── DropdownRow.cs
    │       ├── InputFieldRow.cs
    │       └── RowHoverHandler.cs
    └── Patches/
        ├── KeyBoardPanelManagerPatch.cs
        └── ClosePanelBlockerPatch.cs
```

## Technology Stack

- **Target framework**: `net6.0`
- **BepInEx**: `BepInEx.Unity.IL2CPP` 6.0.0-be.* with `BepInEx.PluginInfoProps`
- **Patching**: HarmonyX
- **Interop references**: `Assembly-CSharp`, `UnityEngine.CoreModule`, `UnityEngine.UI`, `UnityEngine.InputModule`, `Unity.TextMeshPro`, `Il2Cppmscorlib`
- **Language features**: C# 10 records, `init` properties, nullable reference types, `required` where appropriate, `switch` expressions for tab/group mapping

## Implementation Order

1. **Skeleton**: `SettingsLib.csproj`, `Plugin.cs`, `SettingsMenuManager`.
2. **Domain**: `SettingLocation`, `ISettingEntry`, `SettingEntry<T>`, `KeybindEntry`, `SettingsRegistry`.
3. **Public API**: `SettingsMenu` + `SettingsTab` enum + `KeybindRegistration`.
4. **Lifecycle tracking**: `PanelTracker` + `SettingsPanelStateListener` + `KeyBoardPanelManagerPatch`.
5. **UI discovery**: `UIFinder` + `StyleCatalog` (with sub-style records: `TextAppearance`, `RowStyle`, `TabStyle`, `GroupHeaderStyle`, `KeybindButtonStyle`).
6. **Row construction**: `RowElementBuilder` shared helpers + per-element builders.
7. **Row controllers**: `SettingRowBase`, `KeybindRow`, `SliderRow`, `ToggleRow`, `DropdownRow`, `InputFieldRow`.
8. **Row wiring**: `RowFactory` maps `SettingType` to builder + controller.
9. **Tabs and groups**: `TabManager` native tab mapping and custom tab handling. Custom tab implementation is contained in its own package/namespace and managed exclusively by `TabManager`.
10. **Orchestration**: `SettingsInjector` builds the full layout.
11. **Input**: `InputDispatcher`.
12. **Integration**: Example consumer mods depend on `SettingsLib` and register via `SettingsMenu`.

## Risks

- Custom tab switching integrates with `M1Toggle`/`M1ToggleGroup` for native left/right navigation and mutual exclusivity. `TabBarScrollController` keeps the active tab visible while respecting tab bar boundaries, and `TabManager` detects when native code re-activates a vanilla content panel and tears down custom tab state.
- `TMP_InputField` for strings needs to handle Chinese input and native on-screen keyboard gracefully; fallback is a simple `TMP_InputField` with `EndEdit` event.
- `Register<T>` with `enum` requires converting the selected dropdown index back to the enum value; `AcceptableValueList` and `AcceptableValueRange` must be inspected at registration time.
- `PanelTracker.TryLocatePanel` relies on hardcoded hierarchy paths (`CUIManager/Canvas_PC(Clone)/SettingRoot/PC_Panel_setting`); game updates that rename nodes will produce warning logs but silent failure until paths are updated.
