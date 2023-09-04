using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ConnieSDK
{
    public static class ObjSerializer
    {
        public static void Serialize (Mesh mesh)
        {
            Serialize(mesh, ObjSerializerSettings.Defaults);
        }

        public static void Serialize (Mesh mesh, ObjSerializerSettings settings)
        {

        }

        public static Mesh Deserialize (FileStream stream)
        {
            return Deserialize(stream, ObjSerializerSettings.Defaults);
        }

        public static Mesh Deserialize (FileStream stream, ObjSerializerSettings settings)
        {
            return new Mesh();
        }
    }

    public class ObjSerializerSettings
    {
        public static ObjSerializerSettings Defaults => new();
    }
}
