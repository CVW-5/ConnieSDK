using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConnieSDK;

#nullable enable
namespace CVWTerrain
{
    public class GravitySource : MonoBehaviour
    {
        public static GravitySource? Singleton;

        [field: SerializeField]
        public float Strength { get; private set; } = 9.81f;

        private void Start()
        {
            if(Singleton != null)
            {
                ConnieSDK.Logger.Write("Multiple GravitySources are present in the scene, ensure only one exists at a time.", LogType.Warning, true);
                enabled = false;
            }
            else Singleton = this;
        }
    }
}
