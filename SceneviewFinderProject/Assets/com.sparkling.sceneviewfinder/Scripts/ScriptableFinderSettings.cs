using UnityEngine;

namespace Sparkling.SceneFinder
{
    [System.Serializable]
    public class DeepPickerSettings
    {
        public Vector2 WindowSize;
        public int MaxShowableItemCount;

        public Color HeaderBackgroundColor;
        public Color HeaderTextColor;

        public Color SearchbarTextColor;

        public Color BodyBackgroundColor;
        public Color RowBackgroundColor;
        public Color OutlineColor;

        public Color RowHoverColor;
        public Color RowSelectedColor;
        public Color RowTextColor;

        public DeepPickerSettings()
        {

        }

        public DeepPickerSettings(DeepPickerSettings settings)
        {
            Set(settings);
        }

        public void Set(DeepPickerSettings settings)
        {
            WindowSize = settings.WindowSize;
            MaxShowableItemCount = settings.MaxShowableItemCount;

            HeaderBackgroundColor = settings.HeaderBackgroundColor;
            HeaderTextColor = settings.HeaderTextColor;

            SearchbarTextColor = settings.SearchbarTextColor;

            BodyBackgroundColor = settings.BodyBackgroundColor;
            RowBackgroundColor = settings.RowBackgroundColor;
            OutlineColor = settings.OutlineColor;

            RowHoverColor = settings.RowHoverColor;
            RowSelectedColor = settings.RowSelectedColor;
            RowTextColor = settings.RowTextColor;
        }

        public static DeepPickerSettings DEFAULT => new DeepPickerSettings()
        {
            WindowSize = new Vector2(200, 250),
            MaxShowableItemCount = 50,
            HeaderBackgroundColor = new Color(0.16f, 0.16f, 0.16f),
            HeaderTextColor = Color.white,
            SearchbarTextColor = Color.white,
            BodyBackgroundColor = new Color(0.18f, 0.18f, 0.18f),
            RowBackgroundColor = new Color(0.22f, 0.22f, 0.22f),
            OutlineColor = Color.white,
            RowHoverColor = new Color(0.26f, 0.26f, 0.26f, 1f),
            RowSelectedColor = new Color(0.24f, 0.48f, 0.90f, 1f),
            RowTextColor = Color.white
        };
    }
}
