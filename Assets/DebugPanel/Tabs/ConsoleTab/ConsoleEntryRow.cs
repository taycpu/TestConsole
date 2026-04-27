using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DebugPanel.Tabs
{
    /// <summary>
    /// Renders a single console log entry row.
    /// Tap to expand/collapse the stack trace.
    /// </summary>
    public class ConsoleEntryRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text stackTraceText;
        [SerializeField] private Button expandButton;
        [SerializeField] private Image backgroundImage;

        private bool _expanded;

        private void Awake()
        {
            expandButton?.onClick.AddListener(ToggleExpand);
            if (stackTraceText != null)
                stackTraceText.gameObject.SetActive(false);
        }

        public void Bind(ConsoleEntryData data, Color color)
        {
            if (messageText != null)
            {
                messageText.text = data.Message;
                messageText.color = color;
            }

            if (stackTraceText != null)
                stackTraceText.text = data.StackTrace;

            if (backgroundImage != null)
            {
                var bg = color;
                bg.a = 0.08f;
                backgroundImage.color = bg;
            }
        }

        private void ToggleExpand()
        {
            _expanded = !_expanded;
            if (stackTraceText != null)
                stackTraceText.gameObject.SetActive(_expanded);
        }
    }
}
