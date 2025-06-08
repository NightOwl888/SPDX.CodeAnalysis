using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace SPDX.CodeAnalysis
{
    public class FileSystemLicenseHeaderProvider : ILicenseHeaderProvider
    {
        // Outer key = LICENSES.HEADERS directory full path
        // Inner key = "MIT" from "MIT.txt", etc.
        private readonly ConcurrentDictionary<string, Lazy<LicenseHeaderCache>> _cache =
            new(StringComparer.Ordinal);

        public bool TryGetLicenseHeader(string fileDirectory, ReadOnlySpan<char> spdxLicenseIdentifier, out IReadOnlyList<string> result)
        {
            if (string.IsNullOrWhiteSpace(fileDirectory))
                throw new ArgumentNullException(nameof(fileDirectory));

            string? headerDir = FindLicenseHeaderDirectory(fileDirectory);
            if (headerDir is null)
            {
                result = Array.Empty<string>();
                return false;
            }

            var licenseCache = _cache.GetOrAdd(headerDir, static dir => new Lazy<LicenseHeaderCache>(() =>
            {
                var map = new Dictionary<StringKey, IReadOnlyList<string>>(StringKey.Comparer);
                foreach (var file in Directory.EnumerateFiles(dir, "*.txt", SearchOption.TopDirectoryOnly))
                {
                    string name = Path.GetFileNameWithoutExtension(file); // "MIT" from "MIT.txt"
                    map[new StringKey(name)] = ReadLinesFromFile(file);
                }
                return new LicenseHeaderCache(map);
            })).Value;

            return licenseCache.TryGetLicenseHeader(spdxLicenseIdentifier, out result);

            //int hashCode = StringHelper.GetHashCode(spdxLicenseIdentifier);

            //foreach (var kvp in licenseMap)
            //{
            //    StringKey key = kvp.Key;
            //    if (key.GetHashCode() == hashCode && key.Equals(spdxLicenseIdentifier))
            //    {
            //        result = kvp.Value;
            //        return true;
            //    }
            //}

            //result = Array.Empty<string>();
            //return false;

            ////return licenseMap[$"{spdxLicenseIdentifier}.txt"];
        }

        //public IReadOnlyDictionary<string, IReadOnlyList<string>> GetLicenseHeaders(string fileDirectory)
        //{
        //    string? headerDir = FindLicenseHeaderDirectory(fileDirectory);
        //    if (headerDir is null)
        //        return Empty;

        //    var licenseMap = _cache.GetOrAdd(headerDir, static dir => new Lazy<Dictionary<string, IReadOnlyList<string>>>(() =>
        //    {
        //        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        //        foreach (var file in Directory.EnumerateFiles(dir, "*.txt", SearchOption.TopDirectoryOnly))
        //        {
        //            var name = Path.GetFileNameWithoutExtension(file); // "MIT" from "MIT.txt"
        //            result[name] = ReadLinesFromFile(file);
        //        }
        //        return result;
        //    })).Value;

        //    return licenseMap;
        //}

        private static string? FindLicenseHeaderDirectory(string startingDirectory)
        {
            var directory = startingDirectory;
            while (directory != null)
            {
                var candidate = Path.Combine(directory, "LICENSES.HEADERS");
                if (Directory.Exists(candidate))
                    return candidate;

                directory = Path.GetDirectoryName(directory);
            }

            return null;
        }

        private static List<string> ReadLinesFromFile(string path)
        {
            var lines = new List<string>();
            using var reader = File.OpenText(path);
            while (reader.ReadLine() is { } line)
                lines.Add(line);

            return lines;
        }

        //private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Empty =
        //    new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

        //private static readonly IReadOnlyList<string> Empty = new List<string>().AsReadOnly();

        


        //public IReadOnlyDictionary<string, IReadOnlyList<string>> GetLicenseHeaders(string directory)
        //{
        //    var dict = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        //    if (!Directory.Exists(directory))
        //        return dict;

        //    foreach (var file in Directory.EnumerateFiles(directory, "*.txt", SearchOption.TopDirectoryOnly))
        //    {
        //        var key = Path.GetFileName(file).Trim();
        //        dict[key] = ReadLinesFromFile(file);
        //    }

        //    return dict;
        //}

        //private static List<string> ReadLinesFromFile(string path)
        //{
        //    var lines = new List<string>();
        //    using var reader = File.OpenText(path);
        //    while (reader.ReadLine() is { } line)
        //    {
        //        lines.Add(line.Trim());
        //    }
        //    return lines;
        //}

    }
}
