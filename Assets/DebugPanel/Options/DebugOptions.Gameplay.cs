using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugPanel.Options
{
    /// <summary>
    /// Example: Gameplay debug options.
    /// Copy this pattern to create your own DebugOptions.MyFeature.cs files.
    /// </summary>
    public partial class DebugOptions
    {
        // ── Game Speed ────────────────────────────────────────────────────────

        private float _gameSpeed = 1f;

        [Category("Gameplay")]
        [NumberRange(0.1f, 5f)]
        [Sort(0)]
        public float GameSpeed
        {
            get => _gameSpeed;
            set
            {
                _gameSpeed = value;
                Time.timeScale = value;
                OnPropertyChanged(nameof(GameSpeed));
            }
        }

        // ── Scene Management ──────────────────────────────────────────────────

        [Category("Gameplay")]
        [Sort(10)]
        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        [Category("Gameplay")]
        [Sort(11)]
        public void LoadNextScene()
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(next);
            else
                Debug.LogWarning("[DebugOptions] No next scene in build settings.");
        }

        // ── Read-Only Diagnostics ─────────────────────────────────────────────

        [Category("Diagnostics")]
        [DisplayName("FPS (approx)")]
        [Sort(0)]
        public float CurrentFPS => 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
    }
}
