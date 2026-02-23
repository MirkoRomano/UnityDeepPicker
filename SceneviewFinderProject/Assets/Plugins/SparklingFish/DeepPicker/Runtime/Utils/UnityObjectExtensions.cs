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
            return obj switch
            {
                null => UnityObjectType.Unknown,
                GameObject => UnityObjectType.GameObject,
                Component => UnityObjectType.Component,
                Material => UnityObjectType.Material,
                Texture => UnityObjectType.Texture,
                AnimationClip => UnityObjectType.AnimationClip,
                AudioClip => UnityObjectType.AudioClip,
                Mesh => UnityObjectType.Mesh,
                ScriptableObject => UnityObjectType.ScriptableObject,
                Shader => UnityObjectType.Shader,
                _ => UnityObjectType.Unknown
            };
        }
    }
}