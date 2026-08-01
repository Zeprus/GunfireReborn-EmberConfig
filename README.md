# EmberConfig

<img src="EmberConfig/branding/icon/emberconfig_icon.png" alt="EmberConfig icon" width="100" />

A standalone BepInEx 6 plugin for Gunfire Reborn that exposes a generic `SettingsMenu` API. Mods can register settings and keybinds that appear inside the native in-game settings panel with vanilla visual and audio parity.

## Features

- **Native visual parity** — fonts, colors, layout, sprites, and sounds match the vanilla settings menu.
- **All common BepInEx config types** — `bool`, `int`/`float` with range, `enum`, `string`, and dual `KeyCode` keybinds.
- **Custom tabs and groups** — organize settings by mod, with optional sub-groups.
- **Hover descriptions** and vanilla-style row highlighting.
- **Dual-key keybinds** rendered with vanilla TMP sprite icons.
- **Soft-dependency friendly** — consumer mods can reference EmberConfig without hard coupling.
- **No vanilla savefile changes** — all values live in per-mod BepInEx `.cfg` files.

## Requirements

- Gunfire Reborn (Steam)
- BepInEx 6 (Unity IL2CPP)

## Installation

Build the project with `dotnet build EmberConfig/EmberConfig.csproj -p:DeployToPlugins=false` (or download the dll from the releases) and copy `EmberConfig.dll` to `BepInEx/plugins/`.
Game paths are read from the `GUNFIRE_REBORN_DIR` environment variable; they are centralized in `Mods/EmberConfig/Directory.Build.props` with a fallback to `E:\SteamLibrary\steamapps\common\Gunfire Reborn`. To override per build, pass `-p:GameDir=<game root>`.

## Dependency (Soft Dependency)

Reference EmberConfig with `BepInDependency.DependencyFlags.SoftDependency` and guard every call that touches EmberConfig behind a method marked with `[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]`.

```csharp
using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.IL2CPP;
using EmberConfig.Public;
using UnityEngine;

[BepInPlugin("your.mod.guid", "Your Mod", "1.0.0")]
[BepInDependency("zeprus.gunfire.EmberConfig", BepInDependency.DependencyFlags.SoftDependency)]
// The BepInEx.PluginInfoProps package (or equivalent source) generates MyPluginInfo
// with the plugin name. Pass MyPluginInfo.PLUGIN_NAME as ModName.
public class YourModPlugin : BasePlugin
{
    public override void Load()
    {
        if (EmberConfigCompatibility.IsLoaded)
            EmberConfigCompatibility.RegisterSettings(Config);
    }
}

public static class EmberConfigCompatibility
{
    private static bool? isLoaded;

    public static bool IsLoaded
    {
        get
        {
            isLoaded ??= IL2CPPChainloader.Instance?.Plugins.ContainsKey("zeprus.gunfire.EmberConfig") ?? false;
            return isLoaded.Value;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void RegisterSettings(ConfigFile config)
    {
        var options = new SettingOptions<float>(
            Section: "MyMod",
            Key: "Volume",
            DefaultValue: 0.8f,
            Description: "Master volume",
            Label: "Master Volume",
            Tab: SettingsTab.Audio.ToNativeName(),
            Group: "My Mod",
            ModName: MyPluginInfo.PLUGIN_NAME);

        SettingsMenu.Register(config, options);
    }
}
```

## Public API

All public API lives in the `EmberConfig.Public` namespace.

- `SettingsMenu.Register<T>(ConfigFile configFile, SettingOptions<T> options)`
- `SettingControlStyle` — choose the control used for a setting (`Auto`, `Switch`, `Dropdown`, `Carousel`).
- `SettingsMenu.Register<T>(ConfigEntry<T> config, SettingOptions<T> options)`
- `SettingsMenu.RegisterKeybind(ConfigFile configFile, KeybindOptions options)`
- `SettingsMenu.RegisterKeybind(ConfigEntry<KeyCode> primary, ConfigEntry<KeyCode>? secondary, KeybindOptions options)`

