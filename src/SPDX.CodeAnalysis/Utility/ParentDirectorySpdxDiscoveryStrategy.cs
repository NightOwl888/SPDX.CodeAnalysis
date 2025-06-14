using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SPDX.CodeAnalysis
{
    public sealed class ParentDirectorySpdxDiscoveryStrategy : ILicenseDiscoveryStrategy
    {
        private readonly IFileSystem _fs;

        public ParentDirectorySpdxDiscoveryStrategy(IFileSystem fs)
        {
            _fs = fs ?? throw new ArgumentNullException(nameof(fs));
        }

        public string? FindLicenseLocation(string startingDirectory, string topLevelDirName, ReadOnlySpan<char> spdxLicenseIdentifier)
        {
            //string fullPath = Path.GetFullPath(startingDirectory);
            //ReadOnlySpan<char> dir = fullPath.AsSpan();
            ReadOnlySpan<char> dir = startingDirectory.AsSpan();
            while (!PathInternal.IsEffectivelyEmpty(dir))
            {
                //string basePath = Path.Combine(dir, topLevelDirName);
                string basePath = PathHelper.NormalizeAndJoin(dir, topLevelDirName.AsSpan());

                if (_fs.DirectoryExists(basePath))
                {
                    // First: look for sub-directory
                    //string subPath = Path.Combine(basePath, spdxLicenseIdentifier.ToString());
                    string subPath = PathHelper.NormalizeAndJoin(basePath.AsSpan(), spdxLicenseIdentifier);
                    if (_fs.DirectoryExists(subPath))
                        return subPath;

                    // Then: look for single file
                    foreach (string file in _fs.EnumerateFiles(basePath, "*.txt"))
                    {
                        // Note that the filter above should only return files that end in .txt so this will always work
                        int dotIndex = file.LastIndexOf('.');

                        if (subPath.AsSpan().SequenceEqual(file.AsSpan(0, dotIndex)))
                        {
                            return StringHelper.Concat(subPath.AsSpan(), ".txt".AsSpan());
                        }
                    }
                }

                dir = PathHelper.GetDirectoryName(dir);
            }

            return null;
        }
    }
}
