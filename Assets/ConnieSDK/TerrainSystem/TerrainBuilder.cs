using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using loki_geo;
using System.Linq;

#nullable enable
namespace CVWTerrain
{
    public enum TextureFormat
    {
        Unknown,
        ASTER_DEM,
        Generic
    }

    public class TerrainBuilder : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private bool AlwaysDrawGizmos = false;
        [SerializeField, Tooltip("Divides up mesh generation into tasks to minimize freezing of the UI; Completed meshes will show up in green.")]
        private bool BuildAsync = true;
        public string OutputDirectory = "./Output/Terrain";

        [Header("Preview")]
        [Min(1)]
        public float PreviewRadius = (float)Globe.EarthRadius;
        public float PreviewScale => PreviewRadius / (float)Globe.EarthRadius;
        [Min(0)]
        public int PreviewIndex = 0;
        public bool ShowPreview = false;

        [Header("Source Heightmap")]
        public Texture2D SourceTexture;
        public TextureFormat Format = TextureFormat.ASTER_DEM;
        public static readonly System.Text.RegularExpressions.Regex AsterRegex = new System.Text.RegularExpressions.Regex(@"AST.*(?<lat>(N|S)\d{2})(?<lon>(E|W)\d{3})(_dem)?");
        [Tooltip("If Format = Generic, this is how many color steps were used to generate this heightmap. See the Documentation for details.")]
        public int HeightmapColorSteps = 3;
        public float MinAltitude = 0;
        public float MaxAltitude = 1500;
        public float SeaLevelAdjust = -5;

        [Header("Map Properties")]
        [Tooltip("The Southwest corner of this texture file. X is Longitude (-W/+E), Y is Latitude (-S/+N)")]
        public Vector2 SWCorner;
        [Tooltip("The Northeast corner of this texture file. X is Longitude (-W/+E), Y is Latitude (-S/+N)")]
        public Vector2 NECorner;
        public Vector2 Center => (SWCorner + NECorner) / 2;
        public double CenterAltitudeOffset = 0;
        public LatLonCoord CenterLL => new LatLonCoord { Lat_Degrees = Center.y, Lon_Degrees = Center.x, Alt = 0 };
        [Tooltip("How large the map area should be, in km. X is E-W direction, Y is N-S."), Min(0)]
        public Vector2 AreaSize = new Vector2(178, 150);

        [Header("Submesh Properties")]
        [Tooltip("How large, in km, each submesh should be on a side before conforming.")]
        public float KmPerSubmesh = 10;
        [Range(1,64)]
        public int SubmeshResolution = 100;
        public bool CreateColliders = true;
        public Material LandMaterial;
        public Material SeaMaterial;

        [Header("LODs")]
        public Transform ViewObject;
        [ContextMenuItem("Sort", nameof(SortLODs))]
        public TerrainLODSettings LODSettings = new TerrainLODSettings();
        private float LastBuild = 1;

        [field:SerializeField]
        public List<TerrainSubmesh> Submeshes { get; private set; } = new List<TerrainSubmesh>(0);
        private List<OceanSubmesh> OceanMeshes = new List<OceanSubmesh>(0);

        [SerializeField]
        private Heightmap Heightmap;

        private Transform LandParent;
        private Transform OceanParent;

