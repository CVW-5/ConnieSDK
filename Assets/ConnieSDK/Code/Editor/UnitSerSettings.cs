using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConnieSDK.Editor
{
    public class UnitSerSettings : ScriptableObject
    {
        public string OutputDir;

        public Material[] CommonMaterials;
        public Mesh[] CommonMeshes;
    }
}
