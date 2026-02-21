using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sparkling.SceneFinder
{
    [InitializeOnLoad]
    public static class DeepClickerCache
    {
        private static string SaveDirectory => Path.GetDirectoryName(SavePath);
        private static string SavePath => Path.Combine(Application.persistentDataPath, "SceneViewFinder", "Data.json");
        public static SimplePool<QueryableItem> QueryablePool;
        public static DeepPickerSettings Settings = new DeepPickerSettings();
        public static List<IFilterable> Filters;

        static DeepClickerCache()
        {
            LoadSettings();
            InstantiatePool();
            LoadFilters();
        }

        public static void LoadSettings()
        {
            if (!File.Exists(SavePath))
            {
                Settings = DeepPickerSettings.DEFAULT;
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                JsonUtility.FromJsonOverwrite(json, Settings);
            }
            catch (Exception)
            {
                Settings = DeepPickerSettings.DEFAULT;
            }
        }

        public static void SaveSettings(DeepPickerSettings settings)
        {
            if(settings == null)
            {
                return;
            }

            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(SavePath, json);
            JsonUtility.FromJsonOverwrite(json, Settings);
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