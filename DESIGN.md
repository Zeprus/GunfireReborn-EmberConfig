# EmberConfig Design and Model

A clean, standalone BepInEx 6 plugin that exposes a generic `SettingsMenu` API for every BepInEx config type, treats keybinds as a dual-key specialization, and injects per-mod grouped settings rows into the native Gunfire Reborn `PC_Panel_setting` UI using native templates, native tabs, custom tabs created on demand, and style data extracted from dumped `AssetRips` prefabs.

## Project Goals

1. **Seamless integration**: Modded settings must be visually and functionally indistinguishable from vanilla settings. This means matching fonts, colors, sizes, layout, navigation, and interaction patterns exactly. A user should not be able to tell which settings are modded and which are vanilla.
2. **No vanilla savefile poisoning**: All mod setting values are stored exclusively in BepInEx `ConfigFile` entries (per-mod `.cfg` files under `BepInEx/config/`). The mod never writes to, modifies, or interferes with the game's native save system. This is the highest priority invariant — any change that risks touching vanilla save data is rejected.
3. **Native parity via generated prefab data and property capture, not cloning**: Custom tabs, rows, group headers, and toggles are built from scratch as new GameObjects. Pure data properties (fonts, colors, font sizes, outline widths, sprite references, RectTransform dimensions) come primarily from `PrefabDataGen`-generated `*StyleFactory` classes built from dumped `AssetRips` prefabs, with a small set of runtime-captured values via `RowStyleCapture`, `GroupHeaderStyleCapture` and `DropdownStyleCapture`. This hybrid approach ensures visual parity without the risks of cloning vanilla GameObjects, which can inherit serialized references to vanilla save/persistence systems and cause savefile poisoning. Only data-only properties are copied — never components with logic, events, or callbacks. Custom tabs use `M1Toggle` (not `Button`) and register with the vanilla `M1ToggleGroup` for native navigation.

## Core Principles

- **Composition over inheritance**: domain objects share behavior through interfaces and records; UI row controllers are thin and specialized.
- **Nullable reference types enabled**; no underscore-prefixed identifiers.
- **HarmonyX patches are triggers only**: real UI logic lives in injectors and controllers.
- **Native parity**: fonts, colors, layout, and sprites from the native settings menu are extracted from dumped prefabs and captured at runtime, then reused. All custom UI elements are built from scratch — never cloned from vanilla GameObjects — to avoid inheriting serialized references to vanilla persistence/save systems.
- **No vanilla savefile poisoning**: all mod config values live in BepInEx `ConfigFile` entries; the mod never touches the game's native save system.
- **Edge-triggered keybind dispatch**: callbacks fire once on key-down, and are suppressed while the settings menu is open or a keybind row is awaiting input.

## Domain Model

```csharp
public readonly record struct SettingLocation(string Tab, string? Group, string? SubGroup = null);

public enum SettingControlStyle
{
    Auto,
    Switch,
    Dropdown,
    Carousel,
}

public interface ISettingEntry
{
    string Id { get; }
    ConfigEntryBase Config { get; }
    string Label { get; }
    string Description => Config.Description?.Description ?? string.Empty;
    SettingLocation Location { get; }
    SettingControlStyle ControlStyle { get; }
    SwitchLabels? SwitchLabels { get; }
    event Action? ValueChanged;
}

public sealed class SettingEntry<T> : ISettingEntry
{
    public string Id { get; }
    public ConfigEntry<T> Config { get; }
    public string Label { get; }
    public SettingLocation Location { get; }
    public SettingControlStyle ControlStyle { get; }
    public SwitchLabels? SwitchLabels { get; }
    public Action<T>? OnValueChanged { get; }
    public event Action? ValueChanged;

    ConfigEntryBase ISettingEntry.Config => Config;
}

public interface IKeybindEntry
{
    string Label { get; }
    int PrimaryKeyCodeValue { get; }
    int? SecondaryKeyCodeValue { get; }
    Action? OnPressed { get; }
    Action? OnReleased { get; }
}

public sealed class KeybindEntry : ISettingEntry, IKeybindEntry
{
    public string Id { get; }
    public ConfigEntry<KeyCode> Primary { get; }
    public ConfigEntry<KeyCode>? Secondary { get; }
    public string Label { get; }
    public SettingLocation Location { get; }
    public SettingControlStyle ControlStyle { get; } = SettingControlStyle.Auto;
    public SwitchLabels? SwitchLabels { get; } = null;
    public Action? OnPressed { get; }
    public Action? OnReleased { get; }
    public event Action? ValueChanged;

    public int PrimaryKeyCodeValue => (int)Primary.Value;
    public int? SecondaryKeyCodeValue =>
        Secondary is { Value: not KeyCode.None } secondary ? (int)secondary.Value : null;

    ConfigEntryBase ISettingEntry.Config => Primary;
}

public sealed record KeybindRegistration(ConfigEntry<KeyCode> Primary, ConfigEntry<KeyCode>? Secondary);
```

