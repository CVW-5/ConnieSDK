using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using loki_geo;

namespace CVWTerrain
{
    public class SurfaceDebugger : GravityReceiver
    {
        public float GravityStrength = 9.81f;
        public bool ApplyRotation = false;

        private void OnDrawGizmos()
        {
            Vector64 WGSpos = transform.position.ToWGS();

            Vector3 gravity = ComputeGravity(WGSpos).ToVector3();

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, gravity);

            Vector3 north = GetNorthDir(WGSpos).ToVector3();

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, north);

            if(ApplyRotation)
            {
                transform.rotation = ComputeRotation(WGSpos);
            }
        }
    }
}
