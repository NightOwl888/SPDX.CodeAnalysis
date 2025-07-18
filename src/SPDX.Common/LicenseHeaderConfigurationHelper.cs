// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// Utilities for dealing with license header configuration file paths.
    /// </summary>
    public static class LicenseHeaderConfigurationHelper
    {
        /// <summary>
        /// Gets the configuration directory for the passed in configuration file.
        /// This is the directory that source files will match against to determine
        /// whether the license text configuration applies to the code file.
        /// </summary>
        /// <param name="licenseTextFilePath">The license header text path. This must contain a segment with <paramref name="topLevelDirectoryName"/> in it.</param>
        /// <param name="topLevelDirectoryName">The configuration directory name that the <paramref name="licenseTextFilePath"/> applies to.</param>
        /// <returns>The configuration directory path that the  <paramref name="licenseTextFilePath"/> applies to.</returns>
        public static string GetMatchDirectoryPath(string licenseTextFilePath, string topLevelDirectoryName)
        {
            string matchPath = GetMatchPath(topLevelDirectoryName);
            int index = licenseTextFilePath.IndexOf(matchPath);
            Debug.Assert(index >= 0, "topLevelDirectoryName must be in every valid licenseTextFilePath.");
            return licenseTextFilePath.Substring(0, index);
        }

        /// <summary>
        /// Gets a path to check for to identify a valid license configuration file path.
        /// </summary>
        /// <param name="topLevelDirectoryName">The configuration directory name.</param>
        /// <returns>The match path to check using <see cref="string.Contains(string)"/>
        /// in a file path. If <c>true</c>, this is a candidate for a license text file path.</returns>
        public static string GetMatchPath(string topLevelDirectoryName)
            => Path.DirectorySeparatorChar + topLevelDirectoryName;

        /// <summary>
        /// Gets the SPDX-License-Identifier value from the <paramref name="licenseTextFilePath"/>.
        /// </summary>
        /// <param name="licenseTextFilePath">The license header text path. This must contain a segment with <paramref name="topLevelDirectoryName"/> in it.</param>
        /// <param name="topLevelDirectoryName">The configuration directory path that the <paramref name="licenseTextFilePath"/> applies to.</param>
        /// <returns>The SPDX-License-Identifier, based on the file name or directory name below <paramref name="topLevelDirectoryName"/>.</returns>
        public static string GetSpdxLicenseIdentifier(string licenseTextFilePath, string topLevelDirectoryName)
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
            int dotIndex = candidate.LastIndexOf('.');
            if (dotIndex >= 0)
            {
                return candidate.Slice(0, dotIndex).ToString();
            }
            return candidate.ToString();
        }
    }
}
