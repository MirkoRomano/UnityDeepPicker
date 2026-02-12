using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[InitializeOnLoad]
public static class SceneViewFinder
{
    private const int MAX_SHOWABLE_OBJECTS = 100;
    private static readonly GameObject[] pickedIgnoreObjects;
    private static readonly HashSet<Object> pickedObjectsMap;

    static SceneViewFinder()
    {
        pickedObjectsMap = new HashSet<Object>();
        pickedIgnoreObjects = new GameObject[MAX_SHOWABLE_OBJECTS];
        SceneView.duringSceneGui += OnSceneGUI;
    }

    public static void OnSceneGUI(SceneView view)
    {
        Event e = Event.current;

        if (!IsViewFinderEvent(e))
        {
            return;
        }

        pickedObjectsMap.Clear();
        System.Array.Clear(pickedIgnoreObjects, 0, pickedIgnoreObjects.Length);
        PickupAllSceneObjectUnderPoint(e.mousePosition);
        
        PopupWindow.Show(new Rect(e.mousePosition, Vector2.zero), 
                         new SceneViewContextMenu(pickedObjectsMap));
    }

    private static bool IsViewFinderEvent(Event e)
    {
        return e.type == EventType.MouseDown
            && e.button == 1
            && e.alt;
    }

    private static void PickupAllSceneObjectUnderPoint(Vector2 point)
    {
        GameObject picked = null;
        int index = 0;

        do
        {
            if(index > MAX_SHOWABLE_OBJECTS - 1)
            {
                break;
            }

            picked = HandleUtility.PickGameObject(point, false, pickedIgnoreObjects);

            if(picked != null)
            {
                pickedObjectsMap.Add(picked);
                pickedIgnoreObjects[index] = picked;
                index++;
            }
        }
        while (picked != null);

        Vector2 screenPos = HandleUtility.GUIPointToScreenPixelCoordinate(point);
        Canvas[] canvases = GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;
        
        foreach (Canvas canvas in canvases)
        {
            if (canvas.TryGetComponent(out GraphicRaycaster raycaster))
            {
                if (index > MAX_SHOWABLE_OBJECTS - 1)
                {
                    break;
                }

                Object obj = raycaster.gameObject;
                if (pickedObjectsMap.Contains(obj))
                {
                    continue;
                }

                pickedObjectsMap.Add(obj);
                index++;
            }
        }
    }
}