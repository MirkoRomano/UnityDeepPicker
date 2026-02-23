using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Sparkling.DeepClicker
{
    internal class DeepClickerContextMenu : PopupWindowContent, IFilterContext
    {
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
        GUIStyle searchTextField;

        private int objectsCount;
        private int filteredObjectsCount;

        private string searchFilter;
        private string lastSearchFilter;

        private string filterWord;

        private Vector2 verticalPos;

        public DeepClickerContextMenu(IEnumerable<QueryableItem> objs, Action onOpen = null, Action onClose = null)
        {
            searchField = new SearchField();

            OnOpenEvent = onOpen;
            OnCloseEvent = onClose;

            objects = objs.OrderBy(o => o.Name);
            rootObjects = objs.Where(o => o.IsRoot);
            filteredObjects = objects;
            objectsCount = objs.Count();

#if UNITY_2021_1_OR_NEWER
            expandedItems = objs.Where(o => o.Children.Count > 0).ToHashSet();
#else
            expandedItems = new HashSet<QueryableItem>();
            foreach (var o in objs)
            {
                if (o.Children.Count > 0)
                {
                    expandedItems.Add(o);
                }
            }
#endif
            searchFilter = string.Empty;
            lastSearchFilter = string.Empty;

            searchTextField = new GUIStyle(GUI.skin.FindStyle("SearchTextField"));
            searchTextField.normal.textColor = DeepClickerCache.Settings.SearchbarTextColor;

            loader.Initialize(objects);
        }

        public override Vector2 GetWindowSize()
        {
            return DeepClickerCache.Settings.WindowSize;
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
            Rect outline = DrawOutline(rect);
            Rect background = DrawPopupBackground(outline);
            Rect header = DrawHeader(background);
            Rect label = DrawHeaderLabel(header);
            Rect searchBar = DrawSeachBar(header);
            Rect scrollview = DrawScrollview(background, searchBar);
        }

        private Rect DrawOutline(Rect rect)
        {
            ContextMenuDrawHelper.DrawOutline(rect, DeepClickerCache.Settings.OutlineColor);
            return rect;
        }

        private Rect DrawPopupBackground(Rect rect)
        {
            rect = new Rect(
                rect.x + ContextMenuDrawHelper.OUTLINE_THICKNESS,
                rect.y + ContextMenuDrawHelper.OUTLINE_THICKNESS,
                rect.width - ContextMenuDrawHelper.OUTLINE_THICKNESS * 2,
                rect.height - ContextMenuDrawHelper.OUTLINE_THICKNESS * 2
            );

            ContextMenuDrawHelper.DrawBackground(rect, DeepClickerCache.Settings.BodyBackgroundColor);
            return rect;
        }

        private Rect DrawHeader(Rect rect)
        {
            rect = new Rect(rect.x,
                            rect.y,
                            rect.width,
                            ContextMenuDrawHelper.HEADER_HEIGHT);

            ContextMenuDrawHelper.DrawBackground(rect, DeepClickerCache.Settings.HeaderBackgroundColor);
            return rect;
        }

        private Rect DrawHeaderLabel(Rect rect)
        {
            rect = new Rect(rect.x + ContextMenuDrawHelper.VERTICAL_PADDING,
                            rect.y,
                            rect.width,
                            rect.height);

            string label = string.IsNullOrEmpty(searchFilter)
                         ? $"Objects found ({objectsCount})"
                         : $"Objects found ({filteredObjectsCount}/{objectsCount})";

            ContextMenuDrawHelper.DrawHeaderLabel(rect, label, DeepClickerCache.Settings.HeaderTextColor);
            return rect;
        }

        private Rect DrawSeachBar(Rect rect)
        {
            Rect background = new Rect(
                rect.x,
                rect.yMax,
                rect.width,
                ContextMenuDrawHelper.LINE_HEIGHT + ContextMenuDrawHelper.SPACING
            );

            EditorGUI.DrawRect(background, DeepClickerCache.Settings.HeaderBackgroundColor);

            Rect searchBar = new Rect(
                rect.x + ContextMenuDrawHelper.VERTICAL_PADDING,
                rect.yMax + ContextMenuDrawHelper.SPACING,
                rect.width - ContextMenuDrawHelper.VERTICAL_PADDING * ContextMenuDrawHelper.SPACING,
                ContextMenuDrawHelper.LINE_HEIGHT
            );

            ContextMenuDrawHelper.DrawSearchBar(searchBar, searchField, ref searchFilter, searchTextField);
            if (searchFilter != lastSearchFilter)
            {
                lastSearchFilter = searchFilter;
                FilterObjects();
            }

            return searchBar;
        }

        private Rect DrawScrollview(Rect popupRect, Rect searchBarRect)
        {
            float scrollY = searchBarRect.yMax + ContextMenuDrawHelper.SPACING;
            float scrollHeight = popupRect.yMax - scrollY;

            Rect scrollRect = new Rect(
                popupRect.x,
                scrollY,
                popupRect.width,
                scrollHeight
            );

            int visibleRows = CountVisibleRows();
            float contentHeight = visibleRows * ContextMenuDrawHelper.LINE_HEIGHT;

            Rect viewRect = new Rect(
                0,
                0,
                scrollRect.width - 16,
                contentHeight
            );

            verticalPos = GUI.BeginScrollView(scrollRect, verticalPos, viewRect);

            var rowStyle = new ContextMenuDrawHelper.RowStyle()
            {
                Background = DeepClickerCache.Settings.RowBackgroundColor,
                Hover = DeepClickerCache.Settings.RowHoverColor,
                Selected = DeepClickerCache.Settings.RowSelectedColor,
                Text = DeepClickerCache.Settings.RowTextColor
            };

            int rowIndex = 0;

            if (string.IsNullOrEmpty(searchFilter))
            {
                foreach (var root in rootObjects)
                {
                    DrawTreeItem(root, 0, rowStyle, ref rowIndex, viewRect.width);
                }
            }
            else
            {
                foreach (var obj in filteredObjects)
                {
                    bool expanded = false;

                    Rect rowRect = new Rect(
                        0,
                        rowIndex * ContextMenuDrawHelper.LINE_HEIGHT,
                        viewRect.width,
                        ContextMenuDrawHelper.LINE_HEIGHT
                    );

                    ContextMenuDrawHelper.DrawRow(
                        rowRect,
                        obj.Item,
                        0,
                        false,
                        ref expanded,
                        rowStyle,
                        SelectObject
                    );

                    rowIndex++;
                }
            }

            GUI.EndScrollView();
            return scrollRect;
        }

        private void SelectObject(UnityEngine.Object obj)
        {
            Selection.activeGameObject = obj as GameObject;
            EditorGUIUtility.PingObject(obj);
        }

        /// <summary>
        /// Draw an expandable tree item
        /// </summary>
        private void DrawTreeItem(
            QueryableItem item,
            int indentLevel,
            ContextMenuDrawHelper.RowStyle style,
            ref int rowIndex,
            float width)
        {
            bool hasChildren = item.Children != null && item.Children.Count > 0;

            Rect rowRect = new Rect(
                0,
                rowIndex * ContextMenuDrawHelper.LINE_HEIGHT,
                width,
                ContextMenuDrawHelper.LINE_HEIGHT
            );

            bool isExpanded = expandedItems.Contains(item);
            ContextMenuDrawHelper.DrawRow(
                rowRect,
                item.Item,
                indentLevel,
                hasChildren,
                ref isExpanded,
                style,
                SelectObject
            );

            if (isExpanded)
            {
                expandedItems.Add(item);
            }
            else
            {
                expandedItems.Remove(item);
            }

            rowIndex++;
            if (hasChildren && isExpanded)
            {
                foreach (var child in item.Children)
                {
                    DrawTreeItem(child, indentLevel + 1, style, ref rowIndex, width);
                }
            }
        }

        /// <summary>
        /// Count every visible row
        /// </summary>
        private int CountVisibleRows()
        {
            int total = 0;

            foreach (var root in rootObjects)
            {
                total += CountVisibleRows(root);
            }

            return total;
        }

        /// <summary>
        /// Count every visible row
        /// </summary>
        private int CountVisibleRows(QueryableItem item)
        {
            int count = 1;

            if (expandedItems.Contains(item))
            {
                foreach (var child in item.Children)
                {
                    count += CountVisibleRows(child);
                }
            }

            return count;
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

            int filterCount = DeepClickerCache.Filters.Count;
            for (int i = 0; i < filterCount; i++)
            {
                IFilterable filterable = DeepClickerCache.Filters[i];
                if (!filterable.Evaluate(this))
                {
                    continue;
                }

                filterWord = searchFilter.Substring(filterable.FilterKeyword.Length).Trim();
                filteredObjects = filterable.Filter(this);
                break;
            }

            filteredObjects = filteredObjects.OrderBy(o => o.SiblingIndex);
            filteredObjectsCount = filteredObjects.Count();
        }
    }
}