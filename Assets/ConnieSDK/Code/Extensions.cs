using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConnieSDK
{
    public static class Extensions
    {
        public static Transform[] GetChildren (this Transform tr)
        {
            List<Transform> children = new List<Transform>();
            int expected = tr.childCount;

            for (int i = 0; i < expected; i++)
            {
                children.Add(tr.GetChild(i));
            }

            return children.ToArray();
        }
    }
}
