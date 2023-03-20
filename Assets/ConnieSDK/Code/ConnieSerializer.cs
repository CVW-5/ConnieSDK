using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#nullable enable
namespace ConnieSDK
{
    public enum ObjectType
    {
        Generic = 0,
        Unit = 1,
        Store = 2,
        MapData = 4,
        Mission = 8,
    }

    public static class ConnieSerializer
    {
        public static string AssetDirectory { get; private set; } =
            Path.Join(Environment.CurrentDirectory, "Assets");
        public static readonly Dictionary<ObjectType, string> FileTypes =
            new Dictionary<ObjectType, string>
            {
                {ObjectType.Generic,"zip" },
                {ObjectType.Unit, "cwu" },
                {ObjectType.Store,"cwu" },
                {ObjectType.MapData,"cwmap" },
                {ObjectType.Mission,"cwmis" },
            };

        public static void SetAssetDirectory(string path)
        {
            Debug.Log($"Switched target directory to {path}\n" +
                $"Was {AssetDirectory}");

            AssetDirectory = path;
        }

#if UNITY_EDITOR
        public static bool SerializeObject(GameObject prefab, ObjectType type, string outputName)
        {
            string fullpath = Path.Join(AssetDirectory, $"{outputName}.{FileTypes[type]}");
            Debug.Log($"Serializing a prefab to {fullpath}...");

            using (ArchiveWrapper archive = new ArchiveWrapper(fullpath, true))
            {

                if (archive.HasEntry("EmptyAsset"))
                    archive.DeleteEntry("EmptyAsset");

                DateTime generationTime = DateTime.UtcNow;

                archive.WriteEntry("UnitData.txt",
                    $"WARNING: THIS FILE FORMAT IS TEMPORARY, DO NOT USE IT FOR LONG TERM DATA STORAGE\n\n" +
                    $"ConnieSDK Unit Definition: General\n" +
                    $"Name: {outputName}\n" +
                    $"Type: {type}\n\n" +
                    $"Generated at {generationTime:yyyymmdd HH:mm:ss} UTC\n" +
                    $"Using ConnieSDK version {Globals.Version}\n\n" +
                    $"WARNING: THIS FILE FORMAT IS TEMPORARY, DO NOT USE IT FOR LONG TERM DATA STORAGE");
                Debug.Log("Wrote Unit Data...");

                archive.WriteEntry("Hierarchy.txt",
                    $"WARNING: THIS FILE FORMAT IS TEMPORARY, DO NOT USE IT FOR LONG TERM DATA STORAGE\n\n" +
                    $"ConnieSDK Unit Definition: Hierarchy\n" +
                    $"{prefab.name}\n - " +
                    string.Join("\n - ", prefab.transform.GetChildren().Select(x => x.name)) + "\n" +
                    $"Generated at {generationTime:yyyymmdd HH:mm:ss} UTC\n" +
                    $"Using ConnieSDK version {Globals.Version}\n\n" +
                    $"WARNING: THIS FILE FORMAT IS TEMPORARY, DO NOT USE IT FOR LONG TERM DATA STORAGE");
                Debug.Log("Wrote Hierarchy...");

                Debug.Log($"Archive now includes {archive.EntryNames.Length} entries");
            }

            Debug.Log("Done!");
            return true;
        }
#endif

        public static GameObject? DeserializeObject(string filepath, bool hideByDefault = true)
        {
            return null;
        }
    }
}
