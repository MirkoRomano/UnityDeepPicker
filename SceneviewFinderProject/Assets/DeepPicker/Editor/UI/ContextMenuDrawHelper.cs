using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Sparkling.DeepClicker
{
    internal static class ContextMenuDrawHelper
    {
        public struct RowStyle
        {
            public Color Background;
            public Color Hover;
            public Color Selected;
            public Color Text;
        }

        public const string ELLIPSIS = "...";
        public const float ICON_WIDTH = 20f;
        public const float TRUNCATE_PADDING = 10f;
        public const float INDENT_WIDTH = 15f;
        public const float LINE_HEIGHT = 20f;
        public const float LEFT_MARGIN = 5f;
        public const float VERTICAL_PADDING = 6f;
        public const float LEFT_PANEL_WIDTH = 320f;
        public const float PREVIEW_PADDING = 12f;
        public const float HEADER_HEIGHT = 22f;
        public const float FOLDOUT_WIDTH = 14f;
        public const float SPACING = 4f;
        public const float OUTLINE_THICKNESS = 2f;
        public const float BUTTON_HEIGHT = 35f;

        public static void DrawOutline(Rect rect, Color color) => EditorGUI.DrawRect(rect, color);

        public static void DrawBackground(Rect rect, Color color) => EditorGUI.DrawRect(rect, color);

        public static void DrawHeader(Rect rect, Color color) => EditorGUI.DrawRect(rect, color);

        public static void DrawHeaderLabel(Rect rect, string text, Color color)
        {
            Color old = GUI.contentColor;
            GUI.contentColor = color;
            GUI.Label(rect, text);
            GUI.contentColor = old;
        }

        public static void DrawSearchBar(Rect rect, SearchField bar, ref string text, GUIStyle textField)
        {
            text = bar.OnGUI(rect, text, textField,
                                         GUI.skin.FindStyle("SearchCancelButton"), 
                                         GUI.skin.FindStyle("SearchCancelButtonEmpty"));
        }

        public static bool DrawRow(Rect rowRect,
        UnityEngine.Object item,
        int indentLevel,
        bool hasChildren,
        ref bool isExpanded,
        RowStyle style,
        Action<UnityEngine.Object> onClick)
        {
            if (item == null)
            {
                return false;
            }

            Event e = Event.current;
            bool hovered = rowRect.Contains(e.mousePosition);

            if (Selection.activeGameObject == item)
            {
                EditorGUI.DrawRect(rowRect, style.Selected);
            }
            else if (hovered)
            {
                EditorGUI.DrawRect(rowRect, style.Hover);
            }
            else
            {
                EditorGUI.DrawRect(rowRect, style.Background);
            }

            float indent = indentLevel * INDENT_WIDTH;
            Rect foldoutRect = new Rect(
                rowRect.x + indent,
                rowRect.y,
                FOLDOUT_WIDTH,
                rowRect.height
            );

            if (hasChildren)
            {
                isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, GUIContent.none);
            }

            GUIContent content = EditorGUIUtility.ObjectContent(item, item.GetType());

            Rect labelRect = new Rect(
                rowRect.x + indent + foldoutRect.width,
                rowRect.y,
                rowRect.width - indent,
                rowRect.height
            );

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.normal.textColor = style.Text;
            float availableWidth = labelRect.width - ICON_WIDTH - TRUNCATE_PADDING;

            if (labelStyle.CalcSize(new GUIContent(content.text)).x > availableWidth)
            {
                content.text = TruncateText(content.text, availableWidth, labelStyle);
            }

            GUI.Label(labelRect, content, labelStyle);

            if (e.type == EventType.MouseDown && e.button == 0 && hovered)
            {
                e.Use();
                onClick?.Invoke(item);
            }

            return isExpanded;
        }


        private static string TruncateText(string text, float maxWidth, GUIStyle style)
        {
            while (text.Length > 0)
            {
                string temp = text + ELLIPSIS;
                if (style.CalcSize(new GUIContent(temp)).x <= maxWidth)
                {
                    return temp;
                }

                text = text.Substring(0, text.Length - 1);
            }

            return ELLIPSIS;
        }
    }
}