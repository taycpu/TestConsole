using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DebugPanel
{
    /// <summary>
    /// Manages the root Canvas, tab bar, and tab switching.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class DebugPanelCanvas : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Tab Buttons")]
        [SerializeField] private Button optionsTabButton;
        [SerializeField] private Button consoleTabButton;
        [SerializeField] private Button systemInfoTabButton;
        [SerializeField] private Button closeButton;

        [Header("Tab Panels")]
        [SerializeField] private GameObject optionsTabPanel;
        [SerializeField] private GameObject consoleTabPanel;
        [SerializeField] private GameObject systemInfoTabPanel;

        [Header("Tab Button Colors")]
        [SerializeField] private Color activeTabColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color inactiveTabColor = new Color(0.15f, 0.15f, 0.15f);

        private DebugTab _currentTab = DebugTab.Options;
        private Image[] _tabButtonImages;

        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            _tabButtonImages = new Image[]
            {
                optionsTabButton?.GetComponent<Image>(),
                consoleTabButton?.GetComponent<Image>(),
                systemInfoTabButton?.GetComponent<Image>()
            };

            optionsTabButton?.onClick.AddListener(() => SwitchToTab(DebugTab.Options));
            consoleTabButton?.onClick.AddListener(() => SwitchToTab(DebugTab.Console));
            systemInfoTabButton?.onClick.AddListener(() => SwitchToTab(DebugTab.SystemInfo));
            closeButton?.onClick.AddListener(() => DebugPanelManager.Instance.HidePanel());
        }

        private void Start()
        {
            Hide();
            SwitchToTab(DebugTab.Options);
        }

        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void SwitchToTab(DebugTab tab)
        {
            _currentTab = tab;

            SetTabActive(optionsTabPanel, tab == DebugTab.Options);
            SetTabActive(consoleTabPanel, tab == DebugTab.Console);
            SetTabActive(systemInfoTabPanel, tab == DebugTab.SystemInfo);

            RefreshTabButtonColors();
        }

        private void SetTabActive(GameObject tabPanel, bool active)
        {
            if (tabPanel != null)
                tabPanel.SetActive(active);
        }

        private void RefreshTabButtonColors()
        {
            for (int i = 0; i < _tabButtonImages.Length; i++)
            {
                if (_tabButtonImages[i] != null)
                    _tabButtonImages[i].color = (i == (int)_currentTab) ? activeTabColor : inactiveTabColor;
            }
        }
    }
}
