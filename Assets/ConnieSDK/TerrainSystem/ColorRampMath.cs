using System;
using System.Collections.Generic;
using UnityEngine;

namespace CVWTerrain
{
    public static class ColorRampMath
    {
        public static float InverseRamp (Color color, int steps)
        {
            return steps switch
            {
                3 => InverseThreeStepRamp(color),
                4 => InverseFourStepRamp(color),
                _ => throw new NotImplementedException($"Inverse color ramp functionality for a ramp with {steps} is not implemented")
            };
        }

        private static float InverseThreeStepRamp (Color color)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;

            float sum = r + g + b;

            float s0 = 0.0f;
            float s1 = 0.33333f;
            float s2 = 0.66667f;
            float s3 = 1.0f;

            return sum / 3;
        }

        private static float InverseFourStepRamp (Color color)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;

            float s0 = 0.0f;
            float s1 = (float)1 / 4;
            float s2 = (float)2 / 4;
            float s3 = (float)3 / 4;
            float s4 = 1.0f;

            if(r == 0)
            {
                return g switch
                {
                    0 => inverseT(b, s3, s4, 4f, false), // Decreasing B component (0.75 to 1.0)
                    > 0 => inverseT(b, s2, s3, 4f, true), // Increasing B component (0.5 to 0.75)
                    _ => throw new InvalidOperationException($"Color value for a ramp is invalid! Value: {g}")
                };
            }
            if (b == 0)
            {
                return g switch
                {
                    0 => inverseT(r, s0, s1, 4f, true), // Increasing R component (0.0 to 0.25)
                    > 0 => inverseT(r, s1, s2, 4f, false), // Decreasing R component (0.25 to 0.5)
                    _ => throw new InvalidOperationException($"Color value for a ramp is invalid! Value: {g}")
                };
            }

            throw new InvalidOperationException($"Color value for a ramp is invalid! Value: {color}");
        }

        private static float inverseT (float c, float t0, float t1, float n, bool ascending)
        {
            if(ascending)
            {
                float frac = (float)(c + (n * t0)) / (float)n;

                return frac + t0;
            }
            else
            {
                float frac = (float)(c - (n * t1)) / (float)n;

                return t0 - frac;
            }
        }
    }
}
