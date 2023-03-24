using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;
using ConnieSDK.Components;

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
        public static readonly Dictionary<ObjectType, string> FileTypes = new Dictionary<ObjectType, string>
        {
            {ObjectType.Generic,"zip" },
            {ObjectType.Unit, "cwu" },
            {ObjectType.Store,"cwu" },
            {ObjectType.MapData,"cwmap" },
            {ObjectType.Mission,"cwmis" },
        };

        public static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            IncludeFields = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
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

            TransformWrapper hierarchy = new TransformWrapper(prefab.transform, 5, true, true);
            Debug.Log($"Hierarchy crawled, found {hierarchy.Components.Length} valid components and {hierarchy.Children.Length} direct children");
            
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

                string hierarchyJson = JsonSerializer.Serialize(hierarchy, jsonOptions);

                archive.WriteEntry("Hierarchy.json", hierarchyJson);
                /*
                archive.WriteEntry("Hierarchy.txt",
                    $"WARNING: THIS FILE FORMAT IS TEMPORARY, DO NOT USE IT FOR LONG TERM DATA STORAGE\n\n" +
                    $"ConnieSDK Unit Definition: Hierarchy\n" +
                    $"{prefab.name}\n - " +
                    string.Join("\n - ", hierarchy.Children.Select(x => x.Name)) + "\n" +
                    $"Generated at {generationTime:yyyymmdd HH:mm:ss} UTC\n" +
                    $"Using ConnieSDK version {Globals.Version}\n\n" +
                    $"WARNING: THIS FILE FORMAT IS TEMPORARY, DO NOT USE IT FOR LONG TERM DATA STORAGE");
                */
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
