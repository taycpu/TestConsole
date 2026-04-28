using UnityEngine;
using DebugPanel.Tabs;

namespace DebugPanel
{
    /// <summary>
    /// Main singleton entry point for the Debug Panel.
    /// Auto-instantiates from Resources/DebugPanel.prefab on first access.
    /// </summary>
    public class DebugPanelManager : MonoBehaviour
    {
        private static DebugPanelManager _instance;

        public static DebugPanelManager Instance
        {
            get
            {
                if (_instance == null)
                    Bootstrap();
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        [SerializeField] private DebugPanelCanvas panelCanvas;

        public bool IsVisible => panelCanvas != null && panelCanvas.IsVisible;

        public delegate void VisibilityChangedHandler(bool visible);
        public event VisibilityChangedHandler OnVisibilityChanged;

        // ── Options ────────────────────────────────────────────────────────
        private OptionsTab _optionsTab;
        // ── System Info ───────────────────────────────────────────────────
        private SystemInfoTab _systemInfoTab;

        private static void Bootstrap()
        {
            // Try to find existing instance in scene first
            _instance = FindFirstObjectByType<DebugPanelManager>();
            if (_instance != null)
                return;

            var prefab = Resources.Load<GameObject>("DebugPanel");
            if (prefab != null)
            {
                var go = Instantiate(prefab);
                go.name = "DebugPanel [Runtime]";
                _instance = go.GetComponent<DebugPanelManager>();
            }
            else
            {
                // Fallback: build from code if prefab not yet created
                var go = new GameObject("DebugPanel [Runtime]");
                _instance = go.AddComponent<DebugPanelManager>();
                Debug.LogWarning("[DebugPanel] Resources/DebugPanel.prefab not found. Falling back to code-only init.");
            }

            DontDestroyOnLoad(_instance.gameObject);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _optionsTab = GetComponentInChildren<OptionsTab>(true);
            _systemInfoTab = GetComponentInChildren<SystemInfoTab>(true);
        }

        // ── Panel visibility ───────────────────────────────────────────────

        public void ShowPanel()
        {
            if (panelCanvas == null) return;
            panelCanvas.Show();
            OnVisibilityChanged?.Invoke(true);
        }

        public void ShowPanel(DebugTab tab)
        {
            ShowPanel();
            panelCanvas.SwitchToTab(tab);
        }

        public void HidePanel()
        {
            if (panelCanvas == null) return;
            panelCanvas.Hide();
            OnVisibilityChanged?.Invoke(false);
        }

        public void TogglePanel()
        {
            if (IsVisible) HidePanel();
            else ShowPanel();
        }

        // ── Options API ───────────────────────────────────────────────────

        public void AddOptionContainer(object container)
        {
            _optionsTab?.AddContainer(container);
        }

        public void RemoveOptionContainer(object container)
        {
            _optionsTab?.RemoveContainer(container);
        }

        public void AddOption(Options.OptionDefinition option)
        {
            _optionsTab?.AddOption(option);
        }

        public void RemoveOption(Options.OptionDefinition option)
        {
            _optionsTab?.RemoveOption(option);
        }

        // ── System Info API ───────────────────────────────────────────────

        public void AddSystemInfo(string label, string value, string category = "Default")
        {
            _systemInfoTab?.AddEntry(label, value, category);
        }
    }
}
