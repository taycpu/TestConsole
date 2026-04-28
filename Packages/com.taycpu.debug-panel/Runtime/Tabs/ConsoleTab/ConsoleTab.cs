using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DebugPanel.Tabs
{
    public enum LogTypeFilter { Log = 0, Warning = 1, Error = 2 }

    /// <summary>
    /// The Console Tab — hooks Application.logMessageReceived and displays
    /// log, warning, and error messages with filtering and expand-on-tap.
    /// </summary>
    public class ConsoleTab : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject consoleEntryPrefab;
        [SerializeField] private Button clearButton;

        [Header("Filter Toggles")]
        [SerializeField] private Toggle showLogsToggle;
        [SerializeField] private Toggle showWarningsToggle;
        [SerializeField] private Toggle showErrorsToggle;

        [Header("Colors")]
        [SerializeField] private Color logColor = Color.white;
        [SerializeField] private Color warningColor = new Color(1f, 0.85f, 0f);
        [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f);

        private readonly List<ConsoleEntryData> _entries = new List<ConsoleEntryData>();
        private readonly List<GameObject> _rows = new List<GameObject>();

        private bool _showLogs = true;
        private bool _showWarnings = true;
        private bool _showErrors = true;

        private void Awake()
        {
            Application.logMessageReceived += OnLogReceived;
            clearButton?.onClick.AddListener(ClearLog);

            showLogsToggle?.onValueChanged.AddListener(v => { _showLogs = v; RebuildList(); });
            showWarningsToggle?.onValueChanged.AddListener(v => { _showWarnings = v; RebuildList(); });
            showErrorsToggle?.onValueChanged.AddListener(v => { _showErrors = v; RebuildList(); });
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogReceived;
        }

        private void OnEnable()
        {
            RebuildList();
        }

        private void OnLogReceived(string message, string stackTrace, LogType type)
        {
            _entries.Add(new ConsoleEntryData(message, stackTrace, type));
            if (gameObject.activeInHierarchy && IsPassingFilter(type))
                AppendRow(_entries[_entries.Count - 1]);
        }

        private void ClearLog()
        {
            _entries.Clear();
            foreach (var row in _rows)
                if (row != null) Destroy(row);
            _rows.Clear();
        }

        private void RebuildList()
        {
            foreach (var row in _rows)
                if (row != null) Destroy(row);
            _rows.Clear();

            foreach (var entry in _entries)
                if (IsPassingFilter(entry.LogType))
                    AppendRow(entry);
        }

        private void AppendRow(ConsoleEntryData entry)
        {
            if (consoleEntryPrefab == null || contentRoot == null) return;
            var go = Instantiate(consoleEntryPrefab, contentRoot);
            var row = go.GetComponent<ConsoleEntryRow>();
            row?.Bind(entry, GetColor(entry.LogType));
            _rows.Add(go);
        }

        private bool IsPassingFilter(LogType type)
        {
            return type switch
            {
                LogType.Warning => _showWarnings,
                LogType.Error or LogType.Exception or LogType.Assert => _showErrors,
                _ => _showLogs
            };
        }

        private Color GetColor(LogType type)
        {
            return type switch
            {
                LogType.Warning => warningColor,
                LogType.Error or LogType.Exception or LogType.Assert => errorColor,
                _ => logColor
            };
        }
    }

    public class ConsoleEntryData
    {
        public string Message { get; }
        public string StackTrace { get; }
        public LogType LogType { get; }

        public ConsoleEntryData(string message, string stackTrace, LogType logType)
        {
            Message = message;
            StackTrace = stackTrace;
            LogType = logType;
        }
    }
}
