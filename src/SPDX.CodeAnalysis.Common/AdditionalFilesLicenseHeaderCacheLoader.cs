// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// A cache loader that can be used by a Roslyn code analyzer to load MSBuild-supplied AdditionalFiles
    /// into the <see cref="LicenseHeaderCache"/>.
    /// </summary>
    public sealed class AdditionalFilesLicenseHeaderCacheLoader : ILicenseHeaderCacheLoader
    {
        private readonly ImmutableArray<AdditionalText> additionalFiles;

        public AdditionalFilesLicenseHeaderCacheLoader(ImmutableArray<AdditionalText> additionalFiles)
        {
            this.additionalFiles = additionalFiles;
        }

        public IReadOnlyList<LicenseHeaderCacheText> LoadLicenseHeaders(string codeFilePath, string topLevelDirectoryName)
        {
            List<LicenseHeaderCacheText> result = new();
            string matchPath = LicenseHeaderConfigurationHelper.GetMatchPath(topLevelDirectoryName);

            foreach (AdditionalText file in additionalFiles
                .Where(f => f.Path.Contains(matchPath) && f.Path.EndsWith(".txt", StringComparison.Ordinal)))
            {
                string spdxLicenseIdentifier = LicenseHeaderConfigurationHelper.GetSpdxLicenseIdentifier(file.Path, topLevelDirectoryName);
                string matchDirectoryPath = LicenseHeaderConfigurationHelper.GetMatchDirectoryPath(file.Path, topLevelDirectoryName);

                // Get the SourceText from AdditionalText
                SourceText? sourceText = file.GetText();

                IReadOnlyList<string> lines;
                if (sourceText is not null)
                {
                    // Get the lines as IReadOnlyList<string>
                    lines = sourceText!.Lines
                        .Select(line => sourceText.ToString(line.Span))
                        .ToList();
                }
                else
                {
                    Debug.Assert(false); // We should never get here.
                    lines = Array.Empty<string>();
                }

                result.Add(new LicenseHeaderCacheText(spdxLicenseIdentifier, file.Path, matchDirectoryPath, lines));
            }
            return result;
        }
    }
}
