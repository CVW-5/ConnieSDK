using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#nullable enable
namespace ConnieSDK
{
    public static class MeshLibrary
    {
        private static Dictionary<string, MeshCollection> collections = new Dictionary<string, MeshCollection>();

        public static MeshCollection AddCollection (string name)
        {
            if (collections.ContainsKey(name))
                throw new Exception($"The mesh collection {name} already exists!");

            MeshCollection mc = new MeshCollection();
            collections[name] = mc;

            return mc;
        }

        public static MeshCollection? GetCollection (string name)
        {
            try
            {
                return collections[name];
            }
            catch
            {
                return null;
            }
        }

        public static Mesh GetMesh (string collection, string name)
        {
            if (collections[collection] is MeshCollection mc)
                if (mc[name] is Mesh mesh)
                    return mesh;

            throw new IndexOutOfRangeException();
        }

        public static void SetMesh (string collection, string name, Mesh mesh)
        {
            if (collections[collection] is MeshCollection mc)
                mc[name] = mesh;
        }
    }
}
