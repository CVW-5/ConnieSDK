using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using ConnieSDK.Meshes;

#nullable enable
namespace ConnieSDK
{
    //[JsonKnownType(typeof(LightJson), "Light")]
    //[JsonDerivedType(typeof(ColliderJson), "Collider")]
    //[JsonDerivedType(typeof(AudioJson), "Audio")]
    //[JsonDerivedType(typeof(MeshRendererJson), "Mesh")]
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

        public abstract void Attach(Transform to);

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

            [JsonConstructor]
            public LightJson(LightType Type, Color Color, float Radius, float Intensity)
            {
                this.Type = Type;
                this.Color = Color;
                this.Radius = Radius;
                this.Intensity = Intensity;
            }

            public override void Attach(Transform to)
            {
                Light l = (Light)to.gameObject.AddComponent(typeof(Light));

                l.type = Type;
                l.color = Color;
                l.range = Radius;
                l.intensity = Intensity;
            }
        }

        public class ColliderJson: WrappedComponent
        {
            public ColliderJson(Collider c)
            {

            }

            [JsonConstructor]
            public ColliderJson()
            {

            }

            public override void Attach(Transform to)
            {
                //throw new System.NotImplementedException();
                Debug.LogError("Unable to Deserialize a collider - they're not yet implemented");
            }
        }

        public class AudioJson: WrappedComponent
        {
            public AudioClip? Clip;

            public AudioJson(AudioSource a)
            {

            }

            [JsonConstructor]
            public AudioJson(AudioClip? Clip)
            {
                this.Clip = Clip;
            }

            public override void Attach(Transform to)
            {
                throw new System.NotImplementedException();
            }
        }

        public class MeshRendererJson: WrappedComponent
        {
            [JsonIgnore]
            public Mesh? BaseMesh;

            public string MeshName = string.Empty;

            public MeshRendererJson (MeshRenderer mr)
            {
                MeshFilter mf = (MeshFilter)mr.GetComponent(typeof(MeshFilter));

                BaseMesh = mf.sharedMesh;
            }

            [JsonConstructor]
            public MeshRendererJson(string MeshName)
            {
                this.MeshName = MeshName;
            }

            public void StoreMesh (MeshCollection? collection, string name)
            {
                if (BaseMesh is null || collection is null)
                    return;

                collection[name] = BaseMesh;
                MeshName = name;
            }

            public override void Attach(Transform to)
            {
                MeshFilter mf = (MeshFilter)to.gameObject.AddComponent(typeof(MeshFilter));
                MeshRenderer mr = (MeshRenderer)to.gameObject.AddComponent(typeof(MeshRenderer));

                mf.sharedMesh = MeshLibrary.Current?[MeshName];
                mr.sharedMaterial = Resources.Load("MISSING_MATERIAL") as Material;
            }
        }
    }
}
