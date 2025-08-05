// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SPDX.CodeAnalysis
{
    public class LicenseHeaderCacheLifetimeManager : ILicenseHeaderCacheLifetimeManager
    {
        private const string HashFileSuffix = ".SpdxLicenseHeaderFilesHash.txt"; // TODO: Somehow share this with the SPDX.CodeAnalysis.Build.targets file

        private string? currentHash = null;
        private LicenseHeaderCache? cache;
        private readonly object syncLock = new();

        public LicenseHeaderCache GetCache(ImmutableArray<AdditionalText> additionalFiles, string topLevelDirectoryName, CancellationToken cancellationToken = default)
        {
            string hash = GetHash(additionalFiles, cancellationToken) ?? string.Empty;

            if (currentHash != hash || cache is null)
            {
                lock (syncLock)
                {
                    if (currentHash != hash || cache is null)
                    {
                        currentHash = hash;
                        cache = LoadCache(additionalFiles, topLevelDirectoryName, cancellationToken);
                    }
                }

            }
            return cache;
        }

        protected internal static LicenseHeaderCache LoadCache(ImmutableArray<AdditionalText> additionalFiles, string topLevelDirectoryName, CancellationToken cancellationToken = default)
        {
            // Load the configuration from AdditionalFiles
            return new LicenseHeaderCache(LoadLicenseHeaders(additionalFiles, topLevelDirectoryName, cancellationToken));
        }

        protected internal static string? GetHash(ImmutableArray<AdditionalText> additionalFiles, CancellationToken cancellationToken = default)
        {
            AdditionalText? hashFile = additionalFiles.FirstOrDefault(f =>
                f.Path.EndsWith(HashFileSuffix, StringComparison.Ordinal));

            return GetTrimmedSingleLine(hashFile, cancellationToken);
        }

        private static string? GetTrimmedSingleLine(AdditionalText? additionalText, CancellationToken cancellationToken = default)
        {
            if (additionalText is null)
                return null;

            SourceText? sourceText = additionalText.GetText(cancellationToken);
            if (sourceText is null || sourceText.Lines.Count == 0)
                return null;

            return sourceText.Lines[0].ToString().Trim();
        }

        private static IEnumerable<LicenseHeaderCacheText> LoadLicenseHeaders(ImmutableArray<AdditionalText> additionalFiles, string topLevelDirectoryName, CancellationToken cancellationToken = default)
        {
            foreach (AdditionalText file in GetLicenseHeaderFiles(additionalFiles, topLevelDirectoryName))
            {
                string spdxLicenseIdentifier = LicenseHeaderConfigurationHelper.GetSpdxLicenseIdentifier(file.Path, topLevelDirectoryName);
                string matchDirectoryPath = LicenseHeaderConfigurationHelper.GetMatchDirectoryPath(file.Path, topLevelDirectoryName);

                // Get the SourceText from AdditionalText
                SourceText? sourceText = file.GetText(cancellationToken);

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

                yield return new LicenseHeaderCacheText(spdxLicenseIdentifier, file.Path, matchDirectoryPath, lines);
            }
        }

        private static IEnumerable<AdditionalText> GetLicenseHeaderFiles(ImmutableArray<AdditionalText> additionalFiles, string topLevelDirectoryName)
        {
            string matchPath = LicenseHeaderConfigurationHelper.GetMatchPath(topLevelDirectoryName);
            return additionalFiles
                .Where(f => f.Path.Contains(matchPath) && f.Path.EndsWith(".txt", StringComparison.Ordinal));
        }
    }
}