        // Start is called before the first frame update
        void Start()
        {
            LODSettings.SortLODs();
            BuildMesh(Preview: true, RegenHeightmap: true);

            int count = Submeshes.Count;

            for(int c = 0; c < transform.childCount; c++)
            {
                Destroy(transform.GetChild(c).gameObject);
            }

            if(LandMaterial == null) LandMaterial = (Material)Resources.Load("TerrainMat", typeof(Material));
            if (SeaMaterial == null) SeaMaterial = (Material)Resources.Load("MISSING_MATERIAL", typeof(Material));

            GameObject lp = new("Land Meshes");
            GameObject op = new("Ocean Meshes");

            LandParent = lp.transform;
            LandParent.SetParent(transform);
            OceanParent = op.transform;
            OceanParent.SetParent(transform);

            for (int i = 0; i< count; i++)
            {
                Vector3 previewPos = (Submeshes[i].worldCenter * PreviewScale).ToVector3();

                GameObject submesh = new($"Land_{i}", typeof(MeshFilter), typeof(MeshRenderer));
                if (CreateColliders) submesh.AddComponent(typeof(MeshCollider));
                submesh.transform.SetParent(LandParent);
                submesh.transform.localPosition = previewPos;
                submesh.transform.localScale = PreviewScale * Vector3.one;
                submesh.GetComponent<MeshRenderer>().sharedMaterial = LandMaterial;

                GameObject oceanmesh = new($"Ocean_{i}", typeof(MeshFilter), typeof(MeshRenderer));
                oceanmesh.transform.SetParent(OceanParent);
                oceanmesh.transform.localPosition = previewPos;
                oceanmesh.transform.localScale = PreviewScale * Vector3.one;
                oceanmesh.GetComponent<MeshRenderer>().sharedMaterial = SeaMaterial;

            }
        }

        private void OnValidate()
        {
            if (!ShowPreview) return;

            var previewer = transform.GetChild(0).gameObject;
            if(previewer.GetComponent<MeshFilter>() == null)
            {
                previewer.AddComponent<MeshFilter>();
            }
            if (previewer.GetComponent<MeshRenderer>() == null)
            {
                previewer.AddComponent<MeshRenderer>();
            }

            if (Submeshes.Count < 1) return;
            if (PreviewIndex >= Submeshes.Count) PreviewIndex = Submeshes.Count - 1;

            Vector3 previewPos = (Submeshes[PreviewIndex].worldCenter * PreviewScale).ToVector3();

            previewer.transform.localPosition = previewPos;
            previewer.transform.localScale = PreviewScale * Vector3.one;
            previewer.GetComponent<MeshFilter>().sharedMesh = Submeshes[PreviewIndex].Mesh;

        }

        [ContextMenu("Preview")]
        public void PreviewMesh()
        {
            BuildMesh(Preview: true);
        }

        [ContextMenu("Build")]
        public void Build_Context ()
        {
            BuildMesh(DynamicResolution: false);
        }

        public void FixedUpdate()
        {
            if(LastBuild-- <= 0)
            {
                LastBuild = 50;
                RebuildMeshes();

                for(int i = 0; i<Submeshes.Count; i++)
                {
                    double dist = Vector64.Distance(ViewObject.position.ToWGS() / PreviewScale, Submeshes[i].worldCenter);
                    bool visible = dist <= LODSettings.HideMeshDistance;

                    var sm = LandParent.GetChild(i);
                    var smMF = sm.GetComponent<MeshFilter>();
                    var smMC = sm.GetComponent<MeshCollider>();

                    sm.gameObject.SetActive(visible);
                    smMF.sharedMesh = Submeshes[i].Mesh;
                    smMC.sharedMesh = Submeshes[i].Mesh;

                    var om = OceanParent.GetChild(i);
                    var omMF = om.GetComponent<MeshFilter>();

                    om.gameObject.SetActive(visible);
                    omMF.sharedMesh = OceanMeshes[i].Mesh;
                }
            }
        }

