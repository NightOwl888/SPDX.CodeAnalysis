using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SPDX.CodeAnalysis
{
    public static class FsmlParser
    {
        public static Dictionary<string, IDictionary<string, string>> Parse(string xml)
        {
            using var reader = new StringReader(xml);
            return Parse(reader);
        }

        public static Dictionary<string, IDictionary<string, string>> Parse(Stream xmlStream)
        {
            using var reader = new StreamReader(xmlStream);
            return Parse(reader);
        }

        private static Dictionary<string, IDictionary<string, string>> Parse(TextReader reader)
        {
            var doc = XDocument.Load(reader);
            var result = new Dictionary<string, IDictionary<string, string>>(StringComparer.Ordinal);

            var rootDirectory = doc.Root?.Elements().FirstOrDefault() ?? throw new InvalidOperationException("Root directory missing");
            ParseDirectory(rootDirectory, "", result);
            return result;
        }

        private static void ParseDirectory(XElement dirElement, string currentPath, Dictionary<string, IDictionary<string, string>> result)
        {
            string dirName = dirElement.Attribute("name")?.Value ?? throw new InvalidOperationException("Directory missing name");
            //string fullPath = Path.Combine(currentPath.Replace('/', Path.DirectorySeparatorChar), dirName);
            string fullPath = PathHelper.NormalizeAndJoin(currentPath.AsSpan(), dirName.AsSpan());
            var files = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var file in dirElement.Elements("file"))
            {
                string name = file.Attribute("name")?.Value ?? throw new InvalidOperationException("File missing name");
                string content = file.Value;
                files[Path.GetFileNameWithoutExtension(name)] = content;
            }

            result[fullPath] = files;

            foreach (var subDir in dirElement.Elements("directory"))
            {
                ParseDirectory(subDir, fullPath, result);
            }
        }
    }
}
