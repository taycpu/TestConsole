# Debug Panel Documentation

## Overview

Debug Panel is a runtime developer overlay for Unity projects. It includes:

- Options tab for toggles, sliders, text fields, enum dropdowns, read-only values, and action buttons
- Console tab that captures Unity logs, warnings, errors, and stack traces
- System Info tab with device, application, graphics, memory, and scene information
- Auto-loaded prefab from `Resources/DebugPanel.prefab`
- Code-only fallback if the prefab cannot be loaded

## Installation By Git URL

Use Unity Package Manager's **Add package from git URL** option:

```text
https://github.com/taycpu/TestConsole.git?path=/Packages/com.taycpu.debug-panel
```

The package path is required because this repository is also a Unity development project.

## Runtime Bootstrap

`DebugPanelBootstrapper` initializes after scene load via `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)`. It accesses `DebugPanelManager.Instance`, which loads `Resources/DebugPanel.prefab` and marks it as `DontDestroyOnLoad`.

If the prefab is missing, the manager creates a fallback object and logs a warning. The prefab is the supported path because it contains the full UI hierarchy and wired component references.

## Opening The Panel

The included prefab contains a small trigger button. Triple-tap the trigger to toggle the panel.

You can also control visibility by code:

```csharp
DebugPanelManager.Instance.ShowPanel();
DebugPanelManager.Instance.HidePanel();
DebugPanelManager.Instance.TogglePanel();
```

Open a specific tab:

```csharp
DebugPanelManager.Instance.ShowPanel(DebugTab.Console);
```

## Registering Options

The recommended integration for package consumers is to register an object that contains public properties and public no-argument methods.

```csharp
using DebugPanel;
using DebugPanel.Options;
using UnityEngine;

public sealed class GameplayDebugOptions : MonoBehaviour
{
    private float _gameSpeed = 1f;

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
    public float GameSpeed
    {
        get => _gameSpeed;
        set
        {
            _gameSpeed = value;
            Time.timeScale = value;
        }
    }

    [Category("Actions")]
    public void ResetTimeScale()
    {
        GameSpeed = 1f;
    }
}
```

### Supported Property Types

- `bool`
- `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`
- `float`, `double`
- `string`
- `enum`

Properties with setters are editable. Getter-only properties are rendered as read-only values.

### Supported Methods

Public instance methods are rendered as buttons when they:

- Return `void`
- Have no parameters
- Are not property accessor methods

## Option Attributes

### Category

Groups options under a section label.

```csharp
[Category("Gameplay")]
public bool GodMode { get; set; }
```

### DisplayName

Overrides the generated label.

```csharp
[DisplayName("God Mode")]
public bool IsGodModeEnabled { get; set; }
```

### Sort

Controls ordering inside a category. Lower values appear first.

```csharp
[Sort(0)]
public bool FirstOption { get; set; }
```

### NumberRange

Renders numeric values with slider bounds.

```csharp
[NumberRange(0.1f, 5f)]
public float GameSpeed { get; set; }
```

## Dynamic Options

Use `DynamicOptionContainer` when options are created from code instead of reflected from a class.

```csharp
using DebugPanel;
using DebugPanel.Options;

var dynamicOptions = new DynamicOptionContainer();
var enabled = false;

dynamicOptions.AddOption(OptionDefinition.Create(
    "Feature Enabled",
    () => enabled,
    value => enabled = value,
    category: "Feature"));

DebugPanelManager.Instance.AddOptionContainer(dynamicOptions);
```

Remove it when it is no longer needed:

```csharp
DebugPanelManager.Instance.RemoveOptionContainer(dynamicOptions);
```

## System Info API

Add custom rows to the System Info tab:

```csharp
DebugPanelManager.Instance.AddSystemInfo("Backend", "Staging", "Build");
```

## Console Tab

The console tab subscribes to Unity log messages at runtime. It displays regular logs, warnings, and errors with filtering controls and expanded stack trace details.

## Samples

Import the **Example Options** sample from Package Manager to see a complete option container MonoBehaviour.

## Rebuilding Prefabs

The package includes an editor utility at:

```text
Tools/Debug Panel/Build Prefab
```

Use this only when you intentionally want to regenerate the package prefab. If you customize the prefab manually, keep the prefab and its `.meta` file under version control.

## Release Builds

Define `DISABLE_DEBUGPANEL` in Player Settings to prevent the bootstrapper from initializing the panel.

This define does not remove package code from compilation by itself; it prevents automatic runtime startup. Use assembly/platform constraints or build pipeline stripping if your project needs stronger release exclusion.

## Troubleshooting

### The panel does not appear

- Confirm the package is installed from the Git URL with `?path=/Packages/com.taycpu.debug-panel`.
- Confirm TextMeshPro essentials are imported in the project.
- Check the Unity console for a missing `Resources/DebugPanel.prefab` warning.
- Confirm `DISABLE_DEBUGPANEL` is not defined.

### Options do not show up

- Confirm the container is registered with `AddOptionContainer`.
- Confirm properties are public instance properties with supported types.
- Confirm methods are public, return `void`, and have no parameters.

### Numeric sliders behave unexpectedly

- Add `[NumberRange(min, max)]` to numeric properties that should be sliders.
- Make sure setters can accept the assigned type.

### Prefab changes are lost

- Do not regenerate the prefab unless intentional.
- Keep prefab `.meta` files committed so Unity preserves script/component references.
