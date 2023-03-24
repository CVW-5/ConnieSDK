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
        public Vector3Json Position;
        [JsonInclude]
        public QuaternionJson Rotation;
        [JsonInclude]
        public Vector3Json Scale;

        [JsonInclude]
        public WrappedComponent[] Components;

        [JsonInclude]
        public TransformWrapper[] Children;

        [JsonIgnore]
        public bool IsEmpty => Components.Length == 0 && Children.Length == 0;

        public TransformWrapper(Transform original, int maxDepth = 5, bool isRoot = false, bool includeEmpty = false)
        {
            Name = original.name;
            Position = new Vector3Json(isRoot ? Vector3.zero : original.localPosition);
            Rotation = new QuaternionJson(isRoot ? Quaternion.identity : original.localRotation);
            Scale = new Vector3Json(1, 1, 1);

            Components = CollectComponents(original, false);
            Children = CollectChildren(original, maxDepth - 1, includeEmpty);
        }

        private WrappedComponent[] CollectComponents(Transform original, bool logInvalid = false)
        {
            Component[] comps = original.GetComponents(typeof(Component));
            List<WrappedComponent> validComps = new List<WrappedComponent>();

            foreach (Component c in comps)
            {
                WrappedComponent? wrappedComponent = ComponentWrapper.Wrap(c);

                if(wrappedComponent is not null)
                {
                    validComps.Add(wrappedComponent);
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
                var newWrapper = new TransformWrapper(t, maxDepth);

                if (!newWrapper.IsEmpty || includeEmpty)
                    wrapped.Add(newWrapper);
            }

            return wrapped.ToArray();
        }
    }
}
