using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

#nullable enable
namespace ConnieSDK
{
    public class ArchiveWrapper: IDisposable
    {
        public string Filepath = string.Empty;

        public bool Loaded => Archive is ZipArchive;
        private ZipArchive? Archive;
        private Stream? rootStream;

        public ArchiveWrapper(string filepath, bool createIfNotExists = true)
        {
            if (!File.Exists(filepath) && !createIfNotExists)
                throw new FileNotFoundException($"The file at {filepath} not exist!");

            OpenArchive(filepath, false);
        }

        public ArchiveWrapper OpenArchive(string filepath, bool includeArtifact = false)
        {
            Filepath = filepath;
            string dir = Path.GetDirectoryName(Filepath);

            if (!Directory.Exists(dir))
                new DirectoryInfo(dir).Create();

            rootStream = File.Open(filepath, FileMode.OpenOrCreate);
            Archive = new ZipArchive(rootStream, ZipArchiveMode.Update, true);

            if(includeArtifact)
            {
                Archive.CreateEntry("EmptyAsset");
            }

            return this;
        }

        public string[] EntryNames
        {
            get => Archive?.Entries.Select(x => x.Name).ToArray() ?? new string[0];
        }

        public bool HasEntry(string name)
        {
            return Archive?.GetEntry(name) is ZipArchiveEntry;
        }

        public bool ReadEntry(string name, out string data)
        {
            data = string.Empty;

            if (Archive is null)
                throw new FileLoadException("The archive is not loaded! It either doesn't exist or wasn't loaded properly.");

            using Stream? entStream = Archive.GetEntry(name)?.Open();

            if (entStream is null)
                return false;

            using StreamReader entReader = new StreamReader(entStream);

            data = entReader.ReadToEnd();
            return true;
        }

        public void WriteEntry(string name, string data)
        {
            if (Archive is null)
                throw new FileLoadException("The archive is not loaded! It either doesn't exist or wasn't loaded properly.");

            ZipArchiveEntry? entry = Archive.GetEntry(name);

            if (entry is null)
                entry = Archive.CreateEntry(name);

            using var entStream = entry.Open();
            using StreamWriter entWriter = new StreamWriter(entStream);

            entWriter.Write(data);
            entStream.SetLength(data.Length);
            entStream.Flush();
        }

        public void DeleteEntry (string name)
        {
            if(Archive?.GetEntry(name) is ZipArchiveEntry ent)
                ent.Delete();
        }

        public void Dispose()
        {
            Archive?.Dispose();
            rootStream?.Dispose();
        }
    }
}
