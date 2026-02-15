using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sparkling.SceneFinder
{
    [InitializeOnLoad]
    public static class SceneViewCache
    {
        public static SimplePool<QueryableItem> QueryablePool;
        public static ScriptableFinderSettings Settings;
        public static List<IFilterable> Filters;

        static SceneViewCache()
        {
            LoadSettiings();
            InstantiatePool();
            LoadFilters();
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

        public static void LoadFilters()
        {
            if(Filters == null)
            {
                Filters = new List<IFilterable>();
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.FullName;
                if (name.StartsWith("System") || name.StartsWith("Unity") || name.StartsWith("mscorlib"))
                    continue;

                try
                {
                    var types = assembly.GetTypes();
                    var filterableTypes = types.Where(t =>
                        typeof(IFilterable).IsAssignableFrom(t) &&
                        !t.IsInterface &&
                        !t.IsAbstract
                    );

                    foreach (var type in filterableTypes)
                    {
                        if (Activator.CreateInstance(type) is IFilterable filter)
                        {
                            Filters.Add(filter);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            Filters = Filters.OrderBy(f => f.FilterIndex).ThenByDescending(f => f.FilterKeyword.Length).ToList();
        }
    }
}