using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConnieSDK
{
    [CreateAssetMenu(menuName ="CVW5/Sun Sky Color Data")]
    public class SunSkyColors : ScriptableObject
    {
        public SunSkyColorData Colors;
    }

    [System.Serializable]
    public class SunSkyColorData
    {
        public Gradient Horizon, Zenith, Sun, Clouds;
    }
}
