using DebugPanel;
using DebugPanel.Options;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ExampleDebugOptions : MonoBehaviour
{
    private float _gameSpeed = 1f;
    private bool _godMode;

    private void OnEnable()
    {
        DebugPanelManager.Instance.AddOptionContainer(this);
    }

    private void OnDisable()
    {
        if (DebugPanelManager.HasInstance)
            DebugPanelManager.Instance.RemoveOptionContainer(this);
    }

    [Category("Gameplay")]
    [DisplayName("Game Speed")]
    [NumberRange(0.1f, 5f)]
    [Sort(0)]
    public float GameSpeed
    {
        get => _gameSpeed;
        set
        {
            _gameSpeed = value;
            Time.timeScale = value;
        }
    }

    [Category("Gameplay")]
    [DisplayName("God Mode")]
    [Sort(1)]
    public bool GodMode
    {
        get => _godMode;
        set => _godMode = value;
    }

    [Category("Diagnostics")]
    [DisplayName("FPS")]
    [Sort(0)]
    public float CurrentFps => 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

    [Category("Actions")]
    [DisplayName("Reload Scene")]
    [Sort(0)]
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    [Category("Actions")]
    [DisplayName("Emit Test Log")]
    [Sort(1)]
    public void EmitTestLog()
    {
        Debug.Log("[DebugPanel] Test log from ExampleDebugOptions.");
    }
}
