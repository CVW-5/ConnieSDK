using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace CVWTerrain
{
    /// <summary>
    /// Used to interpret and store altitude data from a heightmap texture. Usually created by a TerrainBuilder component.
    /// </summary>
    [System.Serializable]
    public class Heightmap
    {
        /// <summary>
        /// The pixel width of the heightmap texture.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// The pixel height of the heightmap texture.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The Southwestern-most (or lower left) corner of the heightmap texture - X is longitude, Y is latitude.
        /// </summary>
        public Vector2 SouthWest { get; private set; }

        /// <summary>
        /// The Northeastern-most (or upper right) corner of the heightmap texture - X is longitude, Y is latitude.
        /// </summary>
        public Vector2 NorthEast { get; private set; }

        /// <summary>
        /// The North-South size, in degrees decimal, of the heightmap texture. Derived from SouthWest and NorthEast fields.
        /// </summary>
        public float LatSize => NorthEast.y - SouthWest.y;
        /// <summary>
        /// The East-West size, in degrees decimal, of the heightmap texture. Derived from SouthWest and NorthEast fields.
        /// </summary>
        public float LonSize => NorthEast.x - SouthWest.x;

        /// <summary>
        /// A two-dimensional rectangular array storing all of the raw altitude values in this heightmap, derived from the source texture.
        /// </summary>
        public float[,] Values { get; private set; }

        /// <summary>
        /// If a point is located at sea level, i.e. Value[x,y] == 0, the amount to adjust that altitude value in order to help it lie underneath any sea-surface mesh.
        /// </summary>
        public float SealevelAdjust = -5;

        private Heightmap() { throw new InvalidOperationException("A Heightmap object should never be created with the default constructor!"); }

        public Heightmap(Texture2D Texture, int ColorDivisions, Vector2 SW, Vector2 NE, float Min, float Max)
        {
            Width = Texture.width;
            Height = Texture.height;
            var Pixels = Texture.GetPixels();

            SouthWest = SW;
            NorthEast = NE;

            Debug.Log($"Heightmap goes from {SouthWest} to {NorthEast} (size: {LatSize} in Latitude, {LonSize} in Longitude).");

            Values = new float[Width, Height];

            string outputFile = "./Output/Terrain/Heightmap.csv";

            if (!File.Exists(outputFile))
                File.Create(outputFile).Close();

            using var stream = File.Open(outputFile, FileMode.Truncate);
            using var writer = new StreamWriter(stream);

            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++)
                {
                    int pixel = x + y * Width;

                    float t = ColorRampMath.InverseRamp(Pixels[pixel], ColorDivisions);

                    //Debug.Log($"Altitude at [{x},{y}] is {Mathf.Lerp(Min, Max, t)} from t={t} from color {Pixels[pixel]}");
                    float height = Mathf.Lerp(Min, Max, t);
                    Values[x, y] = height;

                    //writer.Write($"{height},");
                }
                writer.Write("\n");
                writer.Flush();
            }            
        }

        /// <summary>
        /// Gets the value at a specified latitude and longitude. Includes processing for Sea Level adjustment and options for averaging the altitude value if a point is not pixel-perfect relative to the source texture.
        /// </summary>
        /// <param name="Lat"></param>
        /// <param name="Lon"></param>
        /// <param name="Averaged"></param>
        /// <returns></returns>
        public float GetValue(float Lat, float Lon, bool Averaged = false)
        {
            float posX = (float)(Lon - SouthWest.x) / LonSize * Width;
            float posY = (float)(Lat - SouthWest.y) / LatSize * Height;

            //Debug.Log($"posX, posY: {posX}, {posY}");

            if (Averaged)
            {
                int minX, maxX, minY, maxY;

                float offsetX = posX % 1;
                if (offsetX > 0)
                {
                    minX = Mathf.FloorToInt(posX);
                    maxX = Mathf.CeilToInt(posX);
                }
                else
                {
                    minX = (int)posX;
                    maxX = (int)posX;
                }

                float offsetY = posY % 1;
                if (offsetY > 0)
                {
                    minY = Mathf.FloorToInt(posY);
                    maxY = Mathf.CeilToInt(posY);
                }
                else
                {
                    minY = (int)posY;
                    maxY = (int)posY;
                }

                float LL = GetRawValue(minX, minY);
                float LR = GetRawValue(maxX, minY);
                float UR = GetRawValue(maxX, maxY);
                float UL = GetRawValue(minX, maxY);

                float TopAvg = Mathf.Lerp(UL, UR, offsetX);
                float BtmAvg = Mathf.Lerp(LL, LR, offsetX);

                //Debug.Log($"output value: {Mathf.Lerp(BtmAvg, TopAvg, offsetY)}");
                float final = Mathf.Lerp(BtmAvg, TopAvg, offsetY);

                return final > 0 ? final : SealevelAdjust;
            }
            else
            {
                int idxX = Mathf.RoundToInt(posX);
                int idxY = Mathf.RoundToInt(posY);

                float value = GetRawValue(idxX, idxY);

                //Debug.Log($"output value: {value}");
                return value > 0 ? value : SealevelAdjust;
            }
        }

        /// <summary>
        /// Gets the raw value at pixel coordinate [x,y] within the array, without any processing.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private float GetRawValue (int x, int y)
        {
            try {
                float val = Values[x, y];

                return val;
            }
            catch (IndexOutOfRangeException ioor)
            {
                return 0;
            }
        }
    }
}
