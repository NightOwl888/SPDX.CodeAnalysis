using System;
using System.Collections.Generic;
using System.Text;

namespace SPDX.CodeAnalysis
{
    public sealed class LicenseHeaderCache
    {
        private readonly Dictionary<StringKey, IReadOnlyList<string>> _map;

        public LicenseHeaderCache(Dictionary<StringKey, IReadOnlyList<string>> map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public bool TryGetLicenseHeader(ReadOnlySpan<char> spdxLicenseIdentifier, out IReadOnlyList<string> result)
        {
            int hashCode = StringHelper.GetHashCode(spdxLicenseIdentifier);
            foreach (var kvp in _map)
            {
                var key = kvp.Key;
                if (key.GetHashCode() == hashCode && key.Equals(spdxLicenseIdentifier))
                {
                    result = kvp.Value;
                    return true;
                }
            }

            result = Array.Empty<string>();
            return false;
        }
    }
}
