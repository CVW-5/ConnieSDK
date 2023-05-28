using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;
using ConnieSDK.Components;
using ConnieSDK.Meshes;

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
            {ObjectType.Unit, "zip" },
            {ObjectType.Store,"zip" },
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
        public static bool SerializeObject(UnitDefinition unit, Transform transform)
        {
            string fullpath = Path.Join(AssetDirectory, $"{unit.FileName}.{FileTypes[unit.Type]}");
            MeshCollection mc = MeshLibrary.GetCollection(unit.MeshCollection) ?? MeshLibrary.AddCollection(unit.MeshCollection);

            TransformWrapper hierarchy = new TransformWrapper(transform, 5, true, true, mc);

            using ArchiveWrapper archive = new ArchiveWrapper(fullpath, true);

            if (archive.HasEntry("EmptyAsset"))
                archive.DeleteEntry("EmptyAsset");

            DateTime generationTime = DateTime.UtcNow;

            string unitData = JsonSerializer.Serialize(unit, jsonOptions);
            string hierarchyJson = JsonSerializer.Serialize(hierarchy, jsonOptions);
            string meshData = mc.ToJson();

            archive.WriteEntry("UnitData.json", unitData);
            archive.WriteEntry("Hierarchy.json", hierarchyJson);

            if (unit.MeshWriting == MeshWriteMode.Bundle)
                archive.WriteEntry("Meshes.json", meshData);

            return true;
        }

        public static bool SerializeObject(GameObject prefab, ObjectType type, string outputName, string meshCollection)
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
                Debug.Log("Wrote Hierarchy...");

                Debug.Log($"Archive now includes {archive.EntryNames.Length} entries");
            }

            Debug.Log("Done!");
            return true;
        }
#endif

        // Made an oopsie in the last commit.
        public static Transform? DeserializeObject(string filename, bool hideByDefault = true)
        {
            string fullpath = Path.Join(AssetDirectory, $"{filename}.zip");

            using ArchiveWrapper archive = new ArchiveWrapper(fullpath, false);

            UnitDefinition? data = null;
            MeshCollection? meshes = null;
            TransformWrapper? hierarchy = null;

            if (archive.ReadEntry("UnitData.json", out string unitjson))
                data = JsonSerializer.Deserialize<UnitDefinition>(unitjson, jsonOptions);

            if(data is null)
            {
                Debug.LogError($"No unit definition was found in {filename}.zip. Skipping.");
                return null;
            }

            meshes = FindMeshes(data, archive, AssetDirectory);

            if(archive.ReadEntry("Hierarchy.json", out string hierarchyjson))
                hierarchy = JsonSerializer.Deserialize<TransformWrapper>(hierarchyjson, jsonOptions);

            Transform? transform = hierarchy?.GenerateGameobjects();

            if (hideByDefault)
                transform?.gameObject.SetActive(false);

            return transform;
        }

        private static MeshCollection FindMeshes (UnitDefinition data, ArchiveWrapper archive, string rootfolder)
        {
            // Meshes are bundled within the unit file
            if(data.MeshWriting == MeshWriteMode.Bundle)
            {
                if(archive.ReadEntry("Meshes.json", out string meshjson))
                {
                    MeshCollection mc = MeshLibrary.AddFromJson(data.UnitName, meshjson);
                    MeshLibrary.Current = mc;
                    return mc;
                }
            }
            // Meshes are written to an external file, we must find them externally
            else if (data.MeshWriting == MeshWriteMode.Write && data.MeshCollection != string.Empty)
            {
                throw new NotImplementedException();
            }

            // The unit doesn't know where its meshes are stored. Oops!
            throw new FileNotFoundException(
                $"Unit {data.UnitName} does not have any mesh collections defined! This is a problem!");
        }
    }
}
