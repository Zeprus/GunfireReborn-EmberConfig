namespace EmberConfig.ExampleMod;

using System;
using BepInEx.Configuration;
using EmberConfig.Public;
using UnityEngine;

/// <summary>
/// Example enum used for the full enum dropdown.
/// </summary>
public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

/// <summary>
/// Example enum used for both the full enum dropdown and a restricted list dropdown.
/// </summary>
public enum QualityLevel
{
    Low,
    Medium,
    High,
    Ultra
}

/// <summary>
/// Demonstrates every public registration pattern supported by EmberConfig.
/// Each private helper method is a self-contained example. Call
/// <see cref="RegisterAll"/> from <see cref="Plugin.Load"/>.
/// </summary>
public static class ExampleConfiguration
{
    /// <summary>
    /// Registers all example settings and keybinds. This is the only public
    /// entry point the plugin needs to call.
    /// </summary>
    /// <param name="config">The BepInEx config file for this mod.</param>
    /// <param name="behaviour">The runtime helper that owns the marker and overlay objects.</param>
    public static void RegisterAll(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));
        if (behaviour is null)
            throw new ArgumentNullException(nameof(behaviour));

        RegisterVanillaConfigFileExamples(config, behaviour);
        RegisterVanillaConfigEntryExamples(config, behaviour);
        RegisterCustomConfigFileExamples(config, behaviour);
        RegisterCustomConfigEntryExamples(config, behaviour);
    }

    private static void RegisterVanillaConfigFileExamples(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        RegisterShowFpsToggle(config, behaviour);
        RegisterMasterVolumeSlider(config, behaviour);
        RegisterFovSlider(config, behaviour);
        RegisterDifficultyDropdown(config, behaviour);
        RegisterToggleOverlayKeybind(config, behaviour);
    }

    private static void RegisterVanillaConfigEntryExamples(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        RegisterMaxFpsInput(config, behaviour);
        RegisterMouseSensitivityInput(config, behaviour);
        RegisterNicknameInput(config, behaviour);
        RegisterLanguageDropdown(config, behaviour);
        RegisterMaxRetriesDropdown(config, behaviour);
    }

    private static void RegisterCustomConfigFileExamples(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        RegisterEnableOverlayToggle(config, behaviour);
        RegisterMusicVolumeSlider(config, behaviour);
        RegisterMaxParticlesSlider(config, behaviour);
        RegisterQualityDropdown(config, behaviour);
        RegisterRenderScaleDropdown(config, behaviour);
        RegisterDifficultyCarousel(config, behaviour);
    }

    private static void RegisterCustomConfigEntryExamples(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        RegisterShowHintsToggle(config, behaviour);
        RegisterGammaInput(config, behaviour);
        RegisterRegionDropdown(config, behaviour);
        RegisterFpsCapSlider(config, behaviour);
        RegisterSprintWalkKeybind(config, behaviour);
    }

    /// <summary>
    /// Example 1: A bool toggle that EmberConfig binds for us.
    /// It lives on the vanilla "Game Settings" tab under the "Visual" group,
    /// and shows or hides the example marker object when toggled.
    /// </summary>
    private static void RegisterShowFpsToggle(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new SettingOptions<bool>(
            Section: "Example",
            Key: "ShowFps",
            DefaultValue: false,
            Description: "Show the example FPS marker object.",
            Label: "Show FPS Marker",
            Tab: SettingsTab.GameSettings.ToNativeName(),
            Group: "Visual",
            OnValueChanged: value => behaviour.SetMarkerVisible(value));

        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 2: A float slider with a range.
    /// This is the easiest way to make a numeric setting: EmberConfig picks up
    /// the AcceptableValueRange and renders a slider on the vanilla "Audio" tab.
    /// Changing it immediately updates the Unity master volume.
    /// </summary>
    private static void RegisterMasterVolumeSlider(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new SettingOptions<float>(
            Section: "Example",
            Key: "MasterVolume",
            DefaultValue: 1.0f,
            Description: "Master volume for the example mod.",
            Label: "Master Volume",
            Tab: SettingsTab.Audio.ToNativeName(),
            Group: "Audio",
            AcceptableValues: new AcceptableValueRange<float>(0f, 1f),
            OnValueChanged: value => AudioListener.volume = value);

        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 3: An integer slider with a range on the vanilla "Video" tab.
    /// This demonstrates that integer settings can also render as sliders.
    /// </summary>
    private static void RegisterFovSlider(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new SettingOptions<int>(
            Section: "Example",
            Key: "FieldOfView",
            DefaultValue: 90,
            Description: "Example field of view setting.",
            Label: "Field of View",
            Tab: SettingsTab.Video.ToNativeName(),
            Group: "Video",
            AcceptableValues: new AcceptableValueRange<int>(60, 120),
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Field of view changed to {value}."));

        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 4: An enum dropdown.
    /// When you pass an enum and no AcceptableValueList, EmberConfig creates a
    /// dropdown containing every value of that enum.
    /// </summary>
    private static void RegisterDifficultyDropdown(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new SettingOptions<Difficulty>(
            Section: "Example",
            Key: "Difficulty",
            DefaultValue: Difficulty.Normal,
            Description: "Example difficulty preset.",
            Label: "Difficulty",
            Tab: SettingsTab.GameSettings.ToNativeName(),
            Group: "Gameplay",
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Difficulty changed to {value}."));

        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 5: A single-key keybind that EmberConfig binds for us.
    /// Pressing the key toggles the example overlay; releasing it logs a message.
    /// </summary>
    private static void RegisterToggleOverlayKeybind(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new KeybindOptions(
            Section: "Example",
            Key: "ToggleOverlay",
            DefaultPrimary: KeyCode.F8,
            Description: "Toggle the example overlay.",
            Label: "Toggle Overlay",
            Tab: SettingsTab.MouseKeyboard.ToNativeName(),
            Group: "Input",
            OnPressed: () => behaviour.ToggleOverlay(),
            OnReleased: () => Plugin.Logger?.LogInfo("Toggle overlay key released."));

        SettingsMenu.RegisterKeybind(config, options);
    }

    /// <summary>
    /// Example 6: An integer input field with no range.
    /// Here we create the ConfigEntry ourselves and ask EmberConfig to register
    /// the UI. This is useful when your mod already has a config entry.
    /// </summary>
    /// <remarks>
    /// The default value and description are duplicated in SettingOptions because
    /// EmberConfig does not re-derive them from the existing ConfigEntry.
    /// </remarks>
    private static void RegisterMaxFpsInput(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var configEntry = config.Bind("Example", "MaxFps", 120, "Example max FPS cap; 0 or negative disables the cap.");

        var options = new SettingOptions<int>(
            Section: "Example",
            Key: "MaxFps",
            DefaultValue: 120,
            Description: "Example max FPS cap; 0 or negative disables the cap.",
            Label: "Max FPS",
            Tab: SettingsTab.GameSettings.ToNativeName(),
            Group: "Gameplay",
            OnValueChanged: value => Application.targetFrameRate = value <= 0 ? -1 : value);

        SettingsMenu.Register(configEntry, options);
    }

    /// <summary>
    /// Example 7: A float input field with no range.
    /// This demonstrates a plain numeric value that the user types in directly.
    /// </summary>
    /// <remarks>
    /// The default value and description are duplicated in SettingOptions because
    /// EmberConfig does not re-derive them from the existing ConfigEntry.
    /// </remarks>
    private static void RegisterMouseSensitivityInput(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var configEntry = config.Bind("Example", "MouseSensitivity", 1.0f, "Example mouse sensitivity multiplier.");

        var options = new SettingOptions<float>(
            Section: "Example",
            Key: "MouseSensitivity",
            DefaultValue: 1.0f,
            Description: "Example mouse sensitivity multiplier.",
            Label: "Mouse Sensitivity",
            Tab: SettingsTab.MouseKeyboard.ToNativeName(),
            Group: "Input",
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Mouse sensitivity set to {value}."));

        SettingsMenu.Register(configEntry, options);
    }

    /// <summary>
    /// Example 8: A free-text string input field.
    /// This is a ConfigEntry example, so the ConfigEntry is created before registration.
    /// </summary>
    /// <remarks>
    /// The default value and description are duplicated in SettingOptions because
    /// EmberConfig does not re-derive them from the existing ConfigEntry.
    /// </remarks>
    private static void RegisterNicknameInput(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var configEntry = config.Bind("Example", "Nickname", "Player", "Example player nickname.");

        var options = new SettingOptions<string>(
            Section: "Example",
            Key: "Nickname",
            DefaultValue: "Player",
            Description: "Example player nickname.",
            Label: "Nickname",
            Tab: SettingsTab.GameSettings.ToNativeName(),
            Group: "Profile",
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Nickname changed to '{value}'."));

        SettingsMenu.Register(configEntry, options);
    }

    /// <summary>
    /// Example 9: A string dropdown built from an AcceptableValueList.
    /// The ConfigEntry is created with the same list so the value is validated
    /// and the dropdown is populated automatically.
    /// </summary>
    /// <remarks>
    /// The default value and description are duplicated in SettingOptions because
    /// EmberConfig does not re-derive them from the existing ConfigEntry.
    /// </remarks>
    private static void RegisterLanguageDropdown(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var acceptableValues = new AcceptableValueList<string>("English", "French", "German", "Japanese", "Spanish");
        var configEntry = config.Bind("Example", "Language", "English", new ConfigDescription("Example language.", acceptableValues));

        var options = new SettingOptions<string>(
            Section: "Example",
            Key: "Language",
            DefaultValue: "English",
            Description: "Example language.",
            Label: "Language",
            Tab: SettingsTab.GameSettings.ToNativeName(),
            Group: "Gameplay",
            AcceptableValues: acceptableValues,
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Language changed to {value}."));

        SettingsMenu.Register(configEntry, options);
    }

    /// <summary>
    /// Example 10: An integer dropdown built from an AcceptableValueList.
    /// This shows that lists are not limited to strings.
    /// </summary>
    /// <remarks>
    /// The default value and description are duplicated in SettingOptions because
    /// EmberConfig does not re-derive them from the existing ConfigEntry.
    /// </remarks>
    private static void RegisterMaxRetriesDropdown(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var acceptableValues = new AcceptableValueList<int>(0, 1, 2, 3, 5, 10);
        var configEntry = config.Bind("Example", "MaxRetries", 3, new ConfigDescription("Example max retry count.", acceptableValues));

        var options = new SettingOptions<int>(
            Section: "Example",
            Key: "MaxRetries",
            DefaultValue: 3,
            Description: "Example max retry count.",
            Label: "Max Retries",
            Tab: SettingsTab.Video.ToNativeName(),
            Group: "Video",
            AcceptableValues: acceptableValues,
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Max retries changed to {value}."));

        SettingsMenu.Register(configEntry, options);
    }

    /// <summary>
    /// Example 11: A bool toggle on a custom tab using the vanilla Switch style.
    /// The tab name "Example Mod: General" does not match a vanilla tab, so
    /// EmberConfig creates a new custom tab at the end of the settings tab bar.
    /// </summary>
    private static void RegisterEnableOverlayToggle(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new SettingOptions<bool>(
            Section: "Example",
            Key: "EnableOverlay",
            DefaultValue: false,
            Description: "Show the example overlay object.",
            Label: "Enable Overlay",
            Tab: "Example Mod: General",
            Group: "General",
            OnValueChanged: value => behaviour.SetOverlayVisible(value));
        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 12: A float slider on the custom "Example Mod: General" tab.
    /// This example logs the value so you can see OnValueChanged firing without
    /// changing any real game state.
    /// </summary>
    private static void RegisterMusicVolumeSlider(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new SettingOptions<float>(
            Section: "Example",
            Key: "MusicVolume",
            DefaultValue: 0.8f,
            Description: "Example music volume (logs on change).",
            Label: "Music Volume",
            Tab: "Example Mod: General",
            Group: "General",
            AcceptableValues: new AcceptableValueRange<float>(0f, 1f),
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Music volume changed to {value}."));

        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 13: A sub-group example.
    /// This slider appears under the custom "Example Mod: Visuals" tab, inside
    /// the "Visuals" group, and under the "Rendering" sub-group header.
    /// </summary>
    private static void RegisterMaxParticlesSlider(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new SettingOptions<int>(
            Section: "Example",
            Key: "MaxParticles",
            DefaultValue: 1000,
            Description: "Example maximum particle count.",
            Label: "Max Particles",
            Tab: "Example Mod: Visuals",
            Group: "Visuals",
            SubGroup: "Rendering",
            AcceptableValues: new AcceptableValueRange<int>(0, 10000),
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Max particles changed to {value}."));

        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 14: An enum dropdown on the custom "Example Mod: Visuals" tab.
    /// The QualityLevel enum values are shown as dropdown options.
    /// </summary>
    private static void RegisterQualityDropdown(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new SettingOptions<QualityLevel>(
            Section: "Example",
            Key: "Quality",
            DefaultValue: QualityLevel.Medium,
            Description: "Example overall quality level.",
            Label: "Quality",
            Tab: "Example Mod: Visuals",
            Group: "Visuals",
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Quality changed to {value}."));

        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 15: A float dropdown built from an AcceptableValueList.
    /// This demonstrates that fixed lists also work for floating-point values
    /// on the custom "Example Mod: Rendering" tab.
    /// </summary>
    private static void RegisterRenderScaleDropdown(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var acceptableValues = new AcceptableValueList<float>(0.5f, 0.75f, 1.0f, 1.25f, 1.5f);
        var options = new SettingOptions<float>(
            Section: "Example",
            Key: "RenderScale",
            DefaultValue: 1.0f,
            Description: "Example render scale preset.",
            Label: "Render Scale",
            Tab: "Example Mod: Rendering",
            Group: "Rendering",
            AcceptableValues: acceptableValues,
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Render scale changed to {value}."));

        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 16: An enum rendered as a carousel using MutiClickGroup style.
    /// This uses the same Difficulty enum as the dropdown but requests the
    /// <see cref="SettingControlStyle.Carousel"/> visual style.
    /// </summary>
    private static void RegisterDifficultyCarousel(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var options = new SettingOptions<Difficulty>(
            Section: "Example",
            Key: "DifficultyCarousel",
            DefaultValue: Difficulty.Normal,
            Description: "Example difficulty selector rendered as a carousel.",
            Label: "Difficulty Carousel",
            Tab: "Example Mod: Gameplay",
            Group: "Gameplay",
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Difficulty carousel changed to {value}."),
            ControlStyle: SettingControlStyle.Carousel);

        SettingsMenu.Register(config, options);
    }

    /// <summary>
    /// Example 17: A bool toggle with a sub-group on the custom
    /// "Example Mod: Gameplay" tab. It is created from an existing ConfigEntry
    /// and controls the marker object.
    /// </summary>
    /// <remarks>
    /// The default value and description are duplicated in SettingOptions because
    /// EmberConfig does not re-derive them from the existing ConfigEntry.
    /// </remarks>
    private static void RegisterShowHintsToggle(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var configEntry = config.Bind("Example", "ShowHints", true, "Show the example hint marker.");

        var options = new SettingOptions<bool>(
            Section: "Example",
            Key: "ShowHints",
            DefaultValue: true,
            Description: "Show the example hint marker.",
            Label: "Show Hints",
            Tab: "Example Mod: Gameplay",
            Group: "Gameplay",
            SubGroup: "UI",
            OnValueChanged: value => behaviour.SetMarkerVisible(value),
            ControlStyle: SettingControlStyle.Switch,
            SwitchLabels: new SwitchLabels("Show", "Hide"));

        SettingsMenu.Register(configEntry, options);
    }

    /// <summary>
    /// Example 18: A float input field with no range on the custom
    /// "Example Mod: Rendering" tab. This demonstrates a plain text box for a
    /// decimal value.
    /// </summary>
    /// <remarks>
    /// The default value and description are duplicated in SettingOptions because
    /// EmberConfig does not re-derive them from the existing ConfigEntry.
    /// </remarks>
    private static void RegisterGammaInput(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var configEntry = config.Bind("Example", "Gamma", 1.0f, "Example gamma correction value.");

        var options = new SettingOptions<float>(
            Section: "Example",
            Key: "Gamma",
            DefaultValue: 1.0f,
            Description: "Example gamma correction value.",
            Label: "Gamma",
            Tab: "Example Mod: Rendering",
            Group: "Rendering",
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Gamma changed to {value}."));

        SettingsMenu.Register(configEntry, options);
    }

    /// <summary>
    /// Example 19: A string dropdown from a ConfigEntry on the custom
    /// "Example Mod: Network and Input" tab. The list is passed both to
    /// Config.Bind (for validation) and to SettingOptions.
    /// </summary>
    /// <remarks>
    /// The default value and description are duplicated in SettingOptions because
    /// EmberConfig does not re-derive them from the existing ConfigEntry.
    /// </remarks>
    private static void RegisterRegionDropdown(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var acceptableValues = new AcceptableValueList<string>("NA", "EU", "ASIA", "OCE");
        var configEntry = config.Bind("Example", "Region", "NA", new ConfigDescription("Example server region.", acceptableValues));

        var options = new SettingOptions<string>(
            Section: "Example",
            Key: "Region",
            DefaultValue: "NA",
            Description: "Example server region.",
            Label: "Region",
            Tab: "Example Mod: Network & Input",
            Group: "Network",
            AcceptableValues: acceptableValues,
            OnValueChanged: value => Plugin.Logger?.LogInfo($"Region changed to {value}."));

        SettingsMenu.Register(configEntry, options);
    }

    /// <summary>
    /// Example 20: An integer slider from a ConfigEntry on the custom
    /// "Example Mod: Gameplay" tab. This updates the game's target frame rate
    /// whenever the slider changes.
    /// </summary>
    /// <remarks>
    /// The default value and description are duplicated in SettingOptions because
    /// EmberConfig does not re-derive them from the existing ConfigEntry.
    /// </remarks>
    private static void RegisterFpsCapSlider(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var acceptableValues = new AcceptableValueRange<int>(0, 240);
        var configEntry = config.Bind("Example", "FpsCap", 60, new ConfigDescription("Example FPS cap; 0 disables the cap.", acceptableValues));

        var options = new SettingOptions<int>(
            Section: "Example",
            Key: "FpsCap",
            DefaultValue: 60,
            Description: "Example FPS cap; 0 disables the cap.",
            Label: "FPS Cap",
            Tab: "Example Mod: Gameplay",
            Group: "Gameplay",
            AcceptableValues: acceptableValues,
            OnValueChanged: value => Application.targetFrameRate = value == 0 ? -1 : value);

        SettingsMenu.Register(configEntry, options);
    }

    /// <summary>
    /// Example 21: A dual-key keybind created from existing ConfigEntries.
    /// Primary and secondary keys can both trigger the same callback, which is
    /// useful for actions like sprint or push-to-talk on the custom
    /// "Example Mod: Network and Input" tab.
    /// </summary>
    private static void RegisterSprintWalkKeybind(ConfigFile config, ExampleMonoBehaviour behaviour)
    {
        var primary = config.Bind("Example", "SprintPrimary", KeyCode.LeftShift, "Primary sprint key.");
        var secondary = config.Bind("Example", "SprintSecondary", KeyCode.F, "Secondary sprint key.");

        var options = new KeybindOptions(
            Section: "Example",
            Key: "Sprint",
            DefaultPrimary: KeyCode.LeftShift,
            Description: "Example dual-key sprint action.",
            Label: "Sprint / Walk",
            Tab: "Example Mod: Network & Input",
            Group: "Input",
            DefaultSecondary: KeyCode.F,
            OnPressed: () => Plugin.Logger?.LogInfo("Sprint key pressed."),
            OnReleased: () => Plugin.Logger?.LogInfo("Sprint key released."));

        SettingsMenu.RegisterKeybind(primary, secondary, options);
    }
}
