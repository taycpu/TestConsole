using UnityEngine;
using DebugPanel;
using DebugPanel.Options;

/// <summary>
/// Auto-registers DebugOptions.Current into the DebugPanel on game start.
/// Attach this to any persistent GameObject, or let the DebugPanel prefab include it.
/// Uses [RuntimeInitializeOnLoadMethod] so it works without a scene reference.
/// </summary>
public static class DebugPanelBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
#if !DISABLE_DEBUGPANEL
        // Access the singleton (auto-bootstraps if not in scene)
        var manager = DebugPanelManager.Instance;

        // Register the built-in partial class options
        manager.AddOptionContainer(DebugOptions.Current);

        Debug.Log("[DebugPanel] Initialized. Triple-tap the corner button to open.");
#endif
    }
}
