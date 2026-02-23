using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sparkling.SceneFinder
{
    public class DeepPickerSettingsWindow : EditorWindow
    {
        private DeepPickerSettings settings;
        private SearchField searchField;

        private bool showWindowSettings = true;
        private bool showColorSettings = true;
        private string searchFilter = string.Empty;

        private List<GameObject> activeSceneObjects;
        private Vector2 horizontalPos;
        private Vector2 verticalPos;

        [MenuItem("Tools/Sparkling/SceneViewFinder")]
        public static void Open()
        {
            var window = GetWindow<DeepPickerSettingsWindow>();
            window.titleContent = new GUIContent("SceneView Finder");
            window.minSize = new Vector2(550, 380);
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPreviewPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void Initialize()
        {
            settings = new DeepPickerSettings(DeepClickerCache.Settings);
            searchField = new SearchField();
            activeSceneObjects = GetAllSceneObjects();
        }

        public static List<GameObject> GetAllSceneObjects()
        {
            Scene scene = SceneManager.GetActiveScene();
            List<GameObject> result = new List<GameObject>();

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    result.Add(t.gameObject);
                }
            }

            return result;
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(ContextMenuDrawHelper.LEFT_PANEL_WIDTH));

            horizontalPos = EditorGUILayout.BeginScrollView(horizontalPos);

            GUILayout.Space(6);
            EditorGUILayout.LabelField("SceneView Finder Settings", EditorStyles.boldLabel);
            GUILayout.Space(8);

            DrawSettingsFoldouts();

            GUILayout.Space(15);

            EditorGUILayout.EndScrollView();

            DrawSaveButton();
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
        }

        private void DrawSettingsFoldouts()
        {
            showWindowSettings = EditorGUILayout.Foldout(showWindowSettings, "Window Settings", true);
            if (showWindowSettings)
            {
                EditorGUI.indentLevel++;

                settings.WindowSize = EditorGUILayout.Vector2Field("Popup Window Size", settings.WindowSize);
                settings.MaxShowableItemCount = EditorGUILayout.IntSlider("Max Items", settings.MaxShowableItemCount, 10, 200);

                EditorGUI.indentLevel--;
                GUILayout.Space(8);
            }

            showColorSettings = EditorGUILayout.Foldout(showColorSettings, "Colors", true);
            if (showColorSettings)
            {
                EditorGUI.indentLevel++;

                settings.HeaderBackgroundColor = EditorGUILayout.ColorField("Header Background", settings.HeaderBackgroundColor);
                settings.HeaderTextColor = EditorGUILayout.ColorField("Header Text", settings.HeaderTextColor);
                settings.SearchbarTextColor = EditorGUILayout.ColorField("Search Bar Text", settings.SearchbarTextColor);
                settings.BodyBackgroundColor = EditorGUILayout.ColorField("Body Background", settings.BodyBackgroundColor);
                settings.OutlineColor = EditorGUILayout.ColorField("Body Outline", settings.OutlineColor);
                settings.RowBackgroundColor = EditorGUILayout.ColorField("Row Background", settings.RowBackgroundColor);
                settings.RowHoverColor = EditorGUILayout.ColorField("Row Hover", settings.RowHoverColor);
                settings.RowSelectedColor = EditorGUILayout.ColorField("Row Selected", settings.RowSelectedColor);
                settings.RowTextColor = EditorGUILayout.ColorField("Row Text", settings.RowTextColor);

                EditorGUI.indentLevel--;
            }
        }

        private void DrawSaveButton()
        {
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", EditorStyles.miniButton, GUILayout.Height(ContextMenuDrawHelper.BUTTON_HEIGHT)))
            {
                SaveSettings();
            }

            if (GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.Height(ContextMenuDrawHelper.BUTTON_HEIGHT)))
            {
                ResetSettings();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void SaveSettings()
        {
            if (DeepClickerCache.Settings == null)
            {
                DeepClickerCache.LoadSettings();
            }

            DeepClickerCache.SaveSettings(settings);
        }

        private void ResetSettings()
        {
            settings.Set(DeepClickerCache.Settings);
            Repaint();
        }

        private void DrawRightPreviewPanel()
        {
            EditorGUILayout.BeginVertical();

            GUILayout.Space(6);
            EditorGUILayout.LabelField("Live Preview", EditorStyles.boldLabel);
            GUILayout.Space(8);

            Rect previewRect = GUILayoutUtility.GetRect(
                10,
                10000,
                10,
                10000,
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true)
            );


            DrawContextMenuPreview(previewRect);

            EditorGUILayout.EndVertical();
        }

        private void DrawContextMenuPreview(Rect rect)
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
            rect = new Rect(
                rect.x + ContextMenuDrawHelper.PREVIEW_PADDING,
                rect.y + ContextMenuDrawHelper.PREVIEW_PADDING,
                settings.WindowSize.x,
                settings.WindowSize.y
            );

            ContextMenuDrawHelper.DrawOutline(rect, settings.OutlineColor);
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

            ContextMenuDrawHelper.DrawBackground(rect, settings.BodyBackgroundColor);
            return rect;
        }

        private Rect DrawHeader(Rect rect)
        {
            rect = new Rect(rect.x,
                            rect.y,
                            rect.width,
                            ContextMenuDrawHelper.HEADER_HEIGHT);

            ContextMenuDrawHelper.DrawBackground(rect, settings.HeaderBackgroundColor);
            return rect;
        }

        private Rect DrawHeaderLabel(Rect rect)
        {
            rect = new Rect(rect.x + ContextMenuDrawHelper.VERTICAL_PADDING,
                            rect.y,
                            rect.width,
                            rect.height);

            ContextMenuDrawHelper.DrawHeaderLabel(rect, $"Objects found ({activeSceneObjects.Count})", settings.HeaderTextColor);
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

            EditorGUI.DrawRect(background, settings.HeaderBackgroundColor);

            Rect searchBar = new Rect(
                rect.x + ContextMenuDrawHelper.VERTICAL_PADDING,
                rect.yMax + ContextMenuDrawHelper.SPACING,
                rect.width - ContextMenuDrawHelper.VERTICAL_PADDING * ContextMenuDrawHelper.SPACING,
                ContextMenuDrawHelper.LINE_HEIGHT
            );

            GUIStyle searchTextField = new GUIStyle(GUI.skin.FindStyle("SearchTextField"));
            searchTextField.normal.textColor = settings.SearchbarTextColor;
            ContextMenuDrawHelper.DrawSearchBar(searchBar, searchField, ref searchFilter, searchTextField);
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

            int max = Mathf.Min(activeSceneObjects.Count, settings.MaxShowableItemCount);
            float contentHeight = max * ContextMenuDrawHelper.LINE_HEIGHT;

            Rect viewRect = new Rect(
                0,
                0,
                scrollRect.width - 16,
                contentHeight
            );

            verticalPos = GUI.BeginScrollView(scrollRect, verticalPos, viewRect);

            for (int i = 0; i < max; i++)
            {
                Rect rowRect = new Rect(
                    0,
                    i * ContextMenuDrawHelper.LINE_HEIGHT,
                    viewRect.width,
                    ContextMenuDrawHelper.LINE_HEIGHT
                );

                var rowStyle = new ContextMenuDrawHelper.RowStyle()
                {
                    Background = settings.RowBackgroundColor,
                    Hover = settings.RowHoverColor,
                    Selected = settings.RowSelectedColor,
                    Text = settings.RowTextColor
                };

                bool expanded = false;
                ContextMenuDrawHelper.DrawRow(rowRect, activeSceneObjects[i], 0, false, ref expanded, rowStyle, SelectObject);
            }

            GUI.EndScrollView();

            return scrollRect;
        }

        private void SelectObject(Object obj)
        {
            Selection.activeGameObject = obj as GameObject;
        }
    }
}
