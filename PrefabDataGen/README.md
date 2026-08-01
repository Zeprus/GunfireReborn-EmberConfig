# EmberConfig PrefabDataGen

This is a build-time developer tool. It parses dumped Unity prefabs from an AssetRipper export and writes generated C# style records into the mod's `src/Generated/PrefabData/` folder.

The mod does **not** ship the ripped assets; only the generated `.cs` files are checked in. The generator is run whenever the prefab data needs to be refreshed.

## What you need

- .NET 6 SDK or later
- An `ExportedProject` directory produced by AssetRipper. Its `Assets` folder should contain the vanilla setting prefabs, e.g.

```
<YourRippedAssets>/ExportedProject/Assets/res/uisteam/panel_prefabs/setting/PC_Panel_setting.prefab
```

Replace `<YourRippedAssets>/ExportedProject` below with the actual path.

## Build

From the `Mods/EmberConfig` directory:

```bash
dotnet build PrefabDataGen/PrefabDataGen.csproj
```

## Run

From the `Mods/EmberConfig` directory, pass two arguments:

1. The path to the `ExportedProject` root (the directory that contains `Assets`).
2. The path to the output directory where generated `.cs` files should be written.

```bash
dotnet run --project PrefabDataGen/PrefabDataGen.csproj -- \
  "<YourRippedAssets>/ExportedProject" \
  "EmberConfig/src/Generated/PrefabData"
```

The default output location is `EmberConfig/src/Generated/PrefabData` and is already excluded from `EmberConfig/EmberConfig.csproj` for files that should not be compiled.

## What it does

1. **Loads `PC_Panel_setting.prefab`** from the ripped prefabs.
2. **Parses the Unity YAML** into an in-memory object model (`GameObjectNode`, `ComponentNode`, etc.).
3. **Resolves asset GUIDs** to human-readable names by scanning `*.asset`, `*.mat`, `*.png`, `*.jpg`, `*.tga`, and `*.psd` files and reading their sibling `*.meta` files (sprites, fonts, and materials).
4. **Extracts a representative row and tab** from the panel prefab, and extracts dropdown, switch, slider, keybind, carousel, and input from their respective prefabs.
5. **Emits the generated style factories** into `EmberConfig/src/Generated/PrefabData/`:
   - `RowStyleFactory.cs`
   - `DropdownStyleFactory.cs`
   - `SwitchStyleFactory.cs`
   - `SliderStyleFactory.cs`
   - `KeybindButtonStyleFactory.cs`
   - `CarouselStyleFactory.cs`
   - `InputStyleFactory.cs`
   - `TabStyleFactory.cs`

## Inspecting the output

After running, check the generated factory files in `EmberConfig/src/Generated/PrefabData/`:

- `RowStyleFactory.cs` — the row background, title text, and layout.
- `DropdownStyleFactory.cs` — the dropdown item, template, and scrollbar.
- `SwitchStyleFactory.cs` — the toggle group and option look.
- `SliderStyleFactory.cs` — the slider background, fill, handle, and number text.
- `KeybindButtonStyleFactory.cs` — the keybind primary/secondary text and layout.
- `CarouselStyleFactory.cs` — the carousel arrows and dot group.
- `InputStyleFactory.cs` — the fallback input field built from row text.
- `TabStyleFactory.cs` — the selected and unselected tab text and background.

You can compare the generated values against the source prefab YAML by searching for the row GameObject whose children are named `Title` and `Item`.

## Adding a new control

To add a new control style (e.g. `Switch`, `Slider`, `Dropdown`):

1. Add an `Extraction/*StyleExtractor.cs` class in `PrefabDataGen/Extraction/` and compose it from the shared helpers in that folder (rect, text, sprite, color-block, predicates, etc.).
2. Add a `Models/*RawStyle.cs` record.
3. Add a `Write*StyleFactory` method in `PrefabDataGen/Generation/CSharpFileWriter.cs` that builds the argument list and calls `CSharpCodeBuilder` to emit the factory.
4. Add a `StyleJob<TStyle>` entry in `PrefabDataGen/Generation/Generator.cs` that loads the prefab (when one exists), calls the new extractor, and writes the output.
5. Wire the runtime `*Style` record and `*ElementBuilder` to consume the generated data.

See `RowStyleExtractor.cs` for the current reference implementation.

## Troubleshooting

- **"AssetRips path not found"** — make sure you are passing the directory that contains `Assets`, not the `Assets` directory itself.
- **"Could not find a row with 'Title' and 'Item' children"** — verify that `PC_Panel_setting.prefab` exists and contains at least one GameObject with both a `Title` and `Item` child.
- **Asset names resolve to `null`** — check that the `*.meta` files for sprites, fonts, and materials are present next to their `*.asset`, `*.mat`, `*.png`, `*.jpg`, `*.tga`, or `*.psd` files under the `Assets` tree.