- `SettingLocation` is the value object that decides tab, optional group header, and optional sub-group styling. `Group` is `null` when the mod wants rows placed directly under the tab content.
- `SettingsRegistry` owns the authoritative list of `ISettingEntry` instances and provides lookups by `Tab` (`GetByTab`) plus `GetTabs` and `GetKeybindEntries`.
- `ISettingEntry` is intentionally non-generic so keybinds and scalar settings coexist in the same registry.
- `IKeybindEntry` is the input-dispatch view of a keybind; it exposes integer `KeyCode` values so `InputDispatcher` can poll without touching `BepInEx.Configuration`.

## Public API

The public API is in the `EmberConfig.Public` namespace and is expressed through `SettingOptions<T>` and `KeybindOptions` records.

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
    string? Group,
    string? SubGroup = null,
    Action<T>? OnValueChanged = null,
    AcceptableValueBase? AcceptableValues = null,
    SettingControlStyle ControlStyle = SettingControlStyle.Auto,
    SwitchLabels? SwitchLabels = null)
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
    string? Group,
    string? SubGroup = null,
    KeyCode? DefaultSecondary = null,
    Action? OnPressed = null,
    Action? OnReleased = null)
{
    public KeybindOptions WithTab(SettingsTab tab) => this with { Tab = tab.ToNativeName() };
}
```

- `tab` may be a native tab name (`Game Settings`, `Mouse/Keyboard`, `Video`, `Audio`, `Controller`) or a custom name that becomes a new tab. Use `WithTab(SettingsTab)` for the enum values.
- `group` is normally the mod name; one group header is created per mod per tab. It can be `null`, in which case rows are placed directly under the tab content with no group header.
- `subGroup` is optional and rendered with a visually different header when `group` is non-empty.
- `ControlStyle` lets the mod author pick the control used for a setting: `Auto` chooses based on the value type (`Switch` for `bool`, `Dropdown`/`Carousel` for enums/lists), `Switch` is the boolean style, `Dropdown` and `Carousel` are the two list styles, and `Slider` is used for numeric ranges. Incompatible styles (e.g. `Dropdown` on a `bool`, `Switch` on an enum) are ignored and logged as a warning; the resolver falls back to `Auto`.
- `SwitchLabels` lets the mod author override the `On`/`Off` text shown by a `Switch` control. The default is `On`/`Off` regardless of the captured vanilla labels.

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

`InputDispatcher` is constructed with a `Func<IEnumerable<IKeybindEntry>>` and optional `Func<int, bool>` delegates for key-down/key-up testing. Each frame it iterates the registered `IKeybindEntry` instances:

1. If the settings panel is open, or if `SettingsPanelState.IsCapturing` is `true`, all callbacks are skipped.
2. It checks `Input.GetKeyDown` against `PrimaryKeyCodeValue` and `SecondaryKeyCodeValue` (both treated as `KeyCode` integers; `0`/`None` is ignored).
3. On key-down, it invokes `OnPressed` once, wrapping the call in a try/catch and logging any exception.
4. If `OnReleased` is set, it invokes it once on `Input.GetKeyUp` for either key.

This is edge-triggered and single-frame, so keybinds do not fire while held and do not fire while the settings menu is open or a keybind row is awaiting capture.

## Runtime Architecture

```
Plugin (BasePlugin)
  Load()
    -> SettingsRegistry.Current = new SettingsRegistry()
    -> RegisterEmberConfigSettings()              // self-tuning settings for tab scroll/width/etc.
    -> AddComponent<SettingsMenuManager>()

