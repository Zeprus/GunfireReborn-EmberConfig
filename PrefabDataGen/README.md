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
  "src/Generated/PrefabData"
```

The default output location is `src/Generated/PrefabData` and is already excluded from `EmberConfig.csproj` for files that should not be compiled.

## What it does

1. **Loads `PC_Panel_setting.prefab`** from the ripped prefabs.
2. **Parses the Unity YAML** into an in-memory object model (`GameObjectNode`, `ComponentNode`, etc.).
3. **Resolves asset GUIDs** to human-readable names by scanning `.asset.meta` and `.mat.meta` files (sprites, fonts, and materials).
4. **Extracts a representative row** from the prefab: title text appearance, background color/sprite, highlight color, RectTransform data, and so on.
5. **Emits `PrefabStyleFactory.cs`**, which the mod uses at runtime to build `RowStyle` and, in later phases, the other control styles.

## Inspecting the output

After running, check:

- `Mods/EmberConfig/src/Generated/PrefabData/PrefabStyleFactory.cs` — the generated C# that feeds `StyleCatalog.Create`.

You can compare the generated values against the source prefab YAML by searching for the row GameObject whose children are named `Title` and `Item`.

## Adding a new control

To add a new control style (e.g. `Switch`, `Slider`, `Dropdown`):

1. Add an `Extraction/*StyleExtractor.cs` class in `PrefabDataGen/Extraction/`.
2. Add a `Models/*RawStyle.cs` record.
3. Extend `CSharpFileWriter.cs` to emit the style and a factory method.
4. Update `Generator.cs` to call the extractor and write the output.
5. Wire the runtime `*Style` record and `*ElementBuilder` to consume the generated data.

See `RowStyleExtractor.cs` for the current reference implementation.

## Troubleshooting

- **"AssetRips path not found"** — make sure you are passing the directory that contains `Assets`, not the `Assets` directory itself.
- **"Could not find a row with 'Title' and 'Item' children"** — verify that `PC_Panel_setting.prefab` exists and contains at least one GameObject with both a `Title` and `Item` child.
- **Asset names resolve to `null`** — check that the `.meta` files for sprites, fonts, and materials are present under the `Assets` tree.
