using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace ConnieSDK
{
    public static class ComponentWrapper
    {
        public static WrappedComponent? Wrap (Component comp)
        {
            return comp switch
            {
                Light l => new WrappedComponent.LightJson(l),
                Collider c and not MeshCollider => new WrappedComponent.ColliderJson(c),
                AudioSource audio => new WrappedComponent.AudioJson(audio),
                MeshRenderer mesh => new WrappedComponent.MeshRendererJson(mesh),
                _ => null
            };
        }
    }
}
