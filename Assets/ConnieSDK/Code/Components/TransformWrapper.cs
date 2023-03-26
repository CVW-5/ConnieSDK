using System.Collections.Generic;
using System.Text.Json.Serialization;
using UnityEngine;

#nullable enable
namespace ConnieSDK.Components
{
    public class TransformWrapper
    {
        [JsonInclude]
        public string Name;
        [JsonInclude]
        public Vector3 Position;
        [JsonInclude]
        public Vector3 Rotation;
        [JsonInclude]
        public Vector3 Scale;

        [JsonInclude]
        public WrappedComponent[] Components;

        [JsonInclude]
        public TransformWrapper[] Children;

        [JsonIgnore]
        public bool IsEmpty => Components.Length == 0 && Children.Length == 0;

        [JsonIgnore]
        private Meshes.MeshCollection? MeshCollection;

        public TransformWrapper(Transform original, int maxDepth = 5, bool isRoot = false, bool includeEmpty = false, Meshes.MeshCollection? MeshCollection = null)
        {
            this.MeshCollection = MeshCollection;

            Name = original.name;
            Position = isRoot ? Vector3.zero : original.localPosition;
            Rotation = isRoot ? Vector3.zero : original.localEulerAngles;
            Scale = isRoot ? Vector3.one : original.localScale;

            Components = CollectComponents(original, false);
            Children = CollectChildren(original, maxDepth - 1, includeEmpty);
        }

        [JsonConstructor]
        public TransformWrapper (string Name, Vector3 Position, Vector3 Rotation, Vector3 Scale, WrappedComponent[] Components, TransformWrapper[] Children)
        {
            this.Name = Name;
            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;
            this.Components = Components;
            this.Children = Children;
        }

        private WrappedComponent[] CollectComponents(Transform original, bool logInvalid = false)
        {
            Component[] comps = original.GetComponents(typeof(Component));
            List<WrappedComponent> validComps = new List<WrappedComponent>();

            foreach (Component c in comps)
            {
                WrappedComponent? wrappedComponent = WrappedComponent.Auto(c);

                if(wrappedComponent is not null)
                {
                    validComps.Add(wrappedComponent);
                }
                if(wrappedComponent is WrappedComponent.MeshRendererJson mrj)
                {
                    mrj.StoreMesh(MeshCollection, Name);
                }
            }

            return validComps.ToArray();
        }

        private TransformWrapper[] CollectChildren(Transform original, int maxDepth, bool includeEmpty)
        {
            Transform[] children = original.GetChildren();
            List<TransformWrapper> wrapped = new List<TransformWrapper>();

            foreach(Transform t in children)
            {
                var newWrapper = new TransformWrapper(t, maxDepth, MeshCollection:MeshCollection);

                if (!newWrapper.IsEmpty || includeEmpty)
                    wrapped.Add(newWrapper);
            }

            return wrapped.ToArray();
        }

        public Transform GenerateGameobjects (Transform? parent = null)
        {
            GameObject me = new GameObject(Name);
            Transform tr = me.transform;

            if (parent is Transform)
                tr.SetParent(parent);

            tr.localPosition = Position;
            tr.localEulerAngles = Rotation;
            tr.localScale = Scale;

            foreach(WrappedComponent wc in Components)
            {
                wc.Attach(tr);
            }

            foreach(TransformWrapper tw in Children)
            {
                tw.GenerateGameobjects(tr);
            }

            return tr;
        }
    }
}
