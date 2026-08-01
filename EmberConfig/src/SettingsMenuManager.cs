namespace EmberConfig;

using System;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using EmberConfig.Core;
using EmberConfig.UI;
using UnityEngine;

/// <summary>
/// Manages discovery of the vanilla settings panel, injection of modded rows,
/// keybind dispatch, and UI lifecycle.
/// </summary>
public class SettingsMenuManager : MonoBehaviour, UI.IKeybindRowServices
{
    public SettingsMenuManager(IntPtr ptr) : base(ptr) { }

    /// <summary>
    /// Whether a keybind row is currently capturing input.
    /// </summary>
    public bool IsCapturing => injector?.IsCapturing ?? false;

    private readonly ToastManager toastManager = new();
    private readonly RebuildCoordinator rebuildCoordinator = new();

    private SettingsRegistry? registry;
    private PanelTracker? panelTracker;
    private UIFinder? uiFinder;
    private TabManager? tabManager;
    private RowFactory? rowFactory;
    private SettingsInjector? injector;
    private InputDispatcher? inputDispatcher;

    private void Awake()
    {
        try
        {
            DontDestroyOnLoad(gameObject);

            RegisterIl2CppTypes();

            if (!SettingsRegistry.IsInitialized)
                SettingsRegistry.Current = new SettingsRegistry();
            registry = SettingsRegistry.Current;

            panelTracker = new PanelTracker();
            uiFinder = new UIFinder();
            tabManager = new TabManager(uiFinder);
            rowFactory = new RowFactory(uiFinder, this);
            injector = new SettingsInjector(registry, tabManager, rowFactory, uiFinder);
            inputDispatcher = new InputDispatcher(registry.GetKeybindEntries);

            panelTracker.Opened += OnPanelOpened;
            panelTracker.Closed += OnPanelClosed;
            registry.EntryRegistered += OnEntryRegistered;
            SettingsPanelState.KeybindPanelRefreshed += OnKeybindPanelRefreshed;

            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.Awake failed: {ex}");
            enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (panelTracker is not null)
        {
            panelTracker.Opened -= OnPanelOpened;
            panelTracker.Closed -= OnPanelClosed;
            panelTracker.Reset();
        }

        if (registry is not null)
            registry.EntryRegistered -= OnEntryRegistered;

        SettingsPanelState.KeybindPanelRefreshed -= OnKeybindPanelRefreshed;
        SettingsPanelState.IsCapturing = false;
        SettingsPanelState.IsBlockingClose = false;
    }

    private void Update()
    {
        if (panelTracker is null || uiFinder is null || injector is null || inputDispatcher is null || tabManager is null)
            return;

        TrackPanel();
        InitializeUIIfNeeded();
        RebuildIfRequested();
        ContinueBuildIfNeeded();
        UpdateRowsAndState();
        ValidateTabState();
        PollInputAndToast();
    }

    private void ContinueBuildIfNeeded()
    {
        try
        {
            if (injector!.IsRebuilding)
                injector.BuildNextBatch();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.ContinueBuildIfNeeded failed: {ex}");
        }
    }

    private void TrackPanel()
    {
        try
        {
            panelTracker!.Tick();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.TrackPanel failed: {ex}");
        }
    }

    private void InitializeUIIfNeeded()
    {
        if (uiFinder!.IsReady || panelTracker!.PanelRoot is null)
            return;

        try
        {
            uiFinder.Initialize(panelTracker.PanelRoot);
            tabManager!.OnUIReady();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.InitializeUIIfNeeded failed: {ex}");
        }
    }

    private void RebuildIfRequested()
    {
        try
        {
            if (injector!.IsRebuilding)
                return;

            if (rebuildCoordinator.TryRebuild(panelTracker!.IsOpen, uiFinder!.IsReady))
                injector!.StartRebuild(tabManager!.GetActiveTabName());
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.RebuildIfRequested failed: {ex}");
        }
    }

    private void UpdateRowsAndState()
    {
        try
        {
            injector!.UpdateRows();
            SettingsPanelState.IsCapturing = injector.IsCapturing;
            tabManager!.Update(Time.deltaTime);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.UpdateRowsAndState failed: {ex}");
        }
    }

    private void ValidateTabState()
    {
        try
        {
            tabManager!.ValidateActiveTab();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.ValidateTabState failed: {ex}");
        }
    }

    private void PollInputAndToast()
    {
        try
        {
            inputDispatcher!.Poll(!panelTracker!.IsOpen);
            toastManager.Update();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.PollInputAndToast failed: {ex}");
        }
    }

    public void ShowKeybindToast(Transform rowTransform, string label, KeyCode key)
    {
        toastManager.Show(rowTransform, label, key);
    }

    public void RequestRebuild()
    {
        rebuildCoordinator.RequestRebuild(panelTracker?.IsOpen ?? false, uiFinder?.IsReady ?? false);
    }

    public void OnKeybindPanelRefreshed(int id)
    {
        RequestRebuild();
    }

    private void OnPanelOpened()
    {
        RequestRebuild();
    }

    private void OnPanelClosed()
    {
        SettingsPanelState.IsBlockingClose = false;
        injector?.Clear();
        tabManager?.OnPanelClosed();
        uiFinder?.Reset();
        panelTracker?.Reset();
    }

    private void OnEntryRegistered()
    {
        RequestRebuild();
    }

    private static void RegisterIl2CppTypes()
    {
        ClassInjector.RegisterTypeInIl2Cpp<SettingsPanelStateListener>();
        ClassInjector.RegisterTypeInIl2Cpp<RowHoverHandler>();
        ClassInjector.RegisterTypeInIl2Cpp<DropdownCaptionGuard>();
    }
}
