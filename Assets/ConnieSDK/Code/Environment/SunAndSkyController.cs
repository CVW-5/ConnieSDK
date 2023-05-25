using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace ConnieSDK
{
    public class SunAndSkyController : MonoBehaviour
    {
        public static SunAndSkyController? Singleton;

        [SerializeField]
        private Material? SkyMaterial;
        [SerializeField]
        private Light? Sun;

        [Range(-180, 180), SerializeField]
        private float NorthOffset = 0;
        [Range(-90,90), SerializeField]
        private float Latitude = 0;

        [Range(0,1), SerializeField]
        private float TimeOfDay = 0.5f;

        [SerializeField]
        private TimeSource? TimeSource;
        private bool RunOnUpdate = false;

        // Start is called before the first frame update
        void Start()
        {
            if(Singleton is not null)
            {
                Debug.LogWarning("Multiple SunAndSkyControllers exist! Deactivating duplicates; please ensure only one exists in the scene!", this);
                enabled = false;
                return;
            }

            Singleton = this;

            if(TimeSource is null)
            {
                RunOnUpdate = true;
                return;
            }

            TimeSource.TimeUpdates += UpdateTOD;
        }

        public void UpdateTOD(float newTOD)
        {
            TimeOfDay = newTOD;

            MoveSun();
            UpdateSky();
        }

        private void Update()
        {
            if (!RunOnUpdate) return;

            MoveSun();
            UpdateSky();
        }

        private void MoveSun ()
        {
            if (Sun is null) return;

            Quaternion rotation = ComputeSunAngle();
            Sun.transform.rotation = rotation;
        }

        private Quaternion ComputeSunAngle ()
        {

            Quaternion baseAngle = Quaternion.Euler(0, NorthOffset - 90, Latitude);
            float progressionAngle = 180f * (TimeOfDay * 2 - 0.5f);

            return baseAngle * Quaternion.Euler(progressionAngle, 0, 0);
        }

        private void UpdateSky ()
        {
            if (SkyMaterial is null) return;
            if (!SkyMaterial.HasProperty("SunHeight"))
            {
                Debug.LogWarning("Material doesn't have a field \"SunHeight\"");
                return;
            }

            float sunheight = TimeOfDay < 0.5f ? TimeOfDay * 2 : -2* TimeOfDay + 2;

            SkyMaterial.SetFloat("SunHeight", sunheight);
        }
    }
}
