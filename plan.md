# PRD: Mobile Debug Panel — SRDebugger-Inspired Custom Tool

## Overview

A lightweight, custom-built in-game debug panel for Unity 6 (2D URP), inspired by SRDebugger. Built entirely from scratch — no paid assets. Triggered by a triple-tap on a corner button, displayed as a full-screen overlay, no PIN protection.

---

## Core Features (Prioritized)

### 1. Options Tab
- A **partial class** pattern: `DebugOptions` class split across multiple files (`DebugOptions.Gameplay.cs`, `DebugOptions.Debug.cs`, etc.)
- Properties with get/set show up as **toggles, sliders, or text fields** based on type
- Supported types: `bool`, `float`, `int`, `string`, `Enum`
- Public void methods with no parameters show as **buttons**
- **Attributes**: `[Category("...")]`, `[NumberRange(min, max)]`, `[DisplayName("...")]`, `[Sort(n)]`
- `INotifyPropertyChanged` support for live value refresh
- API to add/remove runtime option containers: `DebugPanel.Instance.AddOptionContainer(obj)`

### 2. Runtime Console
- Hooks into `Application.logMessageReceived`
- Displays `Log`, `Warning`, `Error` messages with color coding
- Filterable by log type
- Tap entry to expand full message + stack trace
- Clear button
- Optional dockable mini-console (collapsed view pinned to game view)

### 3. System Info Tab
- Device model, OS, screen resolution, DPI
- Unity version, app version, build GUID
- Graphics: GPU name, memory, URP quality level
- Current scene name
- Runtime memory usage

---

## Architecture

```
Assets/
└── DebugPanel/
    ├── Core/
    │   ├── DebugPanel.cs              # Singleton, main API (ShowPanel, HidePanel, etc.)
    │   ├── DebugPanelTrigger.cs       # Triple-tap trigger button
    │   └── DebugPanelCanvas.cs        # Root canvas, tab routing
    ├── Tabs/
    │   ├── OptionsTab/
    │   │   ├── OptionsTab.cs
    │   │   ├── OptionRenderer.cs      # Renders a single option based on type
    │   │   └── OptionContainerScanner.cs  # Reflects properties/methods
    │   ├── ConsoleTab/
    │   │   ├── ConsoleTab.cs
    │   │   └── ConsoleEntry.cs
    │   └── SystemInfoTab/
    │       └── SystemInfoTab.cs
    ├── Options/
    │   ├── DebugOptions.cs            # Partial class root
    │   ├── OptionDefinition.cs        # Dynamic option descriptor
    │   ├── DynamicOptionContainer.cs
    │   ├── Attributes/
    │   │   ├── CategoryAttribute.cs
    │   │   ├── NumberRangeAttribute.cs
    │   │   ├── DisplayNameAttribute.cs
    │   │   └── SortAttribute.cs
    │   └── IOptionContainer.cs
    ├── UI/
    │   └── Prefabs/                   # uGUI prefabs for the panel, tabs, entries
    └── Resources/
        └── DebugPanel.prefab          # Auto-loaded prefab
```

---

## Public API Surface

```csharp
// Main singleton
DebugPanel.Instance.ShowPanel();
DebugPanel.Instance.HidePanel();
DebugPanel.Instance.IsVisible { get; }

// Options
DebugPanel.Instance.AddOptionContainer(object container);
DebugPanel.Instance.RemoveOptionContainer(object container);
DebugPanel.Instance.AddOption(OptionDefinition option);
DebugPanel.Instance.RemoveOption(OptionDefinition option);

// Events
DebugPanel.Instance.OnVisibilityChanged += (bool visible) => { };

// System Info
DebugPanel.Instance.AddSystemInfo(string label, string value, string category = "Default");
```

---

## Options Class Usage (Developer-facing)

```csharp
// DebugOptions.Gameplay.cs
public partial class DebugOptions {
    private float _gameSpeed = 1f;

    [Category("Gameplay")]
    [NumberRange(0.1f, 5f)]
    public float GameSpeed {
        get => _gameSpeed;
        set { _gameSpeed = value; Time.timeScale = value; }
    }

    [Category("Utilities")]
    public void ResetLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
```

---

## Implementation Phases

| Phase | Content |
|-------|---------|
| **1** | Core singleton, canvas, trigger button, tab navigation shell |
| **2** | Options tab: reflection scanner, attribute system, UI renderers per type |
| **3** | Console tab: log hook, list view, filter buttons, expand-on-tap |
| **4** | System Info tab: populate from `SystemInfo`, `Application`, `Screen` |
| **5** | Dynamic options API (`AddOptionContainer`, `OptionDefinition.Create`) |
| **6** | Polish: animations, scrolling, mobile touch usability |

---

## Constraints & Design Decisions

- **Unity 6 / URP 2D** — UI built with uGUI (Canvas + TextMeshPro)
- **New Input System** for the triple-tap trigger detection
- **No external dependencies** beyond what's already in the project
- **`#if DEBUG` or a compile flag** can strip the panel from release builds (mirrors SRDebugger's disable feature)
- Prefab auto-loaded via `Resources.Load` so it persists across scenes (DontDestroyOnLoad)
