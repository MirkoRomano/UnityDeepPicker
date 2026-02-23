using UnityEngine;

namespace Sparkling.DeepClicker
{
    public enum UnityObjectType
    {
        Unknown,
        GameObject,
        Component,
        Material,
        Texture,
        AnimationClip,
        AudioClip,
        Mesh,
        ScriptableObject,
        Shader
    }

    public static class UnityObjectExtensions
    {
        public static UnityObjectType GetUnityType(this Object obj)
        {
            if (obj == null) return UnityObjectType.Unknown;
            if (obj is GameObject) return UnityObjectType.GameObject;
            if (obj is Component) return UnityObjectType.Component;
            if (obj is Material) return UnityObjectType.Material;
            if (obj is Texture) return UnityObjectType.Texture;
            if (obj is AnimationClip) return UnityObjectType.AnimationClip;
            if (obj is AudioClip) return UnityObjectType.AudioClip;
            if (obj is Mesh) return UnityObjectType.Mesh;
            if (obj is ScriptableObject) return UnityObjectType.ScriptableObject;
            if (obj is Shader) return UnityObjectType.Shader;
            return UnityObjectType.Unknown;
        }
    }
}