using System;
using System.Collections.Generic;
using System.IO;
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
#if UNITY_EDITOR
            Path.Join(Environment.CurrentDirectory, "Output");
#elif UNITY_STANDALONE
            Path.Join(Environment.CurrentDirectory, "Assets");
#endif
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
        public static bool SerializeObject(UnitData unit)
        {
            string fullpath = Path.Join(AssetDirectory, $"{unit.FileName}.{FileTypes[unit.Type]}");
            MeshCollection mc = MeshLibrary.GetCollection(unit.MeshCollection) ?? MeshLibrary.AddCollection(unit.MeshCollection);

            TransformWrapper hierarchy = new TransformWrapper(unit.transform, unit.MeshCollection, 5, true, true);

            using ArchiveWrapper archive = new ArchiveWrapper(fullpath, true);

            if (archive.HasEntry("EmptyAsset"))
                archive.DeleteEntry("EmptyAsset");

            DateTime generationTime = DateTime.UtcNow;

            string unitData = JsonSerializer.Serialize(unit, jsonOptions);
            string hierarchyJson = JsonSerializer.Serialize(hierarchy, jsonOptions);

            archive.WriteEntry("UnitData.json", unitData);
            archive.WriteEntry("Hierarchy.json", hierarchyJson);

            return true;
        }

        public static bool SerializeObject(GameObject prefab, ObjectType type, string outputName, string meshCollection)
        {
            string fullpath = Path.Join(AssetDirectory, $"{outputName}.{FileTypes[type]}");
            Debug.Log($"Serializing a prefab to {fullpath}...");

            TransformWrapper hierarchy = new TransformWrapper(prefab.transform, meshCollection, 5, true, true);
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
                Debug.Log("Wrote Hierarchy...");

                Debug.Log($"Archive now includes {archive.EntryNames.Length} entries");
            }

            Debug.Log("Done!");
            return true;
        }
#endif

        // Made an oopsie in the last commit.
        public static Transform? DeserializeObject(string filepath, bool hideByDefault = true)
        {
            using ArchiveWrapper archive = new ArchiveWrapper(filepath, false);

            TransformWrapper? hierarchy = null;

            if(archive.ReadEntry("Hierarchy.json", out string json))
            {
                hierarchy = JsonSerializer.Deserialize<TransformWrapper>(json, jsonOptions);
            }

            Transform? transform = hierarchy?.GenerateGameobjects();

            if (hideByDefault)
                transform?.gameObject.SetActive(false);

            return transform;
        }
    }
}
