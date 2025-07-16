// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.IO;

namespace SPDX.CodeAnalysis
{
    // TODO: Need to re-do this because IO is not allowed in an analyzer.
    // The two options are:
    // 1. Use AdditionalFiles in MSBuild
    // 2. Use a Source Generator to read IO and create metadata to share with the analyzer

    public sealed class FileSystem : IFileSystem
    {
        public bool DirectoryExists(string path)
            //=> Directory.Exists(path);
            => true;

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
            //=> Directory.EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
            => Array.Empty<string>();

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
            //=> Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
            => Array.Empty<string>();

        public TextReader OpenText(string path)
            //=> File.OpenText(path);
            => TextReader.Null;
    }
}
