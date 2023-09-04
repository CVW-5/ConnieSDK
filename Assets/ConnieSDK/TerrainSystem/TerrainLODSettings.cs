using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CVWTerrain
{
    [System.Serializable]
    public class TerrainLODSettings
    {
        public List<TerrainLOD> LODs = new List<TerrainLOD>(1);
        //[Min(1)]
        public int MinimumResolution = 0;

        public float HideMeshDistance = 100000;

        public void SortLODs ()
        {
            var sortLODQuery = from TLOD in LODs
                               orderby TLOD.MaxRange
                               select TLOD;

            LODs = sortLODQuery.ToList();
        }

        public int GetResolutionAtRange (float Range)
        {
            var nextlargestLOD = LODs.FirstOrDefault(x => x.MaxRange >= Range);

            //Debug.Log($"nextlargestLOD object for range {Range} is {nextlargestLOD}");

            if (nextlargestLOD == null) return MinimumResolution;

            //Debug.Log($"nextlargestLOD resolution is {nextlargestLOD.Resolution}");
            return Mathf.Max(nextlargestLOD.Resolution, MinimumResolution);
        }
    }

    [System.Serializable]
    public class TerrainLOD
    {
        public float MaxRange = 1500;
        public float SqrRange => MaxRange * MaxRange;

        public int Resolution = 16;

        public TerrainLOD (float MaxRange, int Resolution)
        {
            this.MaxRange = MaxRange;
            this.Resolution = Resolution;
        }
    }
}
