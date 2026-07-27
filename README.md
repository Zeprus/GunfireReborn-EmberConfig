# SettingsLib

A standalone BepInEx 6 plugin for Gunfire Reborn that exposes a generic `SettingsMenu` API for injecting per-mod settings rows into the native in-game settings panel. Modded settings are visually indistinguishable from vanilla — matching fonts, colors, sizes, layout, and navigation.

## Features

- **Native visual parity**: Fonts, colors, font sizes, sprites, and layout dimensions are captured at runtime from vanilla UI elements via `StyleCatalog` and sub-style records. No hardcoded assets.
- **All BepInEx config types supported**: bool (toggle), int/float with range (slider), enum (dropdown), string (input field), KeyCode (dual-key keybind).
- **Custom tabs**: Unmatched tab names create new tabs using the native `M1Toggle`/`M1ToggleGroup` system and a dedicated content panel under the vanilla `Viewport`.
- **Group and sub-group headers**: Settings are organized by group (typically mod name) with optional sub-group headers.
- **Row hover highlight and description**: Modded rows tint their background and show a description in the vanilla `setting_desc/desc` field on hover, matching vanilla behavior.
- **Vanilla-style keybinds**: Bound keys render as TMP `<sprite>` tags using the vanilla `Sck_json` sprite asset; `Esc` unbinds; capture shows the vanilla cover mask and a rebind toast.
- **No vanilla savefile poisoning**: All values stored exclusively in BepInEx `ConfigFile` entries. The game's native save system is never touched.
- **Vanilla-style controller navigation**: Modded rows carry `DYSelect` components so the DYControl navigation system can select them, and interactive controls include `AkGameObj`/`AkTriggerMouseClick` for pointer-click audio triggers.
- **Per-control Wwise audio parity**: Toggles, sliders, dropdowns, input fields, keybind buttons, and tab buttons capture the vanilla `AkEvent` ID and post it through `AkSoundEngine.PostEvent` on the correct lifecycle point. Each control falls back to the captured tab click sound when no control-specific ID is found.
- **Edge-triggered keybind dispatch**: Callbacks fire once on key-down, suppressed while the settings menu is open.
- **Custom tab viewport scroller**: Overflowing tabs are clipped inside a masked `SL_TabViewport`; the active tab is translated into view and clamped to the first/last tab.
- **Robust panel relaunch**: Custom tab buttons and content panels are destroyed when the settings panel closes and rebuilt fresh on reopen, so the UI stays consistent after closing and reopening the menu.
- **Keybind cover mask on custom tabs**: Custom content panels are inserted before the vanilla `covery_mask`, so the "press any key" overlay appears on top during keybind capture.

## Requirements

- Gunfire Reborn (Steam)
- BepInEx 6 (Unity IL2CPP) already installed

## Installation

Build the project and copy `SettingsLib.dll` to `BepInEx/plugins/`. The build target auto-deploys if `GameDir` in the `.csproj` points to your game installation. If the game is running, build with `-p:DeployToPlugins=false` and copy the DLL manually, or restart the game so the locked file can be replaced.

## Soft dependency for consumers

If your mod uses `SettingsLib`, add a soft dependency so BepInEx loads `SettingsLib` before your plugin:

```csharp
using System.Linq;

[BepInPlugin("your.mod.guid", "Your Mod", "1.0.0")]
[BepInDependency("zeprus.gunfire.settingslib", BepInDependency.DependencyFlags.SoftDependency)]
public class YourModPlugin : BasePlugin
{
    public override void Load()
    {
        var settingsLibAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(a.GetName().Name, "SettingsLib", StringComparison.OrdinalIgnoreCase));

        if (settingsLibAssembly is null)
            return;

        // SettingsMenu.Register(...);
    }
}
```

## Public API

### Register a scalar setting

```csharp
var config = Config.Bind("MyMod", "Volume", 0.8f,
    new ConfigDescription("Master volume", new AcceptableValueRange<float>(0f, 1f)));

SettingsMenu.Register(config,
    label: "Master Volume",
    tab: SettingsTab.Audio,
    group: "My Mod",
    onValueChanged: v => AudioListener.volume = v);
```

A custom tab name can still be passed as a string:

```csharp
SettingsMenu.Register(config, label: "My Setting", tab: "My Mod", group: "General");
```

### Register a keybind

```csharp
SettingsMenu.RegisterKeybind(Config,
    section: "MyMod",
    key: "ToggleOverlay",
    defaultPrimary: KeyCode.F8,
    description: "Toggle the overlay",
    label: "Toggle Overlay",
    tab: SettingsTab.MouseKeyboard,
    group: "My Mod",
    onPressed: () => overlay.SetActive(!overlay.activeSelf));
```

## Architecture

