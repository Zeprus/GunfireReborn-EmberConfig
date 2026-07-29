# EmberConfig ExampleMod

A sample BepInEx 6 plugin that shows how to register every kind of setting and keybind supported by **EmberConfig**.

## What it demonstrates

- Passing a `ConfigFile` vs an existing `ConfigEntry`.
- Using vanilla tabs (`Game Settings`, `Audio`, `Video`, `Mouse/Keyboard`) and five custom tabs:
  `Example Mod: General`, `Example Mod: Visuals`, `Example Mod: Rendering`,
  `Example Mod: Gameplay`, and `Example Mod: Network & Input`.
- Scalar setting types: `bool`, `int`, `float`, `string`, and `enum`.
- Ranges (`AcceptableValueRange`) and fixed lists (`AcceptableValueList`).
- Groups and sub-groups inside the settings panel.
- Single-key and dual-key keybinds with `OnPressed` / `OnReleased` callbacks.
- `OnValueChanged` callbacks that update Unity state such as volume, frame rate, and example GameObjects.

## Build

From the workspace root:

```bash
dotnet build Mods/EmberConfig/EmberConfig.csproj -p:DeployToPlugins=false
dotnet build Mods/EmberConfig/ExampleMod/ExampleMod.csproj -p:DeployToPlugins=false
```

## Deploy

Copy `Mods/EmberConfig/ExampleMod/bin/Debug/net6.0/EmberConfig.ExampleMod.dll` to `BepInEx/plugins/`, or build with deployment enabled:

```bash
dotnet build Mods/EmberConfig/ExampleMod/ExampleMod.csproj -p:DeployToPlugins=true
```

EmberConfig must also be present in `BepInEx/plugins/`.

## In-game smoke test

1. Launch the game with EmberConfig and ExampleMod deployed.
2. Open Settings.
3. Switch through the five custom tabs (`Example Mod: General`, `Example Mod: Visuals`, `Example Mod: Rendering`, `Example Mod: Gameplay`, `Example Mod: Network & Input`) and verify the groups appear on each.
4. Switch to the vanilla `Audio`, `Video`, `Game Settings`, and `Mouse/Keyboard` tabs and verify the example rows appear.
5. Change a slider, toggle, dropdown, or input value and check `BepInEx/LogOutput.log` for callback messages.
6. Click a keybind capture button, press a key, and verify the vanilla cover mask and toast appear.
