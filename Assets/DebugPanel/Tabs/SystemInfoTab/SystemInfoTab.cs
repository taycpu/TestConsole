using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace DebugPanel.Tabs
{
    /// <summary>
    /// The System Info Tab.
    /// Auto-populates with device, OS, graphics, and Unity runtime data.
    /// Additional entries can be added via DebugPanelManager.Instance.AddSystemInfo().
    /// </summary>
    public class SystemInfoTab : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject sectionHeaderPrefab;
        [SerializeField] private GameObject infoRowPrefab;

        private readonly List<CustomInfoEntry> _customEntries = new List<CustomInfoEntry>();
        private readonly List<GameObject> _rows = new List<GameObject>();

        private void OnEnable()
        {
            Rebuild();
        }

        public void AddEntry(string label, string value, string category = "Default")
        {
            _customEntries.Add(new CustomInfoEntry(label, value, category));
            if (gameObject.activeInHierarchy)
                Rebuild();
        }

        private void Rebuild()
        {
            foreach (var row in _rows)
                if (row != null) Destroy(row);
            _rows.Clear();

            if (contentRoot == null) return;

            // ── Device ────────────────────────────────────────────────────
            SpawnSection("Device");
            SpawnRow("Model", SystemInfo.deviceModel);
            SpawnRow("Name", SystemInfo.deviceName);
            SpawnRow("Type", SystemInfo.deviceType.ToString());
            SpawnRow("OS", SystemInfo.operatingSystem);

            // ── Screen ────────────────────────────────────────────────────
            SpawnSection("Screen");
            SpawnRow("Resolution", $"{Screen.width} x {Screen.height}");
            SpawnRow("DPI", Screen.dpi.ToString("F0"));
            SpawnRow("Refresh Rate", $"{Screen.currentResolution.refreshRateRatio.numerator} Hz");
            SpawnRow("Full Screen", Screen.fullScreen.ToString());

            // ── Graphics ──────────────────────────────────────────────────
            SpawnSection("Graphics");
            SpawnRow("GPU", SystemInfo.graphicsDeviceName);
            SpawnRow("GPU Vendor", SystemInfo.graphicsDeviceVendor);
            SpawnRow("GPU Memory", $"{SystemInfo.graphicsMemorySize} MB");
            SpawnRow("API", SystemInfo.graphicsDeviceType.ToString());
            SpawnRow("Shader Level", $"SM {SystemInfo.graphicsShaderLevel / 10}.{SystemInfo.graphicsShaderLevel % 10}");
            SpawnRow("Max Texture", $"{SystemInfo.maxTextureSize}px");

            // ── CPU / Memory ──────────────────────────────────────────────
            SpawnSection("CPU / Memory");
            SpawnRow("CPU", SystemInfo.processorType);
            SpawnRow("CPU Cores", SystemInfo.processorCount.ToString());
            SpawnRow("CPU Speed", $"{SystemInfo.processorFrequency} MHz");
            SpawnRow("RAM", $"{SystemInfo.systemMemorySize} MB");

            // ── Application ───────────────────────────────────────────────
            SpawnSection("Application");
            SpawnRow("Product", Application.productName);
            SpawnRow("Version", Application.version);
            SpawnRow("Unity", Application.unityVersion);
            SpawnRow("Platform", Application.platform.ToString());
            SpawnRow("Build GUID", Application.buildGUID);
            SpawnRow("Scene", SceneManager.GetActiveScene().name);
            SpawnRow("Target FPS", Application.targetFrameRate.ToString());
            SpawnRow("Background Run", Application.runInBackground.ToString());

            // ── Custom ────────────────────────────────────────────────────
            if (_customEntries.Count > 0)
            {
                string lastCategory = null;
                foreach (var entry in _customEntries)
                {
                    if (entry.Category != lastCategory)
                    {
                        lastCategory = entry.Category;
                        SpawnSection(entry.Category);
                    }
                    SpawnRow(entry.Label, entry.Value);
                }
            }
        }

        private void SpawnSection(string title)
        {
            if (sectionHeaderPrefab == null) return;
            var go = Instantiate(sectionHeaderPrefab, contentRoot);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = title.ToUpper();
            _rows.Add(go);
        }

        private void SpawnRow(string label, string value)
        {
            if (infoRowPrefab == null) return;
            var go = Instantiate(infoRowPrefab, contentRoot);
            var texts = go.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = label;
                texts[1].text = value;
            }
            _rows.Add(go);
        }

        private class CustomInfoEntry
        {
            public string Label { get; }
            public string Value { get; }
            public string Category { get; }
            public CustomInfoEntry(string label, string value, string category)
            {
                Label = label; Value = value; Category = category;
            }
        }
    }
}
