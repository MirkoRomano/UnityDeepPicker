# Unity Deep Picker

![Unity Version](https://img.shields.io/badge/Unity-2019.4%20|%206000+-black?style=for-the-badge&logo=unity)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**Unity Deep Picker** is a high-performance Editor extension designed for rapid identification and selection of overlapping GameObjects within complex scenes and UI Canvases. It eliminates the friction of navigating dense hierarchies by providing a streamlined picking workflow directly in the Scene View.

---

## Key Features

### Precise Selection Logic
Instantly identify occluded or nested objects in both 2D/3D World Space and Canvas UI. It bypasses the limitations of standard Editor clicking by raycasting through all layers under your cursor.

### High-Performance Architecture
Engineered for large-scale projects with a focus on resource efficiency:
* **Zero-Alloc Pooling**: Utilizes a custom `SimplePool` to eliminate Garbage Collector overhead during picking operations.
* **Time-Sliced Lazy Loading**: Processes complex metadata and components asynchronously via `LazyLoader` to ensure a stutter-free Editor experience even with hundreds of results.
* **Legacy-Safe**: Specifically optimized to handle internal Unity API differences between versions (2019.4 vs 2022+).

### Extensible Search Engine
A robust filtering system that supports both native and user-defined logic:
* **Built-in Filters**: Search by Name (default), Type (`t:`), Labels (`l:`), Tags (`tag:`), and Layers (`lay:`).
* **Reflection-Based Discovery**: Custom filters are automatically detected and integrated into the search engine at startup.

---

## Installation

### Unity Package (Recommended)
1. Download the latest `.unitypackage` from the [Releases](../../releases) page.
2. Import the package into your project via `Assets > Import Package > Custom Package`.

### Manual Installation
Clone this repository or copy the contents into your project's `Assets/Plugins/DeepPicker` folder.

---

## How to Use
1.  **Hover** the cursor over overlapping elements in the **Scene View**.
2.  Press **`Ctrl` + `Right-Click`**.
3.  A popup menu will appear listing all objects under the cursor, maintaining their hierarchical order.
4.  **Type** in the search bar to filter results or **Click** an item to instantly select it in the **Hierarchy** and **Inspector**.

---

## Configuration

Access the settings panel via:  
`Tools > Sparkling > Deep Picker Settings`

The dedicated settings window provides a live preview for:
* **Scanning Parameters**: Adjust popup dimensions and the maximum number of detectable items.
* **Visual Identity**: Full control over colors for headers, search bars, hover/selection states, and outlines to match your preferred Editor theme.

---

## Advanced: Custom Filters
You can extend the search capabilities by implementing the `IFilterable` interface. The tool automatically integrates any new implementation found in the assembly.

### Example: Tag Filter
```csharp
public class TagFilter : IFilterable
{
    public string FilterKeyword => "tag:"; 
    public int FilterIndex => 0; 

    public bool Evaluate(IFilterContext context)
    {
        return context.SearchFilter.StartsWith(FilterKeyword);
    }

    public IEnumerable<QueryableItem> Filter(IFilterContext context)
    {
        string targetTag = context.FilterWord;
        return context.Objects.Where(item => item.HasTag(targetTag));
    }
}
```

## Technical Specifications

### Compatibility
Validated across the following Unity versions:
- Unity 2019.4 LTS (Legacy API & C# 7.3 Compatibility)
- Unity 2022.3 LTS
- Unity 6 (6000.x)

### Core Components
Component	Responsibility
SceneViewFinder	Core logic for 2D/3D/UI raycasting and input handling.
DeepClickerContextMenu	Manages the PopupWindow UI and search logic.
LazyLoader	Handles time-sliced metadata processing.
SimplePool	Generic object pool for QueryableItem reuse.

### License
This project is licensed under the MIT License - see the LICENSE file for details.