        /// <summary>
        /// Used for initial construction of meshes - builds the heightmap, creates the Submesh objects, and does any other initialization. Will create and initialize new TerrainSubmesh objects, so only use this for first-time builds i.e. during Start().
        /// </summary>
        /// <param name="Preview">Is this a preview operation? If true, will only create the Submesh objects and display their bounds.</param>
        /// <param name="DynamicResolution">Should the mesh resolution scale based on LODSettings?</param>
        /// <param name="RegenHeightmap">Should the heightmap be regenerated from scratch? This can be accomplished without a full submesh re-initialization by calling RegenerateHeightmap().</param>
        public void BuildMesh (bool Preview = false, bool DynamicResolution = false, bool RegenHeightmap = true)
        {
            List<float> LatStarts = new List<float>();
            List<float> LonStarts = new List<float>();

            //foreach (Texture2D tex in SourceTexture)
            //{
            float lat, lon;

            ExtractLatLon(SourceTexture.name, out lat, out lon);

            LatStarts.Add(lat);
            LonStarts.Add(lon);
            //}

            if(Format == TextureFormat.ASTER_DEM)
            {
                SWCorner = new Vector2(LonStarts.Min(), LatStarts.Min());
                NECorner = new Vector2(LonStarts.Max() + 1, LatStarts.Max() + 1) + Vector2.one;
            }

            int VertCount = Mathf.CeilToInt(AreaSize.y % KmPerSubmesh > 0 ? (AreaSize.y / KmPerSubmesh) + 1 : AreaSize.y / KmPerSubmesh);
            int HorzCount = Mathf.CeilToInt(AreaSize.x % KmPerSubmesh > 0 ? (AreaSize.x / KmPerSubmesh) + (AreaSize.y % KmPerSubmesh > 0 ? 0 : 1) : AreaSize.x / KmPerSubmesh);

            Submeshes = new(new TerrainSubmesh[VertCount * HorzCount]);
            OceanMeshes = new(new OceanSubmesh[VertCount * HorzCount]);
            Debug.Log($"Estimating {VertCount * HorzCount} submeshes...");

            TangentMatrix CenterMatrix = TangentMatrix.FromLatLon(CenterLL, Globe.EarthRadius);
            CenterMatrix.SetOrigin((0, 0, 0));

            float horzOffset = -AreaSize.x / 2;
            float vertOffset = -AreaSize.y / 2;

            // Generate Heightmap
            if (RegenHeightmap) RegenerateHeightmap();
            else Debug.Log("RegenHeightmap set to false, skipping.");

            for (int i = 0; i < Submeshes.Count; i++)
            {
                Submeshes[i] = new();
                OceanMeshes[i] = new();
                TerrainSubmesh t = Submeshes[i];
                OceanSubmesh o = OceanMeshes[i];

                Vector64 offset = new Vector64(Globe.EarthRadius + CenterAltitudeOffset, horzOffset * 1000, vertOffset * 1000);
                Vector64 size = new Vector64(0, KmPerSubmesh * 1000, KmPerSubmesh * 1000);

                t.Init(offset, size, CenterMatrix);
                o.Init(offset, size, CenterMatrix);

                horzOffset += KmPerSubmesh;
                if(horzOffset >= (AreaSize.x/2))
                {
                    horzOffset = -AreaSize.x / 2;
                    vertOffset += KmPerSubmesh;
                }

                // Actual construction of the mesh. If we're only previewing, skip mesh construction.
                if (Preview) continue;

                int currentRez = SubmeshResolution;

                if(DynamicResolution)
                {
                    double dist = Vector64.Distance(ViewObject.position.ToWGS() / PreviewScale, t.worldCenter);

                    currentRez = LODSettings.GetResolutionAtRange((float)dist);
                }

                t.Build(Heightmap, currentRez);
                o.Build(Heightmap, currentRez);
            }
        }

        /// <summary>
        /// Rebuilds all meshes without creating new TerrainSubmesh objects or regenerating the heightmap. Use this for frame-by-frame rebuilding as the camera/player moves around.
        /// </summary>
        /// <param name="DynamicResolution">Should the mesh resolution scale based on LODSettings?</param>
        public void RebuildMeshes(bool DynamicResolution = true)
        {
            for (int i = 0; i < Submeshes.Count; i++)
            {
                TerrainSubmesh sm = Submeshes[i];
                OceanSubmesh om = OceanMeshes[i];
                int currentRez = SubmeshResolution;

                if (DynamicResolution)
                {
                    double dist = Vector64.Distance(ViewObject.position.ToWGS() / PreviewScale, sm.worldCenter);

                    currentRez = LODSettings.GetResolutionAtRange((float)dist);
                }

                sm.Build(Heightmap, currentRez);
                om.Build(Heightmap, currentRez);
            }
        }

        [ContextMenu("Regenerate Heightmap")]
        public void RegenerateHeightmap ()
        {
            Debug.Log("Generating heightmap...");
            Heightmap = new Heightmap(SourceTexture, HeightmapColorSteps, SWCorner, NECorner, MinAltitude, MaxAltitude);
            Heightmap.SealevelAdjust = SeaLevelAdjust;
            Debug.Log("Done.");
        }

