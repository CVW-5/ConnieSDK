using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConnieSDK.Meshes
{
    public class MeshCollection
    {
        private Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

        public MeshCollection() { }

        public MeshCollection(string json)
        {
            List<JsonMesh> jsonMeshes = JsonConvert.DeserializeObject<List<JsonMesh>>(json, ConnieSerializer.jsonOptions) ?? new List<JsonMesh>();

            foreach (JsonMesh jm in jsonMeshes)
                meshes.Add(jm.Name, jm.ToUnityMesh());
        }

        public Mesh this[string name]
        {
            get => meshes[name];
            set => meshes[name] = value;
        }

        public string ToJson ()
        {
            List<JsonMesh> jsonMeshes = new List<JsonMesh>();

            foreach (KeyValuePair<string, Mesh> kvp in meshes)
                jsonMeshes.Add(new JsonMesh(kvp.Key, kvp.Value));

            return JsonConvert.SerializeObject(jsonMeshes, ConnieSerializer.jsonOptions);
        }
    }
}
