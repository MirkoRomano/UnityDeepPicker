using UnityEngine;

namespace Sparkling.DeepClicker
{
    [System.Serializable]
    internal class DeepPickerSettings
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

        internal DeepPickerSettings()
        {

        }

        internal DeepPickerSettings(DeepPickerSettings settings)
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
            WindowSize = new Vector2(250, 250),
            MaxShowableItemCount = 50,
            HeaderBackgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f),
            HeaderTextColor = Color.white,
            SearchbarTextColor = Color.white,
            BodyBackgroundColor = new Color(0.26f, 0.26f, 0.26f, 1f),
            RowBackgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f),
            OutlineColor = Color.white,
            RowHoverColor = new Color(0.26f, 0.26f, 0.26f, 1f),
            RowSelectedColor = new Color(0.24f, 0.48f, 0.90f, 1f),
            RowTextColor = Color.white
        };
    }
}