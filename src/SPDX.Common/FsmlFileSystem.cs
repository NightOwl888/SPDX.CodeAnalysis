// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPDX.CodeAnalysis
{
    public class FsmlFileSystem : IFileSystem // Only used by tests
    {
        private readonly Dictionary<string, List<string>> filesByDir = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> contentByFile = new(StringComparer.Ordinal);
        private readonly IRootPathNormalizer normalizer;

        public FsmlFileSystem(string fsmlXml, IRootPathNormalizer normalizer)
            : this(FsmlParser.Parse(fsmlXml, normalizer), normalizer)
        {
        }

        public FsmlFileSystem(Stream fsmlXml, IRootPathNormalizer normalizer)
            : this(FsmlParser.Parse(fsmlXml, normalizer), normalizer)
        {
        }

        private FsmlFileSystem(IDictionary<string, IDictionary<string, string>> map, IRootPathNormalizer normalizer)
        {
            this.normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
            
            foreach (var kv in map)
            {
                filesByDir[kv.Key] = kv.Value.Keys.ToList();
                foreach (var fileName in kv.Value.Keys)
                {
                    string fullPath = PathHelper.NormalizeAndCombine(kv.Key.AsSpan(), fileName.AsSpan());
                    contentByFile[fullPath] = kv.Value[fileName];
                }
            }
        }

        public bool DirectoryExists(string path)
        {
            return filesByDir.ContainsKey(normalizer.Normalize(path));
        }

        public IEnumerable<string> EnumerateFiles(string path, string pattern)
        {
            string rootedPath = normalizer.Normalize(path);
            if (!filesByDir.TryGetValue(rootedPath, out var fileNames))
                return Array.Empty<string>();

            return fileNames.Select(fn => PathHelper.NormalizeAndCombine(rootedPath.AsSpan(), fn.AsSpan()));
        }

        public IEnumerable<string> EnumerateDirectories(string path, string pattern)
        {
            string rootedPath = normalizer.Normalize(path, ensureTrailingSlash: true);
            
            // return subdirectories that exist physically under 'path'
            return filesByDir.Keys
                .Where(dir => dir.StartsWith(rootedPath, StringComparison.Ordinal))
                .Select(dir => dir);
        }

        public TextReader OpenText(string path)
        {
            string rootedPath = normalizer.Normalize(path);
            
            if (contentByFile.TryGetValue(rootedPath, out var content))
                return new StringReader(content);
            throw new FileNotFoundException(path);
        }
    }
}
