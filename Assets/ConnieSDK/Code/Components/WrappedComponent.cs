using System.Collections;
using System.Text.Json.Serialization;
using UnityEngine;

#nullable enable
namespace ConnieSDK
{
    [JsonDerivedType(typeof(LightJson), "Light")]
    [JsonDerivedType(typeof(ColliderJson), "Collider")]
    [JsonDerivedType(typeof(AudioJson), "Audio")]
    [JsonDerivedType(typeof(MeshRendererJson), "Mesh")]
    public abstract class WrappedComponent
    {
        public static WrappedComponent? Auto (Component comp)
        {
            return comp switch
            {
                Light l => new LightJson(l),
                Collider c and not MeshCollider => new ColliderJson(c),
                AudioSource audio => new AudioJson(audio),
                MeshRenderer mesh => new MeshRendererJson(mesh),
                _ => null
            };
        }

        public class LightJson : WrappedComponent
        {
            public LightType Type;
            public Color Color;
            public float Radius;
            public float Intensity;

            public LightJson(Light l)
            {
                Type = l.type;
                Color = l.color;
                Radius = l.range;
                Intensity = l.intensity;
            }
        }

        public class ColliderJson: WrappedComponent
        {
            public ColliderJson(Collider c)
            {

            }
        }

        public class AudioJson: WrappedComponent
        {
            public AudioClip? Clip;

            public AudioJson(AudioSource a)
            {

            }
        }

        public class MeshRendererJson: WrappedComponent
        {
            public Mesh? BaseMesh;

            public MeshRendererJson (MeshRenderer mr)
            {

            }
        }
    }
}
