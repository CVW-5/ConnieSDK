using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

#nullable enable
namespace ConnieSDK
{
    public static class UnitSerializer
    {
        public static string AssetDirectory { get; private set; } =
            Path.Join(Environment.CurrentDirectory, "Assets");
        public static Dictionary<string, string> FileTypes =
            new Dictionary<string, string>
            {
                {"Unit","zip" },
                {"MapData", "cwm" },
                {"Stores","cws" },
            };

        public static void SetAssetDirectory (string path)
        {
            Debug.Log($"Switched target directory to {path}\n" +
                $"Was {AssetDirectory}");

            AssetDirectory = path;
        }

#if UNITY_EDITOR
        public static bool SerializeUnit (GameObject prefab, string outputName)
        {
            string fullpath = Path.Join(AssetDirectory, $"{outputName}.{FileTypes["Unit"]}");
            Debug.Log($"Serializing a prefab to {fullpath}...");

            if(!Directory.Exists(AssetDirectory))
            {
                Directory.CreateDirectory(AssetDirectory);
            }
            if (!File.Exists(fullpath))
            {
                Debug.Log("-Target file does not exist, creating it...");
                GenerateArchive(fullpath);
            }

            using(FileStream zipfile = File.Open(fullpath, FileMode.OpenOrCreate))
            {
                using (ZipArchive archive = new ZipArchive(zipfile, ZipArchiveMode.Update))
                {
                    if(archive.GetEntry("EmptyAsset") is ZipArchiveEntry ent)
                    {
                        // Archive creation artifact, delete it!
                        ent.Delete();
                    }

                    var entry = archive.GetEntry("Gameobject.txt") ?? archive.CreateEntry("Gameobject.txt");

                    using (var stream = entry.Open())
                    {
                        StreamWriter writer = new StreamWriter(stream);

                        foreach(var comp in prefab.GetComponents(typeof(Component)))
                        {
                            writer.WriteLine(comp.ToString());
                        }

                        writer.Close();
                    }
                }
            }

            Debug.Log("Done!");
            return false;
        }

        private static void GenerateArchive (string path)
        {
            using (FileStream stream = File.Create(path))
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, false))
                    archive.CreateEntry("EmptyAsset");
            }
        }
#endif

    }
}
