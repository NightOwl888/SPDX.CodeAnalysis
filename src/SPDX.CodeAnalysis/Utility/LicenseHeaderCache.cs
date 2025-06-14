using System;
using System.Collections.Generic;
using System.Text;

namespace SPDX.CodeAnalysis
{
    public sealed class LicenseHeaderCache
    {
        private readonly Dictionary<StringKey, IReadOnlyList<IReadOnlyList<string>>> _map;

        public LicenseHeaderCache(Dictionary<StringKey, IReadOnlyList<IReadOnlyList<string>>> map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public bool TryGetLicenseHeaders(ReadOnlySpan<char> spdxLicenseIdentifier, ReadOnlySpan<char> licenseLocation, out IReadOnlyList<IReadOnlyList<string>> candidateLicenseHeaders)
        {
            int hashCode = StringKey.GetHashCode(spdxLicenseIdentifier, licenseLocation);
            List<IReadOnlyList<string>> licenseHeaders = new List<IReadOnlyList<string>>();

            foreach (var kvp in _map)
            {
                if (kvp.Key.GetHashCode() == hashCode && kvp.Key.Equals(spdxLicenseIdentifier, licenseLocation))
                {
                    licenseHeaders.AddRange(kvp.Value);
                    break;
                }
            }

            candidateLicenseHeaders = licenseHeaders.AsReadOnly();
            return licenseHeaders.Count > 0;
        }
    }
}
