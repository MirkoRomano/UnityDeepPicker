using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Sparkling.SceneFinder
{
    public class SceneViewContextMenu : PopupWindowContent
    {
        private const string ELLIPSIS = "...";
        private const string TYPE_SEARCH_KEYWORD = "t:";
        private const string LABEL_SEARCH_KEYWORD = "l:";
        private const string TAG_SEARCH_KEYWORD = "tag:";

        private Action OnOpenEvent;
        private Action OnCloseEvent;

        private SearchField searchField;

        private IEnumerable<QueryableItem> objects;
        private IEnumerable<QueryableItem> filteredObjects;
        private LazyLoader loader = new LazyLoader();

        private int objectsCount;

        private string searchFilter;
        private string lastSearchFilter;

        private Vector2 scrollPos;

        public SceneViewContextMenu(IEnumerable<QueryableItem> objs, Action onOpen = null, Action onClose = null)
        {
            OnOpenEvent = onOpen;
            OnCloseEvent = onClose;

            objects = objs.OrderBy(o => o.Name);
            filteredObjects = objects;
            objectsCount = objs.Count();
            searchFilter = string.Empty;
            lastSearchFilter = string.Empty;

            searchField = new SearchField();

            loader.Initialize(objects);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 250);
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
            DrawToolbar();
            DrawSearchbar();
            DrawScrollview();
        }

        /// <summary>
        /// Draw the popup toolbar
        /// </summary>
        private void DrawToolbar()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 20, EditorStyles.toolbar);
            EditorGUI.DrawRect(rect, new Color(0.1607843f, 0.1607843f, 0.1607843f));

            float offset = 5f;
            float buttonSize = 25;
            Rect labelRect = new Rect(rect.x + offset, rect.y, rect.width - offset - buttonSize, rect.height);
            GUI.Label(labelRect, $"Select an object ({objectsCount})", EditorStyles.label);
        }

        /// <summary>
        /// Draw the search bar
        /// </summary>
        private void DrawSearchbar()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);

            Rect rect = EditorGUILayout.GetControlRect(false, 20, EditorStyles.toolbar);
            searchFilter = searchField.OnGUI(rect, searchFilter);

            if (searchFilter != lastSearchFilter)
            {
                lastSearchFilter = searchFilter;
                FilterObjects();
            }

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the popup body
        /// </summary>
        private void DrawScrollview()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            Color background = GUI.color;

            foreach (var obj in filteredObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                GUI.color = background;

                Rect rowRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
                GUIContent content = EditorGUIUtility.ObjectContent(obj.Item, obj.GetType());


                if (Selection.activeGameObject == obj.Item)
                {
                    EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.48f, 0.90f, 1f));
                }
                else if (rowRect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(rowRect, new Color(0.6f, 0.6f, 0.6f, 0.2f));
                }

                Vector2 textSize = EditorStyles.label.CalcSize(new GUIContent(content.text));
                float maxTextWidth = rowRect.width - EditorGUIUtility.singleLineHeight - ELLIPSIS.Length;
                if (textSize.x > maxTextWidth)
                {
                    content.text = TruncateText(content.text, maxTextWidth, EditorStyles.label);
                }

                GUI.Label(rowRect, content, EditorStyles.label);

                if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                {
                    Selection.activeGameObject = obj.As<GameObject>();
                    editorWindow.Close();
                    Event.current.Use();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            GUILayout.EndScrollView();
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

            if (searchFilter.StartsWith(TYPE_SEARCH_KEYWORD, StringComparison.OrdinalIgnoreCase))
            {
                string filterType = searchFilter.Substring(TYPE_SEARCH_KEYWORD.Length).Trim();
                filteredObjects = objects.Where(o => o.HasComponent(filterType));
            }
            else if (searchFilter.StartsWith(LABEL_SEARCH_KEYWORD, StringComparison.OrdinalIgnoreCase))
            {
                string targetLabel = searchFilter.Substring(LABEL_SEARCH_KEYWORD.Length).Trim();
                filteredObjects = objects.Where(o => o.HasLabel(targetLabel));
            }
            else if (searchFilter.StartsWith(TAG_SEARCH_KEYWORD, StringComparison.OrdinalIgnoreCase))
            {
                string targetTag = searchFilter.Substring(TAG_SEARCH_KEYWORD.Length).Trim();
                filteredObjects = objects.Where(o => o.HasTag(targetTag));
            }
            else
            {
                filteredObjects = objects.Where(o => o.NameContains(searchFilter));
            }

            filteredObjects = filteredObjects.OrderBy(o => o.Name);
        }
    }
}