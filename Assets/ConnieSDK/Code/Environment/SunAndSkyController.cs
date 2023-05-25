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

        [Space(), Header("Gradients")]
        [SerializeField]
        SunSkyColorData? ColorData;


        [Space(), Header("Map Details")]
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

            if(ColorData is null)
            {
                SunSkyColors? _colorPalette = (SunSkyColors)Resources.Load("DefaultSky");

                ColorData = _colorPalette.Colors ?? throw new KeyNotFoundException("Unable to locate DefaultSky resource!");
            }

            TimeSource.TimeUpdates += UpdateTOD;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        }

        public void UpdateTOD(float newTOD)
        {
            TimeOfDay = newTOD;

            MoveSun();
            UpdateColors();
        }

        private void Update()
        {
            if (!RunOnUpdate) return;

            MoveSun();
            UpdateColors();
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

        private void UpdateColors ()
        {
            if (SkyMaterial is null || ColorData is null) return;
            if (!SkyMaterial.HasProperty("HorizonCol") || !SkyMaterial.HasProperty("ZenithCol"))
            {
                Debug.LogWarning("Material doesn't have a field \"HorizonCol\" or \"ZenithCol\"! These are required for proper behavior");
                return;
            }

            float sunheight = TimeOfDay < 0.5f ? TimeOfDay * 2 : -2* TimeOfDay + 2;

            Color horizon = ColorData.Horizon.Evaluate(sunheight);
            Color zenith = ColorData.Zenith.Evaluate(sunheight);

            SkyMaterial.SetColor("HorizonCol", horizon);
            SkyMaterial.SetColor("ZenithCol", zenith);

            if (Sun is null) return;

            Sun.color = ColorData.Sun.Evaluate(sunheight);

            RenderSettings.ambientEquatorColor = horizon;
            RenderSettings.ambientGroundColor = horizon;
            RenderSettings.ambientSkyColor = zenith;
        }
    }
}