SettingsMenuManager (MonoBehaviour)
  Awake()  -> register IL2CPP types (SettingsPanelStateListener, RowHoverHandler,
              DropdownCaptionGuard);
              create PanelTracker, SettingsRegistry, UIFinder, TabManager,
              RowFactory, SettingsInjector, InputDispatcher, RebuildCoordinator;
              subscribe events; on critical failure log and disable
  Update() -> TrackPanel
              InitializeUIIfNeeded
              RebuildIfRequested
              ContinueBuildIfNeeded          // drive batched row construction
              UpdateRowsAndState
              ValidateTabState
              PollInputAndToast
  OnDestroy() -> unsubscribe events; reset state
  OnPanelOpened()  -> RequestRebuild
  OnPanelClosed()  -> SettingsPanelState.IsBlockingClose = false; injector.Clear();
                      tabManager.OnPanelClosed(); uiFinder.Reset(); panelTracker.Reset()
  OnEntryRegistered() -> RequestRebuild
  OnKeybindPanelRefreshed() -> RequestRebuild
  ShowKeybindToast() -> toastManager.Show(...)

Core
  EmberConfigSettings     // static mod settings for tab scroll/width/font/animation
  SettingsRegistry      -> register + query ISettingEntry; get tabs and keybinds
  VisibilityStore       -> per-mod/tab visibility toggles; creates sentinel SettingEntry<bool> rows; persists in [Visibility]
  SettingLocation       -> tab/optional group/optional sub-group value object
  SettingsPanelState    -> shared static state: IsCapturing, IsBlockingClose,
                           KeybindPanelRefreshed event
  PanelTracker          -> locates PC_Panel_setting (GameObject.Find, then active scene);
                           attaches SettingsPanelStateListener; raises Opened/Closed
  RebuildCoordinator    -> debounces and triggers panel rebuilds with a 3-frame delay
  InputDispatcher       -> polls IKeybindEntry instances, suppresses callbacks in menus
  TransformFinder       -> safe hierarchy search helper (avoids Il2Cpp foreach on Transform)
  SettingsPanelStateListener -> IL2CPP MonoBehaviour on the panel that raises open/closed events

UI
  StyleFactoryController -> creates the StyleCatalog from generated prefab data + runtime captures
  UIResources           -> 1x1 white Sprite for raycast targets that have no visible image
  UIStyleConstants      -> shared visual constants (e.g. destructive ColorBlock) used across UI components

UI/Services
  UIFinder              -> lazy cached transforms / components by path; creates StyleFactoryController
  TextAppearanceApplier -> shared helper that applies a TextAppearance to a TextMeshProUGUI
  PanelLocator          -> walks up the hierarchy to find PC_Panel_setting from a child transform
  ToastManager          -> displays and hides the vanilla keybind toast
  WwiseAudio            -> helper for posting captured Wwise event IDs
  IKeybindRowServices   -> interface the manager implements so keybind rows can show toasts
  NumberParser          -> parses numeric input with both invariant and current-culture fallbacks (e.g. "0.5" and "0,5")
  ScrollPreserver       -> captures and restores ScrollRect position across rebuilds and visibility toggles

UI/Resolvers
  RowTypeResolver       -> maps ISettingEntry to RowType (Switch, Slider, Dropdown, Carousel,
                           InputField, Keybind)
  AcceptableValueResolver -> extracts lists and ranges from AcceptableValueBase via reflection
  OptionResolver        -> resolves the list of values a dropdown/carousel row should display
  MaterialResolver      -> resolves TMP material assets by name
  SpriteResolver        -> resolves Sprite assets by name
  TMP_FontAssetResolver -> resolves TMP_FontAsset by name
  TMP_SpriteAssetResolver -> resolves TMP_SpriteAsset by name

UI/Managers
  TabManager            -> native/custom tab lifecycle; M1Toggle integration; delegates to Tabs/ components
  SettingsInjector      -> batched group/sub-group/row construction; calls RowFactory, GroupBuilder and ResetButtonBuilder; handles targeted visibility refreshes

UI/Factories
  RowFactory            -> maps SettingType/ControlStyle to RowType; resolves audio IDs; builds Transform
                           and matching controller

