// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public IEnumerable<LicenseHeaderFile> GetLicenseHeaderFiles(string codeFilePath, string topLevelDirectoryName)
        {
            string root = FindLicenseHeaderRootDirectory(codeFilePath, topLevelDirectoryName.AsSpan());
            if (!string.IsNullOrEmpty(root))
            {
                return GetLicenseHeaderFilesWithinRoot(root, topLevelDirectoryName);
            }

            return Enumerable.Empty<LicenseHeaderFile>();
        }

        private IEnumerable<LicenseHeaderFile> GetLicenseHeaderFilesWithinRoot(string root, string topLevelDirectoryName)
        {
            var queue = new Queue<string>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // to avoid case-sensitive duplicates
            queue.Enqueue(root);

            string matchPath = GetMatchPath(topLevelDirectoryName);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                // Skip already-visited directories
                if (!visited.Add(current))
                    continue;

                foreach (string file in fileSystem.EnumerateFiles(current, "*.txt")
                    .Where(f => f.Contains(matchPath)))
                {
                    string spdxIdentifier = GetSpdxIdentifier(file, topLevelDirectoryName);
                    string matchDirectoryPath = GetMatchDirectoryPath(file, topLevelDirectoryName);
                    yield return new LicenseHeaderFile(spdxIdentifier, file, matchDirectoryPath, fileSystem.OpenText(file).ReadToEnd());
                }

                foreach (string sub in fileSystem.EnumerateDirectories(current, "*")
                    .Where(d => d.Contains(matchPath)))
                {
                    queue.Enqueue(sub);
                }
            }
        }

        /// <summary>
        /// Gets the configuration directory for the passed in configuration file.
        /// This is the directory that source files will match against to determine
        /// whether the license text configuration applies to the code file.
        /// </summary>
        /// <param name="licenseTextFilePath">The license header text path. This must contain a segment with <paramref name="topLevelDirectoryName"/> in it.</param>
        /// <param name="topLevelDirectoryName">The configuration directory name that the <paramref name="licenseTextFilePath"/> applies to.</param>
        /// <returns>The configuration directory path that the  <paramref name="licenseTextFilePath"/> applies to.</returns>
        private string GetMatchDirectoryPath(string licenseTextFilePath, string topLevelDirectoryName)
        {
            string matchPath = GetMatchPath(topLevelDirectoryName);
            int index = licenseTextFilePath.IndexOf(matchPath);
            Debug.Assert(index >= 0, "topLevelDirectoryName must be in every valid licenseTextFilePath.");
            return licenseTextFilePath.Substring(0, index);
        }

        private string GetMatchPath(string topLevelDirectoryName)
            => Path.DirectorySeparatorChar + topLevelDirectoryName;


        private string GetSpdxIdentifier(string licenseTextFilePath, string topLevelDirectoryName)
        {
            PathSplitEnumerator enumerator = licenseTextFilePath.SplitPath();
            ReadOnlySpan<char> topLevelDirectoryNameSpan = topLevelDirectoryName.AsSpan();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Segment.SequenceEqual(topLevelDirectoryNameSpan))
                {
                    // Advance one more time to get the SPDX identifier
                    enumerator.MoveNext();
                    break;
                }
            }
            ReadOnlySpan<char> candidate = enumerator.Current.Segment;
            int dotIndex = candidate.IndexOf('.');
            if (dotIndex >= 0)
            {
                return candidate.Slice(0, dotIndex).ToString();
            }
            return candidate.ToString();
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