        private bool ExtractLatLon(string filename, out float lat, out float lon)
        {
            lat = 0;
            lon = 0;

            if (!AsterRegex.IsMatch(filename)) return false;

            var match = AsterRegex.Match(filename);
            var latstr = match.Groups["lat"].Value;
            var lonstr = match.Groups["lon"].Value;

            if (!float.TryParse(latstr.Substring(1, 2), out lat)) return false;
            if (latstr.StartsWith("S")) lat *= -1;

            if (!float.TryParse(lonstr.Substring(1, 3), out lon)) return false;
            if (lonstr.StartsWith("W")) lon *= -1;

            return true;
        }

        //[ContextMenu("Check Parameters")]
        public void CheckParams()
        {
            string fullpath = Path.GetFullPath(OutputDirectory);
            ValidateOutputDir(fullpath);

            var size = EstimateMapSize();

            string eastsize = $"{size.EastEdge:f0}";
            string westsize = $"{size.WestEdge:f0}";
            string padding = " ".PadLeft(westsize.Length + 1, ' ');

            string line1 = $"{padding}     {size.NorthEdge:f0}m";
            string line2 = $"{padding} NW ------- NE ";
            string linec = $"{padding}  |         |  ";
            string linem = $" {westsize}m |         | {eastsize}m";
            string line6 = $"{padding} SW ------- SE ";
            string line7 = $"{padding}     {size.SouthEdge:f0}m";

            int pointestimate = (int)((size.WestEdge / 1000) * (Mathf.Max(size.NorthEdge, size.SouthEdge) / 1000)) * 4;
            bool estimateacceptable = pointestimate <= 65536;

            string message = string.Join("\n", "TerrainBuilder parameter checkout:", line1, line2, linec, linem, linec, line6, line7, $"Point estimate: {pointestimate}, {(estimateacceptable ? "OK" : "TOO MANY POINTS!")}");

            if (estimateacceptable) Debug.Log(message);
            else Debug.LogWarning(message);
        }

