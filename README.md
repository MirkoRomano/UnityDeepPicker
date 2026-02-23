# 🔎 Unity Deep Picker (SceneView Finder)
A lightweight and high-performance Unity Editor tool that allows you to search, filter, and select overlapping GameObjects directly inside complex Scenes and UI Canvases.
Designed to improve workflow efficiency when working with dense hierarchies, layered UI, or crowded 3D environments.

---

## ✨ Features

### 🔍 Precise GameObject Selection
Quickly pick objects hidden behind others in both 2D/3D World Space and Canvas UI.

### ⚡ High-Performance Architecture
Built with:
- A custom **SimplePool** to prevent unnecessary memory allocations  
- A time-sliced **LazyLoader** to process complex hierarchies without freezing the Editor  

Smooth performance even in large scenes.

### 🎯 Smart Filtering System
- Instantly search objects by name
- Extend the search engine using custom filters via the `IFilterable` interface
- Reflection-based auto-detection of custom filters

### 🎨 Fully Customizable UI
Fine-tune every visual detail:
- Popup size
- Maximum scanned items
- Background color
- Hover state
- Selected state
- Outline color

All configurable through a dedicated Settings Window.

### 🧩 Native Editor Integration
Works seamlessly inside the Unity Scene View as a native popup window — no intrusive UI or workflow changes.

---

## 🎥 Demo

*(Add GIF or screenshots here for maximum impact)*

---

## 📦 Installation

### Option 1 — Unity Package (Recommended)

1. Go to **Releases**
2. Download the latest `.unitypackage`
3. Import it into your Unity project

---

### Option 2 — Manual Installation

Clone this repository inside your Unity project:

```
Assets/YourToolFolder
```

---

## 🖱 How to Use

1. Open any Scene in Unity.
2. Hover your mouse over overlapping GameObjects or UI elements.
3. Hold **Alt + Right-Click** in the Scene View.
4. A popup will appear listing all objects directly under your cursor, preserving hierarchy order.
5. Click an item to instantly select and ping it in the Hierarchy.

---

## 🛠 Advanced: Custom Filters

One of the most powerful features of Unity Deep Picker is its extensible filtering system.

By implementing the `IFilterable` interface, your custom logic will be automatically detected via reflection and integrated into the search engine.

### Example: Filter by Tag

If you want to search only for objects with a specific tag using:

```
tag:Player
```

You can implement:

```csharp
using Sparkling.SceneFinder;
using System.Linq;
using System.Collections.Generic;

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

Your filter will automatically become available in the search system.

---

## ⚙️ Configuration

You can customize how Unity Deep Picker looks and behaves.

Navigate to:

```
Tools > Sparkling > SceneViewFinder
```

A live preview window will allow you to configure:

### Window Settings
- Default popup size  
- Maximum number of scanned items  

### Colors
- Header  
- Search bar  
- Row hover/selection  
- Popup outline  

Click **Save** to apply your settings globally.

---

## 🚀 Why This Tool Exists

Working with large Unity scenes can become slow and frustrating — especially when dealing with layered UI Canvases or dense 3D environments.

Unity Deep Picker was built to:

- Provide instant deep-picking functionality  
- Improve hierarchy navigation  
- Reduce friction when selecting overlapping objects  
- Maintain high performance with minimal memory overhead  

---

## 🛠 Technical Details

- Unity 2022.3+
- C#
- EditorWindow-based architecture
- Reflection-driven extensible filtering system (`IFilterable`)
- Custom pooling and lazy loading systems

---

## 📄 License

MIT License
