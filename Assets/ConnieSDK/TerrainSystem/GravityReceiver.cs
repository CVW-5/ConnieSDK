using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using loki_geo;

#nullable enable
namespace CVWTerrain
{
    public class GravityReceiver : MonoBehaviour
    {
        private float GravityStrength => GravitySource.Singleton?.Strength ?? 9.81f;

        public Vector64 ComputeGravity(Vector64 worldPosition)
        {
            Vector64 upDir = worldPosition.normalized;

            return GravityStrength * -upDir;
        }

        public Vector64 GetNorthDir(Vector64 worldPosition) => TangentMatrix.GetNorthVector(worldPosition.normalized);

        public Quaternion ComputeRotation(Vector64 worldPosition)
        {
            Vector64 upDir = worldPosition.normalized;
            Vector64 NorthDir = GetNorthDir(worldPosition);

            var up3 = upDir.ToVector3();
            var nr3 = NorthDir.ToVector3();

            Quaternion rotation = Quaternion.LookRotation(nr3, up3);
            return rotation;
        }
    }
}
