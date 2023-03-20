using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

#nullable enable
namespace ConnieSDK
{
    public static class ConnieSerializer
    {
        public static string AssetDirectory { get; private set; } =
            Path.Join(Environment.CurrentDirectory, "Assets");
        public static Dictionary<string, string> FileTypes =
            new Dictionary<string, string>
            {
                {"Unit","zip" },
                {"MapData", "zip" },
                {"Stores","zip" },
            };

        public static void SetAssetDirectory(string path)
        {
            Debug.Log($"Switched target directory to {path}\n" +
                $"Was {AssetDirectory}");

            AssetDirectory = path;
        }

#if UNITY_EDITOR
        public static bool SerializeUnit(GameObject prefab, string outputName)
        {
            string fullpath = Path.Join(AssetDirectory, $"{outputName}.{FileTypes["Unit"]}");
            Debug.Log($"Serializing a prefab to {fullpath}...");

            using ArchiveWrapper archive = new ArchiveWrapper(fullpath, true);

            if (archive.HasEntry("EmptyAsset"))
                archive.DeleteEntry("EmptyAsset");

            Debug.Log("Done!");
            return true;
        }
#endif

    }
}
