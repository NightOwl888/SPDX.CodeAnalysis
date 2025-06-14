using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    public class InMemoryLicenseHeaderProvider //: ILicenseHeaderProvider // TODO: Re-implement this interface for testing
    {
        private readonly Dictionary<string, LicenseHeaderCache> _cache;

        public InMemoryLicenseHeaderProvider(Dictionary<string, IDictionary<string, string>> mockData)
        {
            _cache = new Dictionary<string, LicenseHeaderCache>(StringComparer.Ordinal);

            foreach (var (dir, entries) in mockData)
            {
                var map = new Dictionary<StringKey, IReadOnlyList<string>>(StringKey.Comparer);
                foreach (var (spdxId, lines) in entries)
                    map[new StringKey(spdxId)] = LinesToList(lines);

                //_cache[dir] = new LicenseHeaderCache(map);
            }
        }

        private static List<string> LinesToList(string text)
        {
            var result = new List<string>();
            foreach (var line in text.SplitLines())
            {
                result.Add(line.Trim().ToString());
            }
            return result;
        }

        public bool TryGetLicenseHeader(string fileDirectory, ReadOnlySpan<char> spdxLicenseIdentifier, out IReadOnlyList<string> result)
        {
            if (_cache.TryGetValue(fileDirectory, out var licenseCache))
            {
                //return licenseCache.TryGetLicenseHeader(spdxLicenseIdentifier, out result);
            }

            result = Array.Empty<string>();
            return false;
        }


        //private readonly Dictionary<string, Dictionary<string, IReadOnlyList<string>>> _mockData;

        //public InMemoryLicenseHeaderProvider(Dictionary<string, Dictionary<string, IReadOnlyList<string>>> mockData)
        //{
        //    _mockData = mockData;
        //}

        ////public IReadOnlyDictionary<string, IReadOnlyList<string>> GetLicenseHeaders(string directory)
        ////{
        ////    return _mockData.TryGetValue(directory, out var headers)
        ////        ? headers
        ////        : new Dictionary<string, IReadOnlyList<string>>();
        ////}

        //public bool TryGetLicenseHeader(string fileDirectory, ReadOnlySpan<char> spdxLicenseIdentifier, out IReadOnlyList<string> result)
        //{

        //}
    }
}
