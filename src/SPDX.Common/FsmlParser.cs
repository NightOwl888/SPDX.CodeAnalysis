// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SPDX.CodeAnalysis
{
    public static class FsmlParser
    {
        public static Dictionary<string, IDictionary<string, string>> Parse(string xml, IRootPathNormalizer normalizer)
        {
            using var reader = new StringReader(xml);
            return Parse(reader, normalizer);
        }

        public static Dictionary<string, IDictionary<string, string>> Parse(Stream xmlStream, IRootPathNormalizer normalizer)
        {
            using var reader = new StreamReader(xmlStream);
            return Parse(reader, normalizer);
        }

        private static Dictionary<string, IDictionary<string, string>> Parse(TextReader reader, IRootPathNormalizer normalizer)
        {
            if (normalizer is null)
                throw new ArgumentNullException(nameof(normalizer));
            
            var doc = XDocument.Load(reader);
            var result = new Dictionary<string, IDictionary<string, string>>(StringComparer.Ordinal);

            var rootDirectory = doc.Root?.Elements().FirstOrDefault() ?? throw new InvalidOperationException("Root directory missing");
            ParseDirectory(rootDirectory, normalizer.RootPath, result);
            return result;
        }

        private static void ParseDirectory(XElement dirElement, string currentAbsolutePath, Dictionary<string, IDictionary<string, string>> result)
        {
            string dirName = dirElement.Attribute("name")?.Value
                             ?? throw new InvalidOperationException("Directory missing name");
            
            string fullPath = PathHelper.NormalizeAndCombine(currentAbsolutePath.AsSpan(), dirName.AsSpan());
            
            var files = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var file in dirElement.Elements("file"))
            {
                string name = file.Attribute("name")?.Value ?? throw new InvalidOperationException("File missing name");
                string content = file.Value;
                files[name] = content;
            }

            result[fullPath] = files;

            foreach (var subDir in dirElement.Elements("directory"))
            {
                ParseDirectory(subDir, fullPath, result);
            }
        }
    }
}