UI/Builders (pure GameObject construction, no logic or data binding)
  RowElementBuilder     -> shared helpers: CreateRowRoot, CreateTitle, CreateItem,
                           CreateKeybindButton, CreateObject, SetRect, AddText, AddImage
                           + Metrics constants (layout dimensions, control colors)
  GroupBuilder          -> creates and caches group containers and sub-group headers
  GroupContainerBuilder -> Build(name, title, RowStyle, GroupHeaderStyle, parent) -> Transform
  SliderElementBuilder  -> Build(name, RowStyle, SliderStyle, parent) -> Transform
  SwitchElementBuilder  -> Build(name, RowStyle, SwitchStyle, parent) -> Transform
  KeybindElementBuilder -> Build(name, RowStyle, KeybindButtonStyle, parent) -> Transform
  DropdownElementBuilder -> Build(name, RowStyle, DropdownStyle, parent) -> Transform
  CarouselElementBuilder -> Build(name, RowStyle, CarouselStyle, parent) -> Transform
  InputElementBuilder   -> Build(name, RowStyle, InputStyle?, parent) -> Transform
  TabButtonBuilder      -> custom tab button construction; consumed by CustomTabFactory
  VanillaComponentApplier -> attaches DYSelect, AkGameObj, and AkTriggerMouseClick to controls
  ResetButtonBuilder    -> builds and maintains the "Reset Visibility" button/spacer in the EmberConfig tab

UI/Controllers (row controllers with ConfigEntry two-way binding, implement ISettingRow)
  ISettingRow           -> Transform, Bind, Unbind, Refresh, Update, UpdateHover,
                           IsHovered, Description, IsCapturing
  SettingRowBase        -> Bind/Unbind/Refresh lifecycle; SetValue helper for consistent BoxedValue persistence and ConfigFile.Save
  KeybindRow            -> binds a KeybindEntry; uses KeybindCaptureController and KeybindCoverMask
  KeybindCaptureController -> polls input and drives the WaitRelease/WaitPress/ReleaseNextFrame state machine
  KeybindCoverMask      -> manages the vanilla covery_mask during keybind capture
  SliderRow             -> float/int range; cached onSliderChanged delegate; posts Wwise audio throttled
  SwitchRow             -> bool On/Off two-option toggle; posts Wwise audio on change
  DropdownRow           -> enum / acceptable-value list; uses OptionResolver; posts Wwise audio
  DropdownCaptionGuard  -> IL2CPP MonoBehaviour; refreshes TMP_Dropdown caption on Enable/Start
  CarouselRow           -> left/right option carousel with dot indicators; posts Wwise audio
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
  TabBarScrollEasing    -> pure math helpers for scroll duration/easing; unit-testable
  TabBarVisuals         -> selected/unselected visual sync
  TabBarNavigator       -> NavigateNext/Previous, wrapping, arrow button audio
  TabBarLayout          -> shared helper for tab bar edge/paging/wrap calculations

UI
  StyleFactoryController -> creates the StyleCatalog from generated prefab data + runtime captures
  UIResources           -> 1x1 white Sprite for raycast targets that have no visible image
  UIStyleConstants      -> shared visual constants (e.g. destructive ColorBlock) used across UI components
  StyleCatalog          -> data container for Row, Tab, GroupHeader, KeybindButton, Slider, Switch,
                           Dropdown, Carousel and Input styles
  TextAppearance        -> shared value object: font, material, fontSize, fontSizeMin/Max,
                           color, alignment, fontStyle, outlineWidth, wordWrapping,
                           autoSizing, overflowMode; From(TextMeshProUGUI, fallbackFontSize) factory
  RectData              -> anchor/pivot/size/position for captured RectTransforms
  *StyleCapture         -> runtime capture helpers for values not present in dumped prefabs
                           (RowStyleCapture, GroupHeaderStyleCapture, DropdownStyleCapture)
  *Style                -> readonly record structs that hold pure data (Row, Tab, GroupHeader,
                           KeybindButton, Switch, Slider, Dropdown, Carousel, Input)

Generated/PrefabData
  *StyleFactory         -> auto-generated from AssetRips dumped prefabs (Row, Tab, GroupHeader,
                           KeybindButton, Slider, Switch, Dropdown, Carousel, Input)

Patches
  KeyBoardPanelManagerPatch          -> Postfix on ShowShowItem; raises KeybindPanelRefreshed
  ClosePanelBlockerPatch             -> Prefix on OnBack methods; blocks when capturing/blocking close
  TMP_DropdownOnPointerClickPatch    -> Prefix on OnPointerClick; toggles dropdown open/closed
  TMP_DropdownShowPatch              -> Postfix on Show; reparents dropdown list to root canvas
