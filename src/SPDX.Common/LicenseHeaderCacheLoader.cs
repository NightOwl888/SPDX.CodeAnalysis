using System;
using System.Collections.Generic;
using System.Text;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// Loads a license header cache 
    /// </summary>
    public class LicenseHeaderCacheLoader : ILicenseHeaderCacheLoader
    {
        private readonly ILicenseHeaderConfigurationReader reader;

        public LicenseHeaderCacheLoader(ILicenseHeaderConfigurationReader reader)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public IReadOnlyList<LicenseHeaderCacheText> LoadLicenseHeaders(string codeFilePath, string topLevelDirectoryName)
        {
            List<LicenseHeaderCacheText> result = new();
            foreach (LicenseHeaderFile file in reader.GetLicenseHeaderFiles(codeFilePath, topLevelDirectoryName))
            {
                result.Add(new LicenseHeaderCacheText(file));
            }
            return result.AsReadOnly();
        }
    }
}
