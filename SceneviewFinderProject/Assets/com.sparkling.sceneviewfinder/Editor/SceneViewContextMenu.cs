using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Sparkling.SceneFinder
{
    public class SceneViewContextMenu : PopupWindowContent, IFilterContext
    {
        private const string ELLIPSIS = "...";
        private const float ICON_WIDTH = 20f;
        private const float TRUNCATE_PADDING = 5f;
        private const float LINE_HEIGHT = 20f;
        private const float LEFT_MARGIN = 5f;
        private const float INDENT_WIDTH = 15f;

        private readonly Color BACKGROUND_COLOR = new Color(0.16f, 0.16f, 0.16f);
        private readonly Color SELECTION_COLOR = new Color(0.24f, 0.48f, 0.90f, 1f);
        private readonly Color HOVER_COLOR = new Color(0.6f, 0.6f, 0.6f, 0.2f);

        public IEnumerable<QueryableItem> Objects => objects;
        public IEnumerable<QueryableItem> FilteredObjects => filteredObjects;
        public IEnumerable<QueryableItem> RootObjects => rootObjects;

        public int ObjectsCount => objectsCount;
        public int FilteredObjectsCount => filteredObjectsCount;
        public string SearchFilter => searchFilter;
        public string FilterWord => filterWord;

        private Action OnOpenEvent;
        private Action OnCloseEvent;

        private SearchField searchField;

        private IEnumerable<QueryableItem> objects;
        private IEnumerable<QueryableItem> filteredObjects;
        private IEnumerable<QueryableItem> rootObjects;

        private HashSet<QueryableItem> expandedItems;

        private LazyLoader loader = new LazyLoader();

        private int objectsCount;
        private int filteredObjectsCount;

        private string searchFilter;
        private string lastSearchFilter;

        private string filterWord;

        private Vector2 scrollPos;

        public SceneViewContextMenu(IEnumerable<QueryableItem> objs, Action onOpen = null, Action onClose = null)
        {
            searchField = new SearchField();

            OnOpenEvent = onOpen;
            OnCloseEvent = onClose;

            objects = objs.OrderBy(o => o.Name);
            rootObjects = objs.Where(o => o.IsRoot);
            filteredObjects = objects;
            objectsCount = objs.Count();
            expandedItems = objs.Where(o => o.Children.Count > 0).ToHashSet();
            searchFilter = string.Empty;
            lastSearchFilter = string.Empty;

            loader.Initialize(objects);
        }

        public override Vector2 GetWindowSize()
        {
            return SceneViewCache.Settings.WindowSize;
        }

        public override void OnOpen()
        {
            base.OnOpen();
            OnOpenEvent?.Invoke();
        }

        public override void OnClose()
        {
            base.OnClose();
            OnCloseEvent?.Invoke();
        }

        public override void OnGUI(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                EditorStyles.helpBox.Draw(rect, false, false, false, false);
            }

            DrawToolbar();
            DrawSearchbar();
            DrawScrollview();
        }

        /// <summary>
        /// Draw the popup toolbar
        /// </summary>
        private void DrawToolbar()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, LINE_HEIGHT, EditorStyles.toolbar);
            EditorGUI.DrawRect(rect, BACKGROUND_COLOR);

            Rect labelRect = new Rect(rect.x + LEFT_MARGIN, rect.y, rect.width - LEFT_MARGIN, rect.height);

            string label = string.IsNullOrEmpty(searchFilter)
                        ? $"Objects found ({objectsCount})"
                        : $"Objects found ({filteredObjectsCount}/{objectsCount})";

            GUI.Label(labelRect, label, EditorStyles.label);
        }

        /// <summary>
        /// Draw the search bar
        /// </summary>
        private void DrawSearchbar()
        {
            GUILayout.Space(LEFT_MARGIN);
            GUILayout.BeginHorizontal();
            GUILayout.Space(LEFT_MARGIN);

            Rect rect = EditorGUILayout.GetControlRect(false, LINE_HEIGHT, EditorStyles.toolbar);
            searchFilter = searchField.OnGUI(rect, searchFilter);

            if (searchFilter != lastSearchFilter)
            {
                lastSearchFilter = searchFilter;
                FilterObjects();
            }

            GUILayout.Space(LEFT_MARGIN);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the popup body
        /// </summary>
        private void DrawScrollview()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            Color background = GUI.color;

            if (string.IsNullOrEmpty(searchFilter))
            {
                foreach (var obj in rootObjects)
                {
                    DrawTreeItem(obj, 0);
                }
            }
            else
            {
                foreach (var obj in filteredObjects)
                {
                    DrawRow(obj, 0);
                }
            }

            GUILayout.Space(5);
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Draw recursively a queryable item
        /// </summary>
        private void DrawTreeItem(QueryableItem item, int indentLevel)
        {
            bool hasChildren = item.Children != null && item.Children.Count > 0;
            bool isExpanded = DrawRow(item, indentLevel, hasChildren);

            if (hasChildren && isExpanded)
            {
                foreach (var child in item.Children)
                {
                    DrawTreeItem(child, indentLevel + 1);
                }
            }
        }

        /// <summary>
        /// Draw a Queriable item line
        /// </summary>
        private bool DrawRow(QueryableItem item, int indentLevel, bool hasChildren = false)
        {
            if (item?.Item == null) return false;

            GUILayout.BeginHorizontal();
            GUILayout.Space(indentLevel * INDENT_WIDTH);

            Rect rowRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));

            if (Selection.activeGameObject == item.Item)
            {
                EditorGUI.DrawRect(rowRect, SELECTION_COLOR);
            }
            else if (rowRect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rowRect, HOVER_COLOR);
            }

            bool isExpanded = expandedItems.Contains(item);
            Rect foldoutRect = new Rect(rowRect.x, rowRect.y, 14, rowRect.height);

            if (hasChildren)
            {
                EditorGUI.BeginChangeCheck();
                bool newExpandedState = EditorGUI.Foldout(foldoutRect, isExpanded, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    if (newExpandedState) expandedItems.Add(item);
                    else expandedItems.Remove(item);

                    GUIUtility.ExitGUI();
                }
            }

            GUIContent content = EditorGUIUtility.ObjectContent(item.Item, item.Item.GetType());
            
            float labelX = rowRect.x + foldoutRect.width;
            Rect labelRect = new Rect(labelX, rowRect.y, rowRect.width - foldoutRect.width, rowRect.height);

            float availableTextWidth = labelRect.width - ICON_WIDTH - TRUNCATE_PADDING;
            if (EditorStyles.label.CalcSize(new GUIContent(content.text)).x > availableTextWidth)
            {
                content.text = TruncateText(content.text, availableTextWidth, EditorStyles.label);
            }

            GUI.Label(labelRect, content, EditorStyles.label);

            if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
            {
                if (item.Item is GameObject go)
                {
                    Selection.activeGameObject = go;
                    EditorGUIUtility.PingObject(go);
                }

                Event.current.Use();
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();
            return isExpanded;
        }

        /// <summary>
        /// Truncate a text when it's with is greater than maxWidth
        /// </summary>
        private static string TruncateText(string text, float maxWidth, GUIStyle style)
        {
            if (style.CalcSize(new GUIContent(text)).x <= maxWidth)
            {
                return text;
            }

            int length = text.Length;

            while (length > 0)
            {
                string temp = text.Substring(0, length) + ELLIPSIS;

                if (style.CalcSize(new GUIContent(temp)).x <= maxWidth)
                {
                    return temp;
                }

                length--;
            }

            return ELLIPSIS;
        }

        /// <summary>
        /// Filter objects by search string
        /// </summary>
        private void FilterObjects()
        {
            if (string.IsNullOrWhiteSpace(searchFilter))
            {
                filteredObjects = objects;
                return;
            }

            int filterCount = SceneViewCache.Filters.Count;
            for (int i = 0; i < filterCount; i++)
            {
                IFilterable filterable = SceneViewCache.Filters[i];
                if (!filterable.Evaluate(this))
                {
                    continue;
                }

                filterWord = searchFilter.Substring(filterable.FilterKeyword.Length).Trim();
                filteredObjects = filterable.Filter(this);
                break;
            }

            filteredObjects = filteredObjects.OrderBy(o => o.Name);
            filteredObjectsCount = filteredObjects.Count();
        }
    }
}