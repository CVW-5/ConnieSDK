using System.Text.Json.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace ConnieSDK.Components
{
    /// <summary>
    /// A little wrapper for Vector3s during serialization to avoid silly recursion issues. Pending better understanding in the future.
    /// </summary>
    public struct Vector3Json
    {
        public float X, Y, Z;

        public Vector3Json(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3Json(Vector3 original)
        {
            X = original.x;
            Y = original.y;
            Z = original.z;
        }

        public Vector3 Revert ()
        {
            return new Vector3(X, Y, Z);
        }
    }

    /// <summary>
    /// A little wrapper for Quaternions during serialization to avoid silly recursion issues. Pending better understanding in the future.
    /// </summary>
    public struct QuaternionJson
    {
        public float X, Y, Z, W;

        public QuaternionJson(Quaternion original)
        {
            X = original.x;
            Y = original.y;
            Z = original.z;
            W = original.w;
        }

        public Quaternion Revert ()
        {
            return new Quaternion(X, Y, Z, W);
        }
    }
}
