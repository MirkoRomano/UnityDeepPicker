using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sparkling.SceneFinder
{
    public interface ILazyLoadable
    {
        void Load();
    }

    public sealed class QueryableItem : ILazyLoadable
    {
        public UnityEngine.Object Item => m_item;
        public UnityObjectType ObjectType => m_type;
        public string Name => m_name;
        public string Tag => m_tag;

        private UnityEngine.Object m_item;
        private UnityObjectType m_type;

        private string m_name;
        private string m_tag;

        private readonly HashSet<Component> m_componentsMap;
        private readonly HashSet<string> m_componentsNameMap;
        private readonly HashSet<string> m_labelsMap;

        public QueryableItem()
        {
            m_componentsMap = new HashSet<Component>();
            m_componentsNameMap = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            m_labelsMap = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void Initialize(UnityEngine.Object item)
        {
            if(item == null)
            {
                throw new Exception("Cannot create a quaryable item with a null object");
            }

            m_item = item;
            m_type = item.GetUnityType();
            m_name = item.name;

            if(m_item is GameObject go)
            {
                m_tag = go.tag;
            }
        }

        void ILazyLoadable.Load()
        {
            if(m_type == UnityObjectType.GameObject)
            {
                GameObject go = As<GameObject>();

                foreach (Component component in go.GetComponents<Component>())
                {
                    m_componentsMap.Add(component);

                    Type type = component.GetType();

                    while (type != null)
                    {
                        m_componentsNameMap.Add(type.Name);
                        type = type.BaseType;
                    }
                }
            }

            foreach (string label in AssetDatabase.GetLabels(m_item))
            {
                m_labelsMap.Add(label);
            }
        }

        public void Free()
        {
            m_item = null;
            m_type = default;
            m_name = null;
            m_tag = null;

            m_componentsMap.Clear();
            m_componentsNameMap.Clear();
            m_labelsMap.Clear();
        }

        public T As<T>() where T : class
        {
            return m_item as T;
        }

        public bool Is<T>() where T : class
        {
            return (m_item is T);
        }

        public bool HasComponent(Component component)
        {
            return m_componentsMap.Contains(component);
        }

        public bool HasComponent(string name)
        {
            return m_componentsNameMap.Contains(name);
        }

        public bool HasLabel(string label)
        {
            return m_labelsMap.Contains(label);
        }

        public bool HasTag(string tag)
        {
            return string.Equals(m_tag, tag, StringComparison.OrdinalIgnoreCase);
        }

        public bool NameContains(string name)
        {
            return m_name.Contains(name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (obj is QueryableItem other)
            {
                return m_item == other.m_item;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return m_item != null ? m_item.GetHashCode() : 0;
        }
    }
}