```

## Style System

Visual parity is achieved by a hybrid system: the majority of style data is extracted from the dumped `AssetRips` prefabs and emitted as generated `*StyleFactory` classes, while a small set of values that cannot be hard-coded from the dump is captured at runtime from the live vanilla panel.

`StyleFactoryController.Create(panelRoot)` is the single entry point for building a `StyleCatalog`.

1. Generated `*StyleFactory` classes in `EmberConfig/src/Generated/PrefabData/` produce the canonical `RowStyle`, `TabStyle`, `KeybindButtonStyle`, `SliderStyle`, `SwitchStyle`, `DropdownStyle`, `CarouselStyle` and `InputStyle`. They are regenerated by the `PrefabDataGen` console app whenever the `AssetRips` dump is updated.
2. Runtime `*StyleCapture` classes (`RowStyleCapture`, `GroupHeaderStyleCapture`, `DropdownStyleCapture`) fill in values that must come from the live hierarchy (e.g. the vanilla description text component, the group header layout, and the actual dropdown template at runtime).
3. `StyleCatalog` is a plain data container. It is passed to `UIFinder` after the settings panel is discovered and consumed by `RowFactory` and the element builders. No components with logic, events, or callbacks are copied.

`TextAppearance` is a shared value object used across all style records. `RectData` captures anchor, pivot, size, and position for layout values. The generated factories reference runtime assets by name, using `MaterialResolver`, `SpriteResolver`, `TMP_FontAssetResolver` and `TMP_SpriteAssetResolver` to locate the actual Unity objects.

Style data is read-only: builders and controllers inspect the style records but never mutate them.

## UI Integration

### Panel lifecycle

`PanelTracker` first tries `GameObject.Find("PC_Panel_setting")`, then falls back to scanning the active scene, with a short cooldown between attempts. Once the panel is found, a `SettingsPanelStateListener` (an IL2CPP-registered `MonoBehaviour`) is added or reattached. Its `OnEnable` / `OnDisable` events are wired by `PanelTracker` to set `IsOpen` and notify `SettingsMenuManager` to inject or tear down mod rows.

`KeyBoardPanelManagerPatch` is a safety net: when the native keybind panel refreshes, a Postfix on `ShowShowItem` raises `SettingsPanelState.KeybindPanelRefreshed` so `SettingsMenuManager` can request a rebuild.

### Row construction

`SettingsInjector.StartRebuild` iterates over registered tabs in display order, prioritizing the currently active tab. For each tab it asks `TabManager.GetOrCreateContentForTab` for a content panel, sorts the entries by group (optional), sub-group (optional), and registration order, then enqueues one `BuildJob` per entry.

`SettingsInjector.BuildNextBatch` processes jobs over multiple frames with a 4ms per-frame budget and a minimum of one row per frame. This prevents large mod settings lists from stalling the settings panel opening animation. `SettingsMenuManager.ContinueBuildIfNeeded` drives this on every `Update`.

`GroupBuilder` creates the group and optional sub-group containers; when `Group` is `null` or empty, rows are placed directly under the tab content. `RowFactory.CreateRow` determines the `RowType` from `ISettingEntry.Config.SettingType`, `AcceptableValues`, and the requested `ControlStyle`, then delegates to the appropriate builder in `UI/Builders/`. Builders are pure construction — they create GameObjects, set RectTransforms, and attach Unity components, but contain no `ConfigEntry` or `ISettingEntry` references. The resulting `Transform` is passed to the matching controller in `UI/Controllers/`, which handles two-way data binding, event subscription, and value syncing.

`RowElementBuilder` provides shared helpers (`CreateRowRoot`, `CreateTitle`, `CreateItem`, `CreateKeybindButton`, `CreateObject`, `SetRect`, `AddText`, `AddImage`) and the `Metrics` nested class with named constants for layout dimensions and control colors. `AddText` takes a `TextAppearance` as the single source of truth for the text component's appearance.

`RowHoverHandler` is a registered IL2CPP `MonoBehaviour` on each row root. It detects pointer hover, tints the row's background `Image`, and writes the setting's description to the vanilla `setting_desc/desc` `TextMeshProUGUI`. `SettingsInjector.UpdateRows` polls all rows' hover state and updates the description text exactly once per frame.

### Tab construction

A custom tab is a settings tab that behaves and looks like the vanilla tabs in `tab_switch`. Any distinct non-native tab name gets its own custom tab.

- Custom tabs visually and functionally mirror vanilla tabs.
- Custom tab state, builders, and GameObjects are managed by `TabManager`. Other systems request content through `TabManager.GetOrCreateContentForTab` and never interact with custom tab GameObjects directly.
- Custom tab buttons are `M1Toggle` instances created by `TabButtonBuilder` and registered with the vanilla `M1ToggleGroup` on `tab_switch`.
- Each custom tab has its own `Content_SL_<name>` panel under `setting_scroll/Viewport`. `TabActivationController` activates the matching panel and sets `ScrollRect.content` when a custom tab is selected.
- The `M1ToggleGroup` handles left/right navigation between all registered tabs. Native tabs are kept in their original order; custom tabs are appended after them, sorted alphabetically.
- `TabBarViewportFactory` creates a masked `SL_TabViewport` around `tab_switch`. `TabBarView` reparents the tab bar into it and refreshes sizing each session.
- `TabBarScrollAnimator` translates the tab bar so the active tab stays visible while clamping to the first/last tab. `TabBarNavigator` drives `NavigateNext`/`NavigatePrevious` with wrap-around. Scroll behavior is tuned by `EmberConfigSettings`: `TabScrollSensitivity`, `TabWidthScaling`, `TabScrollAnimationDuration` and `TabMinFontSize`. Far tabs take up to twice the base scroll duration (`TabBarScrollEasing`).
- `TabManager` destroys all custom tab GameObjects and clears its internal caches when the panel closes, then recreates the tabs from `SettingsRegistry` each time the panel reopens. This keeps references valid whether the panel is disabled or fully recreated.
- Custom content panels are inserted just before the vanilla `covery_mask` so the keybind-capture overlay renders on top of them.
- `TabStyle` now comes from the generated `TabStyleFactory` with runtime lookups by name through `TransformFinder`, so it still succeeds after the tab bar has been reparented into `SL_TabViewport`.

### Input Dispatch

`InputDispatcher.Poll` runs inside `SettingsMenuManager.PollInputAndToast`:

1. If the settings panel is open, or if `SettingsPanelState.IsCapturing` is `true`, skip all callbacks.
2. For each registered `IKeybindEntry`, check `Input.GetKeyDown` against the integer `PrimaryKeyCodeValue` and `SecondaryKeyCodeValue`.
3. On key-down, invoke `OnPressed` once.
4. If `OnReleased` is set, invoke it once on `Input.GetKeyUp`.

This is edge-triggered and single-frame, so keybinds do not fire while held and do not fire while the settings menu is open or a keybind is being captured.

## Project Layout

```
EmberConfig/
├── Directory.Build.props
├── README.md
├── DESIGN.md
├── EmberConfig/
│   ├── EmberConfig.csproj
│   ├── EmberConfig.Tests/
│   │   └── src/
│   │       ├── AcceptableValueResolverTests.cs
│   │       ├── EmberConfigSettingsTests.cs
│   │       ├── InputDispatcherTests.cs
│   │       ├── NativeTabResolverTests.cs
│   │       ├── OptionResolverTests.cs
│   │       ├── RowTypeResolverTests.cs
│   │       ├── SettingLocationTests.cs
│   │       ├── SettingsRegistryTests.cs
│   │       ├── SwitchLabelsTests.cs
│   │       ├── TabBarLayoutTests.cs
│   │       └── TabBarScrollEasingTests.cs
│   ├── src/
│   │   ├── Plugin.cs
│   │   ├── SettingsMenuManager.cs
│   │   ├── Public/
│   │   │   ├── SettingsMenu.cs
│   │   │   ├── SettingOptions.cs
│   │   │   ├── KeybindOptions.cs
│   │   │   ├── SettingsTab.cs
│   │   │   ├── KeybindRegistration.cs
│   │   │   ├── SettingControlStyle.cs
│   │   │   └── SwitchLabels.cs
│   │   ├── Core/
│   │   │   ├── EmberConfigSettings.cs
│   │   │   ├── Entries/
│   │   │   │   ├── ISettingEntry.cs
│   │   │   │   ├── IKeybindEntry.cs
│   │   │   │   ├── SettingEntry.cs
│   │   │   │   └── KeybindEntry.cs
│   │   │   └── State/
│   │   │       ├── SettingLocation.cs
│   │   │       ├── SettingsRegistry.cs
│   │   │       ├── InputDispatcher.cs
│   │   │       ├── PanelTracker.cs
│   │   │       ├── RebuildCoordinator.cs
│   │   │       ├── SettingsPanelState.cs
│   │   │       ├── SettingsPanelStateListener.cs
│   │   │       ├── TransformFinder.cs
│   │   │       └── VisibilityStore.cs
│   │   ├── UI/
│   │   │   ├── UIResources.cs
│   │   │   ├── UIStyleConstants.cs
│   │   │   ├── StyleFactoryController.cs
│   │   │   ├── Resolvers/
│   │   │   │   ├── RowTypeResolver.cs
│   │   │   │   ├── AcceptableValueResolver.cs
│   │   │   │   ├── OptionResolver.cs
│   │   │   │   ├── MaterialResolver.cs
│   │   │   │   ├── SpriteResolver.cs
│   │   │   │   ├── TMP_FontAssetResolver.cs
│   │   │   │   └── TMP_SpriteAssetResolver.cs
│   │   │   ├── Services/
│   │   │   │   ├── UIFinder.cs
│   │   │   │   ├── TextAppearanceApplier.cs
│   │   │   │   ├── PanelLocator.cs
│   │   │   │   ├── ToastManager.cs
│   │   │   │   ├── WwiseAudio.cs
│   │   │   │   ├── IKeybindRowServices.cs
│   │   │   │   ├── NumberParser.cs
│   │   │   │   └── ScrollPreserver.cs
│   │   │   ├── Managers/
│   │   │   │   ├── TabManager.cs
│   │   │   │   └── SettingsInjector.cs
│   │   │   ├── Factories/
│   │   │   │   └── RowFactory.cs
│   │   │   ├── Models/
│   │   │   │   └── RowType.cs
│   │   │   ├── Builders/
│   │   │   │   ├── RowElementBuilder.cs
│   │   │   │   ├── GroupBuilder.cs
│   │   │   │   ├── GroupContainerBuilder.cs
│   │   │   │   ├── SliderElementBuilder.cs
│   │   │   │   ├── SwitchElementBuilder.cs
│   │   │   │   ├── KeybindElementBuilder.cs
│   │   │   │   ├── DropdownElementBuilder.cs
│   │   │   │   ├── CarouselElementBuilder.cs
│   │   │   │   ├── InputElementBuilder.cs
│   │   │   │   ├── TabButtonBuilder.cs
│   │   │   │   ├── VanillaComponentApplier.cs
│   │   │   │   └── ResetButtonBuilder.cs
│   │   │   ├── Controllers/
│   │   │   │   ├── ISettingRow.cs
│   │   │   │   ├── SettingRowBase.cs
│   │   │   │   ├── KeybindRow.cs
│   │   │   │   ├── KeybindCaptureController.cs
│   │   │   │   ├── KeybindCoverMask.cs
│   │   │   │   ├── SliderRow.cs
│   │   │   │   ├── SwitchRow.cs
│   │   │   │   ├── DropdownRow.cs
│   │   │   │   ├── DropdownCaptionGuard.cs
│   │   │   │   ├── CarouselRow.cs
│   │   │   │   ├── InputFieldRow.cs
│   │   │   │   └── RowHoverHandler.cs
│   │   │   ├── Styles/
│   │   │   │   ├── StyleCatalog.cs
│   │   │   │   ├── TextAppearance.cs
│   │   │   │   ├── RectData.cs
│   │   │   │   ├── RowStyle.cs
│   │   │   │   ├── RowStyleCapture.cs
│   │   │   │   ├── TabStyle.cs
│   │   │   │   ├── GroupHeaderStyle.cs
│   │   │   │   ├── GroupHeaderStyleCapture.cs
│   │   │   │   ├── KeybindButtonStyle.cs
│   │   │   │   ├── SwitchStyle.cs
│   │   │   │   ├── SliderStyle.cs
│   │   │   │   ├── DropdownStyle.cs
│   │   │   │   ├── DropdownStyleCapture.cs
│   │   │   │   ├── DropdownItemStyle.cs
│   │   │   │   ├── DropdownTemplateStyle.cs
│   │   │   │   ├── DropdownScrollbarStyle.cs
│   │   │   │   ├── CarouselStyle.cs
│   │   │   │   └── InputStyle.cs
│   │   │   └── Tabs/
│   │   │       ├── CustomTab.cs
│   │   │       ├── CustomTabRegistry.cs
│   │   │       ├── CustomTabFactory.cs
│   │   │       ├── NativeTabResolver.cs
│   │   │       ├── TabActivationController.cs
│   │   │       ├── TabBarController.cs
│   │   │       ├── TabBarLayout.cs
│   │   │       ├── TabBarNavigator.cs
│   │   │       ├── TabBarScrollAnimator.cs
│   │   │       ├── TabBarScrollEasing.cs
│   │   │       ├── TabBarView.cs
│   │   │       ├── TabBarViewportFactory.cs
│   │   │       ├── TabBarVisuals.cs
│   │   │       └── TabButtonCollection.cs
│   │   ├── Generated/
│   │   │   └── PrefabData/
│   │   │       ├── RowStyleFactory.cs
│   │   │       ├── TabStyleFactory.cs
│   │   │       ├── GroupHeaderStyleFactory.cs
│   │   │       ├── KeybindButtonStyleFactory.cs
│   │   │       ├── SwitchStyleFactory.cs
│   │   │       ├── SliderStyleFactory.cs
│   │   │       ├── DropdownStyleFactory.cs
│   │   │       ├── CarouselStyleFactory.cs
│   │   │       └── InputStyleFactory.cs
│   │   └── Patches/
│   │       ├── KeyBoardPanelManagerPatch.cs
│   │       ├── ClosePanelBlockerPatch.cs
│   │       ├── TMP_DropdownOnPointerClickPatch.cs
│   │       └── TMP_DropdownShowPatch.cs
├── ExampleMod/
│   ├── ExampleMod.csproj
│   ├── README.md
│   └── src/
│       ├── Plugin.cs
│       └── ExampleConfiguration.cs
├── PrefabDataGen/
│   ├── PrefabDataGen.csproj
│   ├── PrefabDataGen.Tests/
│   │   └── src/
│   │       ├── AssetNameResolverTests.cs
│   │       ├── ComponentNodeTests.cs
│   │       ├── GameObjectNodeTests.cs
│   │       └── YamlParsersTests.cs
│   ├── Extraction/
│   ├── Generation/
│   ├── Parsing/
│   ├── Resolution/
│   └── README.md
```

## Technology Stack

- **Target framework**: `net6.0` (`LangVersion=latest`)
- **BepInEx**: `BepInEx.Unity.IL2CPP` 6.0.0-be.* with `BepInEx.PluginInfoProps`
- **Patching**: HarmonyX
- **Interop references**: `Assembly-CSharp`, `UnityEngine.CoreModule`, `UnityEngine.UI`, `UnityEngine.UIModule`, `UnityEngine.InputModule`, `UnityEngine.TextRenderingModule`, `Unity.TextMeshPro`, `Il2Cppmscorlib`
- **Language features**: C# 12+ records, `init` properties, nullable reference types, `required` where appropriate, `switch` expressions for tab/group mapping, `is`/`or` patterns

## Risks

- Custom tab switching integrates with `M1Toggle`/`M1ToggleGroup` for native left/right navigation and mutual exclusivity. `TabBarScrollAnimator` keeps the active tab visible while respecting tab bar boundaries, and `TabManager` detects when native code re-activates a vanilla content panel and tears down custom tab state. Tab behavior is also exposed as mod-configurable settings (`EmberConfigSettings`) so users can tune scroll sensitivity, width scaling, animation duration and minimum font size.
- `TMP_InputField` for strings needs to handle Chinese input and native on-screen keyboard gracefully; fallback is a simple `TMP_InputField` with `EndEdit` event.
- `Register<T>` with `enum` requires converting the selected dropdown/carousel index back to the enum value; `AcceptableValueList` and `AcceptableValueRange` are inspected at registration time.
- `PanelTracker` tries `GameObject.Find("PC_Panel_setting")` first and falls back to scanning the active scene. If the settings panel is renamed or moved out of the active scene, it will silently fail until the search logic is updated.
- Visual parity is split between generated `PrefabDataGen` style factories and the live `*StyleCapture` classes. If the vanilla prefabs or hierarchy change, the generated files must be regenerated and the capture classes may need updates before any rows can be rendered.
- `TMP_Dropdown` popup lists are reparented to the root canvas and their nested `Canvas`/`GraphicRaycaster` components are destroyed. This works around nested-canvas rendering issues but may behave differently if the vanilla dropdown prefab changes significantly.
