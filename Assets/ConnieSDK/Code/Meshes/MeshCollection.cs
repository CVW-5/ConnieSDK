using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConnieSDK.Meshes
{
    public class MeshCollection
    {
        private Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

        public Mesh this[string name]
        {
            get => meshes[name];
            set => meshes[name] = value;
        }
    }
}