```
SettingsLibPlugin (BasePlugin)
  -> SettingsMenuManager (MonoBehaviour)
      UIFinder            -> locates vanilla panel, captures StyleCatalog
      StyleCatalog        -> aggregates sub-styles from vanilla UI:
                            RowStyle, TabStyle, GroupHeaderStyle, KeybindButtonStyle,
                            InputStyle, SliderStyle, ToggleStyle, DropdownStyle
      TabManager          -> native/custom tab lifecycle via M1Toggle; delegates to Tabs/ helpers
      RowFactory          -> maps config types to builders + controllers
      GroupBuilder        -> creates and caches group/sub-group headers
      SettingsInjector    -> orchestrates group/sub-group/row construction
      ToastManager        -> vanilla keybind toast
      RebuildCoordinator  -> debounced panel rebuilds
      InputDispatcher     -> edge-triggered keybind polling
      WwiseAudio          -> helper for posting captured Wwise event IDs

Core
  SettingsRegistry          -> register + query ISettingEntry
  SettingsPanelState        -> shared static state for patches and UI controllers
  PanelTracker              -> tracks whether PC_Panel_setting is open
  SettingsPanelStateListener -> IL2CPP MonoBehaviour that reports panel open/closed
```

### Style system

Style capture is modular — each UI element has its own focused record:

| Record | Captures |
|--------|----------|
| `TextAppearance` | Font, material, fontSize, fontSizeMin/Max, color, alignment, fontStyle, outlineWidth, wrapping, autoSizing, overflowMode |
| `RowStyle` | Title TextAppearance + background sprite/color/type + highlight color + description text + layout dims |
| `TabStyle` | Selected/Unselected TextAppearance + width/height + selected background sprite/RectData + click sound event ID |
| `GroupHeaderStyle` | Header TextAppearance + spacing |
| `KeybindButtonStyle` | Bound TextAppearance + unbound TextAppearance + TMP sprite asset + background color/sprite/type + Button ColorBlock + primary/secondary/item RectData + click sound event ID |
| `ToggleStyle` | Toggle colors/sprite/state transition colors + graphic RectData + click sound event ID |
| `SliderStyle` | Slider colors + fill/background sprites + handle RectData + wholeNumbers default + click/change sound event ID |
| `DropdownStyle` | Caption TextAppearance + background color/sprite/type + arrow sprite + item/template/scrollbar sub-styles + click sound event ID |
| `DropdownItemStyle` | Item background/highlight colors, text TextAppearance, padding, font sizes |
| `DropdownTemplateStyle` | Template RectTransform + background/highlight colors, scroll sensitivity |
| `DropdownScrollbarStyle` | Scrollbar colors, sprite, size |
| `InputStyle` | Text TextAppearance + caret selection color + background color/sprite/type + placeholder TextAppearance + click sound event ID |

`StyleCatalog.CaptureFrom(panelRoot)` orchestrates all capture calls, passing `RowStyle.Title` as fallback to ensure valid fonts in IL2CPP. No `Default` instances with null fonts exist.

## Project layout

```
src/
  Plugin.cs
  SettingsMenuManager.cs
  Public/          SettingsMenu, SettingsTab, KeybindRegistration
  Core/            ISettingEntry, IKeybindEntry, SettingEntry, KeybindEntry,
                   SettingLocation, SettingsRegistry, SettingsPanelState,
                   PanelTracker, RebuildCoordinator, SettingsPanelStateListener,
                   InputDispatcher, TransformFinder, ...
  UI/
    SettingsInjector, RowFactory, RowTypeResolver, UIFinder, GroupBuilder,
    ToastManager, PanelLocator, TextAppearanceApplier, TabManager,
    TabBarLayout, RowHoverHandler, WwiseAudio
    Tabs/          CustomTab, CustomTabRegistry, CustomTabFactory,
                   NativeTabResolver, TabActivationController,
                   TabBarController, TabBarScrollController,
                   TabBarSizeAdjuster, TabBarViewportFactory
    Styles/        StyleCatalog, TextAppearance, RowStyle, TabStyle,
                   GroupHeaderStyle, KeybindButtonStyle,
                   ToggleStyle, SliderStyle, DropdownStyle,
                   DropdownItemStyle, DropdownTemplateStyle,
                   DropdownScrollbarStyle, InputStyle, RectData
    Builders/      RowElementBuilder, SliderElementBuilder,
                   ToggleElementBuilder, KeybindElementBuilder,
                   DropdownElementBuilder, InputElementBuilder,
                   TabButtonBuilder, GroupContainerBuilder,
                   VanillaComponentApplier
    Controllers/   ISettingRow, SettingRowBase,
                   KeybindRow, KeybindCaptureController, KeybindCoverMask,
                   SliderRow, ToggleRow, DropdownRow, InputFieldRow
  Patches/         KeyBoardPanelManagerPatch, ClosePanelBlockerPatch
```

See [DESIGN.md](DESIGN.md) for the full design document.
