namespace SettingsLib;

using System;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SettingsLib.Core;
using SettingsLib.UI;
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

    private DateTime lastErrorLog = DateTime.MinValue;
    private string? lastErrorMessage;
    private int errorRepeatCount;

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
            inputDispatcher = new InputDispatcher(() => registry.GetKeybindEntries());

            panelTracker.Opened += OnPanelOpened;
            panelTracker.Closed += OnPanelClosed;
            registry.EntryRegistered += OnEntryRegistered;
            SettingsPanelState.KeybindPanelRefreshed += OnKeybindPanelRefreshed;

            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"SettingsMenuManager.Awake failed: {ex}");
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
        try
        {
            if (panelTracker is null || uiFinder is null || injector is null || inputDispatcher is null || tabManager is null)
                return;

            panelTracker.Tick();

            if (!uiFinder.IsReady && panelTracker.PanelRoot is not null)
            {
                uiFinder.Initialize(panelTracker.PanelRoot);
                tabManager.OnUIReady();
            }

            if (rebuildCoordinator.TryRebuild(panelTracker.IsOpen, uiFinder.IsReady))
                injector.Rebuild();

            injector.UpdateRows();
            SettingsPanelState.IsCapturing = injector.IsCapturing;
            tabManager.ValidateActiveTab();
            inputDispatcher.Poll(!panelTracker.IsOpen);
            toastManager.Update();
        }
        catch (Exception ex)
        {
            var elapsed = DateTime.Now - lastErrorLog;
            var message = ex.ToString();
            if (message != lastErrorMessage || elapsed.TotalSeconds >= 2)
            {
                lastErrorLog = DateTime.Now;
                lastErrorMessage = message;
                errorRepeatCount = 0;
                Plugin.Logger?.LogError($"SettingsMenuManager.Update error: {ex}");
            }
            else
            {
                errorRepeatCount++;
                if (errorRepeatCount <= 3 || errorRepeatCount % 60 == 0)
                    Plugin.Logger?.LogError($"SettingsMenuManager.Update error repeated {errorRepeatCount} times: {ex.Message}");
            }
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

    private void OnEntryRegistered(ISettingEntry entry)
    {
        RequestRebuild();
    }

    private static void RegisterIl2CppTypes()
    {
        ClassInjector.RegisterTypeInIl2Cpp<SettingsPanelStateListener>();
        ClassInjector.RegisterTypeInIl2Cpp<RowHoverHandler>();
    }
}
