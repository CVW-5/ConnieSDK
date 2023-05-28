using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace ConnieSDK
{
    public class JsonMesh
    {
        public string Name;

        public Vector3[] Vertices;
        public int[] Tris;
        public Vector3[] Normals;
        public Vector2[] UV0;
        public Vector2[] UV1;
        public Vector2[] UV2;

        public JsonMesh(string Name, Mesh original)
        {
            this.Name = Name;
            Vertices = original.vertices;
            Tris = original.triangles;
            Normals = original.normals;
            UV0 = original.uv;
            UV1 = original.uv2;
            UV2 = original.uv3;
        }

        [System.Text.Json.Serialization.JsonConstructor]
        public JsonMesh(string Name, Vector3[] Vertices, int[] Tris, Vector3[] Normals, Vector2[] UV0, Vector2[] UV1, Vector2[] UV2)
        {
            this.Name = Name;
            this.Vertices = Vertices;
            this.Tris = Tris;
            this.Normals = Normals;
            this.UV0 = UV0;
            this.UV1 = UV1;
            this.UV2 = UV2;
        }

        public Mesh ToUnityMesh ()
        {
            Mesh newMesh = new Mesh();

            newMesh.SetVertices(Vertices);
            newMesh.SetTriangles(Tris, 0);
            newMesh.SetNormals(Normals);
            newMesh.SetUVs(0, UV0);
            newMesh.SetUVs(1, UV1);
            newMesh.SetUVs(2, UV2);

            newMesh.UploadMeshData(true);

            return newMesh;
        }
    }
}
