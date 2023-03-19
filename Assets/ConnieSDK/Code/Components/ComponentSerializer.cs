using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConnieSDK
{
    public static class ComponentSerializer
    {
        public static string SerializeToJson (Component comp)
        {
            return comp switch
            {
                Transform => TransformSerializer.Serialize(comp as Transform),
                _ => throw new System.NotImplementedException($"ConnieSDK does not support serialization of type {comp.GetType()}")
            };
        }
    }
}
