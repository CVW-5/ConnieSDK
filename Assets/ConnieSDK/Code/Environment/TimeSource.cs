using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConnieSDK
{
    public abstract class TimeSource : MonoBehaviour
    {
        public delegate void UpdateTimeDelegate(float time);

        public UpdateTimeDelegate TimeUpdates;

        protected readonly int SecondsPerDay = 60 * 60 * 24;
    }
}
