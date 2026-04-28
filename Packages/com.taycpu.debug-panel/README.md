# Debug Panel

Debug Panel is a lightweight in-game developer panel for Unity. It gives teams a runtime options tab, log console, and system info view without requiring scene setup.

## Install

Open Unity Package Manager, choose **Add package from git URL**, and enter:

```text
https://github.com/taycpu/TestConsole.git?path=/Packages/com.taycpu.debug-panel
```

## Requirements

- Unity 6000.0 or newer
- uGUI
- TextMeshPro

## Quick Start

The package bootstraps itself after scene load. Triple-tap the on-screen corner trigger to open the panel.

Register your own options from any runtime object:

```csharp
using DebugPanel;
using DebugPanel.Options;
using UnityEngine;

public sealed class MyDebugOptions : MonoBehaviour
{
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
    [DisplayName("God Mode")]
    public bool GodMode
    {
        get => _godMode;
        set => _godMode = value;
    }

    [Category("Actions")]
    public void EmitTestLog()
    {
        Debug.Log("Debug Panel test log");
    }
}
```

Supported reflected members:

- Public instance properties of type `bool`, numeric primitives, `string`, or `enum`
- Public `void` instance methods with no parameters

Useful attributes:

- `[Category("Name")]`
- `[DisplayName("Friendly Name")]`
- `[Sort(10)]`
- `[NumberRange(min, max)]`

## Disable In Builds

Add `DISABLE_DEBUGPANEL` to your scripting define symbols to prevent the bootstrapper from initializing the panel.

## Documentation

See [documentation.md](documentation.md) for the full API guide, runtime registration patterns, and troubleshooting notes.
