using loki_geo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CVWTerrain
{
    /// <summary>
    /// A variant of TerrainSubmesh that exclusively produces flat (relative to the surface of earth) meshes for representing the ocean.
    /// </summary>
    public class OceanSubmesh : TerrainSubmesh
    {
        protected override string NamePrefix => "Ocean";
        protected override float GetAltitudeAtPoint(Heightmap map, LatLonCoord point) => 0;
    }
}
