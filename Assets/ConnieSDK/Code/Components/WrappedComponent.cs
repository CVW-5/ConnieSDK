using System.Collections;
using System.Text.Json.Serialization;

namespace ConnieSDK
{
    [JsonDerivedType(typeof(LightJson), "Light")]
    [JsonDerivedType(typeof(ColliderJson), "Collider")]
    [JsonDerivedType(typeof(AudioJson), "Audio")]
    [JsonDerivedType(typeof(MeshRendererJson), "Mesh")]
    public abstract class WrappedComponent
    {
        public class LightJson : WrappedComponent
        {
            public UnityEngine.LightType Type;
            public UnityEngine.Color Color;
            public float Radius;
            public float Intensity;

            public LightJson(UnityEngine.Light l)
            {
                Type = l.type;
                Color = l.color;
                Radius = l.range;
                Intensity = l.intensity;
            }
        }

        public class ColliderJson: WrappedComponent
        {
            public ColliderJson(UnityEngine.Collider c)
            {

            }
        }

        public class AudioJson: WrappedComponent
        {
            public UnityEngine.AudioClip Clip;

            public AudioJson(UnityEngine.AudioSource a)
            {

            }
        }

        public class MeshRendererJson: WrappedComponent
        {
            public UnityEngine.Mesh BaseMesh;

            public MeshRendererJson (UnityEngine.MeshRenderer mr)
            {

            }
        }
    }
}
