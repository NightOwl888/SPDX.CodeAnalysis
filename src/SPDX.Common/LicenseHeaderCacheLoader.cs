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
        private readonly string codeFilePath;

        public LicenseHeaderCacheLoader(ILicenseHeaderConfigurationReader reader, string codeFilePath)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.codeFilePath = codeFilePath ?? throw new ArgumentNullException(nameof(codeFilePath));
        }

        public IReadOnlyList<LicenseHeaderCacheText> LoadLicenseHeaders(string topLevelDirectoryName)
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
