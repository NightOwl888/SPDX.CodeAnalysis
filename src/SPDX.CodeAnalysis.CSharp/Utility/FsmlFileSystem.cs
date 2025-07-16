using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SPDX.CodeAnalysis
{
    public class FsmlFileSystem : IFileSystem // Only used by tests
    {
        private readonly Dictionary<string, List<string>> _filesByDir = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _contentByFile = new(StringComparer.Ordinal);

        public FsmlFileSystem(string fsmlXml)
            : this(FsmlParser.Parse(fsmlXml))
        {
        }

        public FsmlFileSystem(Stream fsmlXml)
            : this(FsmlParser.Parse(fsmlXml))
        {
        }

        private FsmlFileSystem(IDictionary<string, IDictionary<string, string>> map)
        {
            foreach (var kv in map)
            {
                _filesByDir[kv.Key] = kv.Value.Keys.ToList();
                foreach (var fileName in kv.Value.Keys)
                {
                    //string fullPath = Path.Combine(kv.Key, fileName + ".txt");
                    string fullPath = PathHelper.NormalizeAndJoin(kv.Key.AsSpan(), $"{fileName}.txt".AsSpan());
                    _contentByFile[fullPath] = kv.Value[fileName];
                }
            }
        }

        public bool DirectoryExists(string path) =>
            _filesByDir.ContainsKey(path);

        public IEnumerable<string> EnumerateFiles(string path, string pattern)
        {
            if (!_filesByDir.TryGetValue(path, out var fileNames))
                return Array.Empty<string>();

            // only support "*.txt"
            //return fileNames.Select(fn => Path.Combine(path, fn + ".txt"));
            return fileNames.Select(fn => PathHelper.NormalizeAndJoin(path.AsSpan(), $"{fn}.txt".AsSpan()));
        }

        public IEnumerable<string> EnumerateDirectories(string path, string pattern)
        {
            // return subdirectories that exist physically under 'path'
            return _filesByDir.Keys
                .Where(dir => dir.StartsWith(path + Path.DirectorySeparatorChar))
                .Select(dir => dir);
        }

        public TextReader OpenText(string path)
        {
            if (_contentByFile.TryGetValue(path, out var content))
                return new StringReader(content);
            throw new FileNotFoundException(path);
        }
    }
}
