using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sparkling.SceneFinder
{
    [InitializeOnLoad]
    public static class SceneViewCache
    {
        public static SimplePool<QueryableItem> QueryablePool;
        public static ScriptableFinderSettings Settings;

        static SceneViewCache()
        {
            LoadSettiings();
            InstantiatePool();
        }

        public static void LoadSettiings()
        {
            string[] settingsGuid = AssetDatabase.FindAssets("ViewFinderSettings");

            if (settingsGuid.Length > 1) 
            {
                Debug.LogWarning("More than one Scene view finder setting file has been found");
                Debug.LogWarning("Please check in you project folder");
            }

            foreach (string guid in settingsGuid)
            {
                var settingFile = AssetDatabase.LoadAssetByGUID<ScriptableFinderSettings>(new GUID(guid));

                if (settingFile != null) 
                {
                    Settings = settingFile;
                    break;
                }
            }
        }

        public static void InstantiatePool()
        {
            if (QueryablePool == null) 
            {
                QueryablePool = new SimplePool<QueryableItem>(Settings.MaxShowableItemCount);
                QueryablePool.Prepopulate(Settings.MaxShowableItemCount);
            }
            else
            {
                QueryablePool.Resize(Settings.MaxShowableItemCount);
            }
        }
    }
}