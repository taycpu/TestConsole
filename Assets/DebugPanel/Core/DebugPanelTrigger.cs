using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DebugPanel
{
    /// <summary>
    /// Small always-on-screen button in the corner of the screen.
    /// Triple-tap to open the debug panel.
    /// </summary>
    public class DebugPanelTrigger : MonoBehaviour, IPointerClickHandler
    {
        [Header("Settings")]
        [SerializeField] private int tapsRequired = 3;
        [SerializeField] private float tapResetTime = 0.8f;
        [SerializeField] private RectTransform triggerButton;

        private int _tapCount;
        private float _lastTapTime;

        private void Awake()
        {
            // Ensure the trigger itself is not destroyed
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            float now = Time.unscaledTime;

            if (now - _lastTapTime > tapResetTime)
                _tapCount = 0;

            _tapCount++;
            _lastTapTime = now;

            if (_tapCount >= tapsRequired)
            {
                _tapCount = 0;
                DebugPanelManager.Instance.TogglePanel();
            }
        }
    }
}
