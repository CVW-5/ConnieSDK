using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

#nullable enable
namespace ConnieSDK
{
    public class ArchiveWrapper
    {
        public string Filepath = string.Empty;

        public bool Loaded => Archive is ZipArchive;
        private ZipArchive? Archive;
        private Stream? rootStream;
        private StreamWriter? _writer;
        private StreamReader? _reader;

        public ArchiveWrapper (string filepath)
        {
            if (!File.Exists(filepath)) return;

            Filepath = filepath;
            rootStream = File.Open(Filepath, FileMode.Open);

            Archive = new ZipArchive(rootStream, ZipArchiveMode.Update);
        }

        public bool ReadEntry (string name, out string data)
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

        public void WriteEntry (string name, string data)
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
        }

        public void Dispose ()
        {
            _writer?.Dispose();
            _reader?.Dispose();
            rootStream?.Dispose();

            Archive?.Dispose();
        }
    }
}
