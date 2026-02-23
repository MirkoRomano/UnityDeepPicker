using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sparkling.DeepClicker
{
    [InitializeOnLoad]
    internal static class SceneViewListener
    {
        private static int MaxShowableCount => DeepClickerCache.Settings.MaxShowableItemCount;

        private static GameObject[] itemsToIgnore = new GameObject[MaxShowableCount];
        private static HashSet<QueryableItem> foundItems = new HashSet<QueryableItem>();
        private static List<RaycastResult> raycastResults = new List<RaycastResult>();
        private static Dictionary<GameObject, QueryableItem> hierarchyMap = new Dictionary<GameObject, QueryableItem>();

        static SceneViewListener()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public static void OnSceneGUI(SceneView view)
        {
            Event e = Event.current;
            if (!IsViewFinderEvent(e))
            {
                return;
            }

            Event.current.Use();

            ClearCache();
            GetObjectInSceneView(e);
            PopupWindow.Show(new Rect(e.mousePosition, Vector2.zero),
                             new DeepClickerContextMenu(foundItems, null, ClearCache));
        }

        private static bool IsViewFinderEvent(Event e)
        {
            return e.type == EventType.MouseDown
                && e.button == 1
                && e.alt;
        }

        private static void ClearCache()
        {
            ReturnObjectsToPool(foundItems);

            foundItems.Clear();
            raycastResults.Clear();
            hierarchyMap.Clear();
            
            Array.Clear(itemsToIgnore, 0, MaxShowableCount);

            if (itemsToIgnore.Length != MaxShowableCount)
            {
                Array.Resize(ref itemsToIgnore, MaxShowableCount);
            }
        }

        private static QueryableItem PickupFromPool()
        {
            return DeepClickerCache.QueryablePool.Rent();
        }

        public static void ReturnToPool(QueryableItem item)
        {
            item.Free();
            DeepClickerCache.QueryablePool.Return(item);
        }

        public static void ReturnObjectsToPool(IEnumerable<QueryableItem> items)
        {
            foreach (QueryableItem item in items)
            {
                ReturnToPool(item);
            }
        }

        private static void GetObjectInSceneView(Event current)
        {
            Vector2 mousePosition = current.mousePosition;
            PickupWorldObject(mousePosition);
            PickupCanvasObject(mousePosition);

            CalculateFamilyTree();
        }

        private static void PickupWorldObject(Vector2 point)
        {
            GameObject picked = null;

            do
            {
                if (foundItems.Count >= MaxShowableCount)
                {
                    break;
                }

                picked = HandleUtility.PickGameObject(point, false, itemsToIgnore);

                if (picked != null)
                {
                    QueryableItem item = PickupFromPool();
                    item.Initialize(picked);

                    if (foundItems.Add(item))
                    {
                        itemsToIgnore[foundItems.Count - 1] = picked;
                    }
                    else
                    {
                        ReturnToPool(item);
                        break;
                    }
                }
            }
            while (picked != null);
        }

        private static void PickupCanvasObject(Vector2 point)
        {
            Vector2 screenPos = HandleUtility.GUIPointToScreenPixelCoordinate(point);
            var canvases = GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
            foreach (Canvas canvas in canvases)
            {
                if (foundItems.Count >= MaxShowableCount) break;

                if (canvas.TryGetComponent(out GraphicRaycaster raycaster))
                {
                    raycastResults.Clear();
                    raycaster.Raycast(pointerData, raycastResults);

                    foreach (var result in raycastResults)
                    {
                        if (foundItems.Count >= MaxShowableCount)
                        {
                            break;
                        }

                        QueryableItem item = PickupFromPool();
                        item.Initialize(result.gameObject);

                        if (!foundItems.Add(item))
                        {
                            ReturnToPool(item);
                        }
                    }
                }
            }
        }

        private static void CalculateFamilyTree()
        {
            hierarchyMap.Clear();
            foreach (var item in foundItems)
            {
                if (item.Item is GameObject go)
                {
                    if (!hierarchyMap.ContainsKey(go))
                    {
                        hierarchyMap.Add(go, item);
                    }
                }
            }

            foreach (var item in foundItems)
            {
                Transform t = item.GoTransform;
                Transform parentTransform = t.parent;

                if (parentTransform != null && hierarchyMap.TryGetValue(parentTransform.gameObject, out QueryableItem foundParent))
                {
                    item.SetParent(foundParent);
                }
            }
        }

    }
}