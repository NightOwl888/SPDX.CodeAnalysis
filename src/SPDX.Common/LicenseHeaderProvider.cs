using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SPDX.CodeAnalysis
{
    public class LicenseHeaderProvider : ILicenseHeaderProvider
    {
        // Outer key = LICENSES.HEADERS directory full path
        // Inner key = "MIT" from "MIT.txt", etc.
        private readonly Dictionary<string, LicenseHeaderCacheOld> _cache = new(StringComparer.Ordinal);
        private readonly object _lock = new object();
        private readonly IFileSystem _fileSystem;
        private readonly ILicenseDiscoveryStrategy _discoveryStrategy;

        public LicenseHeaderProvider(IFileSystem fileSystem, ILicenseDiscoveryStrategy discoveryStrategy)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _discoveryStrategy = discoveryStrategy ?? throw new ArgumentNullException(nameof(discoveryStrategy));
        }

        private static IReadOnlyList<IReadOnlyList<string>> Empty { get; } = new List<IReadOnlyList<string>>().AsReadOnly();

        public bool TryGetLicenseHeader(string fileDirectory, string topLevelDirectory, ReadOnlySpan<char> spdxLicenseIdentifier, out IReadOnlyList<IReadOnlyList<string>> result)
        {
            if (string.IsNullOrWhiteSpace(fileDirectory))
                throw new ArgumentNullException(nameof(fileDirectory));

            // Use discovery strategy
            var licenseLocation = _discoveryStrategy.FindLicenseLocation(fileDirectory, topLevelDirectory, spdxLicenseIdentifier);
            if (licenseLocation == null)
            {
                result = Empty;
                return false;
            }

            // Cache based on exact location (file or folder)
            var cache = GetOrAddCache(licenseLocation, spdxLicenseIdentifier);

            return cache.TryGetLicenseHeaders(spdxLicenseIdentifier, out result);
        }

        private LicenseHeaderCacheOld GetOrAddCache(string licenseLocation, ReadOnlySpan<char> spdxLicenseIdentifier)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(licenseLocation, out LicenseHeaderCacheOld existing))
                    return existing;

                LicenseHeaderCacheOld newCache = CreateLicenseHeaderCache(licenseLocation, spdxLicenseIdentifier);
                _cache[licenseLocation] = newCache;
                return newCache;
            }
        }

        private LicenseHeaderCacheOld CreateLicenseHeaderCache(string licenseLocation, ReadOnlySpan<char> spdxLicenseIdentifier)
        {
            var map = new Dictionary<StringKey, List<IReadOnlyList<string>>>(StringKey.Comparer);
            var key = new StringKey(spdxLicenseIdentifier.ToString());

            if (_fileSystem.DirectoryExists(licenseLocation))
            {
                foreach (var subFile in _fileSystem.EnumerateFiles(licenseLocation, "*.txt"))
                    AddToMap(map, key, subFile);
            }
            else
            {
                AddToMap(map, key, licenseLocation);
            }

            var readOnly = new Dictionary<StringKey, IReadOnlyList<IReadOnlyList<string>>>(map.Count, StringKey.Comparer);
            foreach (var kvp in map)
            {
                readOnly[kvp.Key] = kvp.Value; // Upcast: List<T> → IReadOnlyList<T>
            }
            return new LicenseHeaderCacheOld(readOnly);
        }

        private void AddToMap(Dictionary<StringKey, List<IReadOnlyList<string>>> map, StringKey key, string path)
        {
            var lines = new List<string>();
            using var reader = _fileSystem.OpenText(path);
            string line;
            while ((line = reader.ReadLine()) is not null)
                lines.Add(line);

            var list = new List<IReadOnlyList<string>> { lines.AsReadOnly() };

            map.TryGetValue(key, out var existing);
            if (existing != null)
                existing.AddRange(list);
            else
                map[key] = list;
        }
    }
}
