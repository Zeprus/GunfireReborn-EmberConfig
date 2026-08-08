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
    /// Current manager instance, set in <see cref="Awake"/> and cleared in <see cref="OnDestroy"/>.
    /// </summary>
    public static SettingsMenuManager? Current { get; private set; }

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
    private Harmony? harmony;

    private string? preRebuildActiveTab;
    private float preRebuildScrollPosition = 1f;

    private void Awake()
    {
        try
        {
            Current = this;

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

            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.Awake failed: {ex}");
            enabled = false;
        }
    }

    private void OnDestroy()
    {
        Current = null;

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

        harmony?.UnpatchSelf();
    }

    private void Update()
    {
        if (registry is null || panelTracker is null || uiFinder is null || tabManager is null || rowFactory is null || injector is null || inputDispatcher is null)
            return;

        TrackPanel();
        InitializeUIIfNeeded();
        CapturePreRebuildScroll();
        RebuildIfRequested();
        var wasRebuilding = injector.IsRebuilding;
        ContinueBuildIfNeeded();
        var isRebuilding = injector.IsRebuilding;
        if (wasRebuilding && !isRebuilding)
            RestoreScrollAfterRebuild();
        UpdateRowsAndState();
        ValidateTabState();
        PollInputAndToast();
    }

    private void RunPhase(string name, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.{name} failed: {ex}");
        }
    }

    private void TrackPanel() => RunPhase(nameof(TrackPanel), () => panelTracker!.Tick());

    private void InitializeUIIfNeeded()
    {
        RunPhase(nameof(InitializeUIIfNeeded), () =>
        {
            if (uiFinder?.IsReady ?? true)
                return;

            if (panelTracker?.PanelRoot is null)
                return;

            uiFinder!.Initialize(panelTracker.PanelRoot);
            tabManager!.OnUIReady();
        });
    }

    private void CapturePreRebuildScroll()
    {
        RunPhase(nameof(CapturePreRebuildScroll), () =>
        {
            if (injector?.IsRebuilding != false)
                return;

            (preRebuildActiveTab, preRebuildScrollPosition) = ScrollPreserver.Capture(uiFinder?.ScrollRect, tabManager!.GetActiveTabName);
        });
    }

    private void RebuildIfRequested()
    {
        RunPhase(nameof(RebuildIfRequested), () =>
        {
            if (injector!.IsRebuilding)
                return;

            if (rebuildCoordinator.TryRebuild(panelTracker!.IsOpen, uiFinder!.IsReady))
                injector!.StartRebuild(tabManager!.GetActiveTabName());
        });
    }

    private void ContinueBuildIfNeeded()
    {
        RunPhase(nameof(ContinueBuildIfNeeded), () =>
        {
            if (injector!.IsRebuilding)
                injector.BuildNextBatch();
        });
    }

    private void UpdateRowsAndState()
    {
        RunPhase(nameof(UpdateRowsAndState), () =>
        {
            injector!.UpdateRows();
            SettingsPanelState.IsCapturing = injector.IsCapturing;
            tabManager!.Update(Time.unscaledDeltaTime);
        });
    }

    private void ValidateTabState() => RunPhase(nameof(ValidateTabState), () => tabManager!.ValidateActiveTab());

    private void RestoreScrollAfterRebuild()
    {
        RunPhase(nameof(RestoreScrollAfterRebuild), () =>
        {
            ScrollPreserver.Restore(uiFinder?.ScrollRect, preRebuildActiveTab, tabManager!.GetActiveTabName, preRebuildScrollPosition);
        });
    }

    private void PollInputAndToast()
    {
        RunPhase(nameof(PollInputAndToast), () =>
        {
            inputDispatcher!.Poll(!panelTracker!.IsOpen);
            toastManager.Update();
        });
    }

    public void ShowKeybindToast(Transform rowTransform, string label, KeyCode key)
    {
        toastManager.Show(rowTransform, label, key);
    }

    public void RequestRebuild()
    {
        rebuildCoordinator.RequestRebuild(panelTracker?.IsOpen ?? false, uiFinder?.IsReady ?? false);
    }

    public void RequestVisibilityRefresh(string modName, string tabName)
    {
        if (string.IsNullOrWhiteSpace(modName))
            return;

        if (injector?.IsRebuilding ?? false)
        {
            RequestRebuild();
            return;
        }

        injector?.RefreshVisibility(modName, tabName);
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
        preRebuildActiveTab = null;
        preRebuildScrollPosition = 1f;
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