        private bool ValidateOutputDir(string fullpath)
        {
            Debug.Log($"Writing to {fullpath}");

            try
            {
                DirectoryInfo outputdir = new DirectoryInfo(fullpath);

                if (!outputdir.Exists)
                {
                    outputdir.Create();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public (float NorthEdge, float SouthEdge, float EastEdge, float WestEdge) EstimateMapSize()
        {
            LatLonCoord NW = new LatLonCoord { Lat_Degrees = NECorner.y, Lon_Degrees = SWCorner.x };
            LatLonCoord NE = new LatLonCoord { Lat_Degrees = NECorner.y, Lon_Degrees = NECorner.x };
            LatLonCoord SW = new LatLonCoord { Lat_Degrees = SWCorner.y, Lon_Degrees = SWCorner.x };
            LatLonCoord SE = new LatLonCoord { Lat_Degrees = SWCorner.y, Lon_Degrees = NECorner.x };

            float NorthEdge = (float)Globe.GetGreatCircleDistance(NW, NE);
            float SouthEdge = (float)Globe.GetGreatCircleDistance(SW, SE);
            float EastEdge = (float)Globe.GetGreatCircleDistance(SE, NE);
            float WestEdge = (float)Globe.GetGreatCircleDistance(SW, NW);

            return (NorthEdge, SouthEdge, EastEdge, WestEdge);
        }

        /// <summary>
        /// Sorts the Level-of-Detail steps in LODSettings by their maximum range.
        /// </summary>
        private void SortLODs()
        {
            LODSettings.SortLODs();
        }

        public void OnDrawGizmos()
        {
            if(AlwaysDrawGizmos) DrawGizmosControlled();
        }

        private void OnDrawGizmosSelected()
        {
            if (!AlwaysDrawGizmos) DrawGizmosControlled();
        }

        private void DrawGizmosControlled ()
        {
            TangentMatrix ViewMatrix = TangentMatrix.FromLatLon(CenterLL, PreviewRadius);
            ViewMatrix.SetScale(PreviewScale);
            ViewMatrix.SetOrigin(Vector64.zero);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, PreviewRadius);
            DrawDEMArea();

            foreach (TerrainSubmesh sub in Submeshes)
            {
                sub.PreviewMatrix = ViewMatrix;
                sub.OnDrawGizmosSelected();
            }

            DrawMeshArea(ViewMatrix);
            DrawLODs();
        }

        private void DrawDEMArea ()
        {
            var SWLL = new LatLonCoord { Lat_Degrees = SWCorner.y, Lon_Degrees = SWCorner.x, Alt = 0 };
            var SELL = new LatLonCoord { Lat_Degrees = SWCorner.y, Lon_Degrees = NECorner.x, Alt = 0 };
            var NELL = new LatLonCoord { Lat_Degrees = NECorner.y, Lon_Degrees = NECorner.x, Alt = 0 };
            var NWLL = new LatLonCoord { Lat_Degrees = NECorner.y, Lon_Degrees = SWCorner.x, Alt = 0 };

            Vector3 SW3 = transform.TransformPoint(Globe.LLToXYZ(SWLL, PreviewRadius).ToVector3());
            Vector3 SE3 = transform.TransformPoint(Globe.LLToXYZ(SELL, PreviewRadius).ToVector3());
            Vector3 NE3 = transform.TransformPoint(Globe.LLToXYZ(NELL, PreviewRadius).ToVector3());
            Vector3 NW3 = transform.TransformPoint(Globe.LLToXYZ(NWLL, PreviewRadius).ToVector3());

            Gizmos.color = Color.yellow;
            //Debug.Log($"{SW3} - {SE3} - {NE3} - {NW3}");

            //Gizmos.DrawSphere(SW3, PreviewRadius / 100);
            //Gizmos.DrawSphere(SE3, PreviewRadius / 100);
            //Gizmos.DrawSphere(NE3, PreviewRadius / 100);
            //Gizmos.DrawSphere(NW3, PreviewRadius / 100);

            Gizmos.DrawLine(SW3, SE3);
            Gizmos.DrawLine(SE3, NE3);
            Gizmos.DrawLine(NE3, NW3);
            Gizmos.DrawLine(NW3, SW3);
        }

        private void DrawMeshArea(TangentMatrix ViewMatrix)
        {
            var radius = Globe.EarthRadius + CenterAltitudeOffset;
            Vector3 center = ViewMatrix.PointToWorldSpace((radius, 0, 0)).ToVector3();
            Vector3 north = ViewMatrix.PointToWorldSpace((radius, 0, -1 / PreviewScale)).ToVector3();
            Vector3 right = ViewMatrix.PointToWorldSpace((radius, 1 / PreviewScale, 0)).ToVector3();
            Vector3 up = ViewMatrix.PointToWorldSpace((radius + (1 / PreviewScale), 0, 0)).ToVector3();

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center, north);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, up);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, right);

            float horzEdge = -AreaSize.x * 1000 / 2;
            float vertEdge = AreaSize.y * 1000 / 2;

            Vector3 LL3 = ViewMatrix.PointToWorldSpace((radius, -horzEdge, -vertEdge)).ToVector3();
            Vector3 LR3 = ViewMatrix.PointToWorldSpace((radius, -horzEdge, vertEdge)).ToVector3();
            Vector3 UR3 = ViewMatrix.PointToWorldSpace((radius, horzEdge, vertEdge)).ToVector3();
            Vector3 UL3 = ViewMatrix.PointToWorldSpace((radius, horzEdge, -vertEdge)).ToVector3();

            Gizmos.color = Color.black;
            Gizmos.DrawLine(LL3, LR3);
            Gizmos.DrawLine(LR3, UR3);
            Gizmos.DrawLine(UR3, UL3);
            Gizmos.DrawLine(UL3, LL3);
        }

        private void DrawLODs()
        {
            if (ViewObject != null)
            {
                Gizmos.color = Color.gray;
                foreach (var lod in LODSettings.LODs)
                {
                    Gizmos.DrawWireSphere(ViewObject.position, lod.MaxRange * PreviewScale);
                }

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(ViewObject.position, LODSettings.HideMeshDistance * PreviewScale);
            }
        }
    }
}