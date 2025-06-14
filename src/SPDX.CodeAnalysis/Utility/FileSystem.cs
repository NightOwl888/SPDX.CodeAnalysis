using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SPDX.CodeAnalysis
{
    public sealed class FileSystem : IFileSystem
    {
        public bool DirectoryExists(string path)
            => Directory.Exists(path);

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
            => Directory.EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
            => Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

        public TextReader OpenText(string path)
            => File.OpenText(path);
    }
}