## Two ways to register

Each registration method comes in two flavors:

- **Pass `ConfigFile`** when you want EmberConfig to create and bind the BepInEx config entry for you.
- **Pass `ConfigEntry`** when your mod already created the config entry itself (for example with `Config.Bind(...)`).

In other words, the `ConfigFile` overloads do both the BepInEx binding and the settings-menu registration, while the `ConfigEntry` overloads only register an existing setting in the menu.

## Code Examples

### Scalar setting

```csharp
using BepInEx.Configuration;
using EmberConfig.Public;
using UnityEngine;

var options = new SettingOptions<float>(
    Section: "MyMod",
    Key: "Volume",
    DefaultValue: 0.8f,
    Description: "Master volume",
    Label: "Master Volume",
    Tab: SettingsTab.Audio.ToNativeName(),
    Group: "My Mod",
    ModName: MyPluginInfo.PLUGIN_NAME,
    AcceptableValues: new AcceptableValueRange<float>(0f, 1f),
    OnValueChanged: v => AudioListener.volume = v);

var config = Config.Bind(options.Section, options.Key, options.DefaultValue,
    new ConfigDescription(options.Description, options.AcceptableValues));

SettingsMenu.Register(config, options);
```

### Setting with a list or enum

```csharp
var dropdownOptions = new SettingOptions<string>(
    Section: "MyMod",
    Key: "Difficulty",
    DefaultValue: "Normal",
    Description: "Gameplay difficulty",
    Label: "Difficulty",
    Tab: SettingsTab.GameSettings.ToNativeName(),
    Group: "My Mod",
    ModName: MyPluginInfo.PLUGIN_NAME,
    AcceptableValues: new AcceptableValueList<string>("Easy", "Normal", "Hard"));

var config = Config.Bind(dropdownOptions.Section, dropdownOptions.Key, dropdownOptions.DefaultValue,
    new ConfigDescription(dropdownOptions.Description, dropdownOptions.AcceptableValues));

SettingsMenu.Register(config, dropdownOptions);
```

### Keybind

```csharp
var keybindOptions = new KeybindOptions(
    Section: "MyMod",
    Key: "ToggleOverlay",
    DefaultPrimary: KeyCode.F8,
    Description: "Toggle the overlay",
    Label: "Toggle Overlay",
    Tab: SettingsTab.MouseKeyboard.ToNativeName(),
    Group: "My Mod",
    ModName: MyPluginInfo.PLUGIN_NAME,
    OnPressed: () => overlay.SetActive(!overlay.activeSelf));

SettingsMenu.RegisterKeybind(Config, keybindOptions);
```

### Custom tab

```csharp
var customOptions = new SettingOptions<bool>(
    Section: "MyMod",
    Key: "ShowFps",
    DefaultValue: true,
    Description: "Show the in-game FPS counter",
    Label: "Show FPS",
    Tab: "My Mod",
    Group: "My Mod",
    ModName: MyPluginInfo.PLUGIN_NAME);

SettingsMenu.Register(Config, customOptions);
```

### Choosing a control style

By default EmberConfig picks the control (`Switch` for booleans, `Dropdown` for enums and lists). You can override this with `ControlStyle`. Incompatible styles (e.g. `Dropdown` on a `bool`) fall back to `Auto`.

For a `Switch` you can also override the `On`/`Off` labels:

```csharp
using EmberConfig.Public;

var switchOptions = new SettingOptions<bool>(
    Section: "MyMod",
    Key: "ShowHints",
    DefaultValue: true,
    Description: "Show hint markers",
    Label: "Show Hints",
    Tab: SettingsTab.GameSettings.ToNativeName(),
    Group: "My Mod",
    ModName: MyPluginInfo.PLUGIN_NAME,
    ControlStyle: SettingControlStyle.Switch,
    SwitchLabels: new SwitchLabels("Show", "Hide"));

SettingsMenu.Register(Config, switchOptions);
```