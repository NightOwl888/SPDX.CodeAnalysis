// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// Represents the license header configuration based on a file system.
    /// </summary>
    public class LicenseHeaderConfigurationReader : ILicenseHeaderConfigurationReader
    {
        private readonly IFileSystem fileSystem;

        public LicenseHeaderConfigurationReader(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <inheritdoc/>
        public IEnumerable<(string filePath, string content)> GetLicenseHeaderFiles(string codeFilePath, string topLevelDirectoryName)
        {
            string root = FindLicenseHeaderRootDirectory(codeFilePath, topLevelDirectoryName.AsSpan());
            if (!string.IsNullOrEmpty(root))
            {
                return GetLicenseHeaderFilesWithinRoot(root, topLevelDirectoryName);
            }

            return Enumerable.Empty<(string filePath, string content)>();
        }

        private IEnumerable<(string filePath, string content)> GetLicenseHeaderFilesWithinRoot(string root, string topLevelDirectoryName)
        {
            var queue = new Queue<string>();
            queue.Enqueue(root);

            string matchPath = Path.DirectorySeparatorChar + topLevelDirectoryName;

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                foreach (string file in fileSystem.EnumerateFiles(current, "*.txt")
                    .Where(f => f.Contains(matchPath)))
                {
                    yield return (file, fileSystem.OpenText(file).ReadToEnd());
                }

                foreach (string sub in fileSystem.EnumerateDirectories(current, "*")
                    .Where(d => d.Contains(matchPath)))
                {
                    queue.Enqueue(sub);
                }
            }
        }

        private string FindLicenseHeaderRootDirectory(string codeFilePath, ReadOnlySpan<char> topLevelDirectoryName)
        {
            ReadOnlySpan<char> root = default;
            // NOTE: This currently only supports full file paths, not directory paths.
            ReadOnlySpan<char> current = PathHelper.GetDirectoryName(Path.GetFullPath(codeFilePath).AsSpan());

            while (!current.IsEmpty)
            {
                string candidate = PathHelper.NormalizeAndJoin(current, topLevelDirectoryName);
                if (fileSystem.DirectoryExists(candidate))
                {
                    root = current; // keep updating as we go up
                }

                current = PathHelper.GetDirectoryName(current); // go one level up
            }

            return root.ToString();
        }
    }
}
