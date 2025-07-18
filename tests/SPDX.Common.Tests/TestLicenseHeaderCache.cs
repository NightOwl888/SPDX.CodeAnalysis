using NUnit.Framework;
using SPDX.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPDX.Common.Tests
{
    public class TestLicenseHeaderCache
    {
        private static class FileSystemXml
        {
            public static string With2OverriddenLevels = @"<filesystem>
  <directory name=""project"">
    <directory name=""LICENSES.HEADERS"">
      <file name=""MIT.txt""><![CDATA[MIT line1
MIT line2]]></file>
      <file name=""Apache-2.0.txt""><![CDATA[Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
""License""); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.]]></file>
    </directory>
    <directory name=""src"">
      <directory name=""specialized"">
        <directory name=""stuff"">
          <directory name=""LICENSES.HEADERS"">
            <directory name=""MIT"">
              <file name=""Standard.txt""><![CDATA[Std line1
Std line2]]></file>
              <file name=""Microsoft.txt""><![CDATA[MS line1
MS line2]]></file>
            </directory>
          </directory>
          <directory name=""nested1"">
            <directory name=""nested2"">
              <directory name=""LICENSES.HEADERS"">
                <directory name=""MIT"">
                   <file name=""Special1""><![CDATA[Special1 line1
Special1 line2
Special1 line3]]></file>
                   <file name=""Special2""><![CDATA[Special2 line1
Special2 line2
Special2 line3]]></file>
                </directory>
              </directory>
              <file name=""special.cs""/>
            </directory>
          </directory>
          <file name=""foo.cs""/>
        </directory>
        <file name=""baz.cs""/>
      </directory>
      <directory name=""normal"">
        <file name=""bar.cs""/>
      </directory>
    </directory>
  </directory>
</filesystem>";
        }

        public const string TopLevelDirectoryName = "LICENSES.HEADERS";

        public static IEnumerable<TestCaseData> MatchingLicenseHeaderCases
        {
            get
            {
                yield return new TestCaseData(
                    FileSystemXml.With2OverriddenLevels,
                    "Apache-2",
                    "project/src/specialized/baz.cs",
                    TopLevelDirectoryName,
                    new[]
                    {
                    new LicenseHeaderCacheText(
                        "Apache-2",
                        "project/LICENSES.HEADERS/Apache-2.0.txt",
                        "project",
                        new[]
                        {
                            "Licensed to the Apache Software Foundation (ASF) under one",
                            "or more contributor license agreements.  See the NOTICE file",
                            "distributed with this work for additional information",
                            "regarding copyright ownership.  The ASF licenses this file",
                            "to you under the Apache License, Version 2.0 (the",
                            "\"License\"); you may not use this file except in compliance",
                            "with the License.  You may obtain a copy of the License at",
                            "",
                            "  http://www.apache.org/licenses/LICENSE-2.0",
                            "",
                            "Unless required by applicable law or agreed to in writing,",
                            "software distributed under the License is distributed on an",
                            "\"AS IS\" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY",
                            "KIND, either express or implied.  See the License for the",
                            "specific language governing permissions and limitations",
                            "under the License.",
                        }),
                    }
                ).SetName("TwoOverriddenLevels_FirstLevel_Apache2");

                yield return new TestCaseData(
                    FileSystemXml.With2OverriddenLevels,
                    "MIT",
                    "project/src/specialized/baz.cs",
                    TopLevelDirectoryName,
                    new[]
                    {
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/LICENSES.HEADERS/MIT.txt",
                        "project",
                        new[]
                        {
                            "MIT line1",
                            "MIT line2",
                        }),
                    }
                ).SetName("TwoOverriddenLevels_FirstLevel_MIT");

                yield return new TestCaseData(
                    FileSystemXml.With2OverriddenLevels,
                    "MIT",
                    "project/src/specific/nested1/nested2/nested3/nested4/nested5/nested6/something.cs",
                    TopLevelDirectoryName,
                    new[]
                    {
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/LICENSES.HEADERS/MIT.txt",
                        "project",
                        new[]
                        {
                            "MIT line1",
                            "MIT line2",
                        }),
                    }
                ).SetName("TwoOverriddenLevels_FirstLevel_MIT_DeepNestedSource");

                yield return new TestCaseData(
                    FileSystemXml.With2OverriddenLevels,
                    "MIT",
                    "project/src/specialized/stuff/foo.cs",
                    TopLevelDirectoryName,
                    new[]
                    {
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/src/specialized/stuff/LICENSES.HEADERS/MIT/Standard.txt",
                        "project/src/specialized/stuff",
                        new[]
                        {
                            "Std line1",
                            "Std line2",
                        }),
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/src/specialized/stuff/LICENSES.HEADERS/MIT/Microsoft.txt",
                        "project/src/specialized/stuff",
                        new[]
                        {
                            "MS line1",
                            "MS line2",
                        })
                    }
                ).SetName("TwoOverriddenLevels_SecondLevel_MIT");

                yield return new TestCaseData(
                    FileSystemXml.With2OverriddenLevels,
                    "MIT",
                    "project/src/specialized/stuff/nestedx/nestedy/nestedz/nested/too/far/foo.cs",
                    TopLevelDirectoryName,
                    new[]
                    {
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/src/specialized/stuff/LICENSES.HEADERS/MIT/Standard.txt",
                        "project/src/specialized/stuff",
                        new[]
                        {
                            "Std line1",
                            "Std line2",
                        }),
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/src/specialized/stuff/LICENSES.HEADERS/MIT/Microsoft.txt",
                        "project/src/specialized/stuff",
                        new[]
                        {
                            "MS line1",
                            "MS line2",
                        })
                    }
                ).SetName("TwoOverriddenLevels_SecondLevel_MIT_DeepNestedSource");

                yield return new TestCaseData(
                    FileSystemXml.With2OverriddenLevels,
                    "MIT",
                    "project/src/specialized/stuff/nested1/nested2/special.cs",
                    TopLevelDirectoryName,
                    new[]
                    {
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/src/specialized/stuff/nested1/nested2/LICENSES.HEADERS/MIT/Special1",
                        "project/src/specialized/stuff/nested1/nested2",
                        new[]
                        {
                            "Special1 line1",
                            "Special1 line2",
                            "Special1 line3",
                        })
                    ,
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/src/specialized/stuff/nested1/nested2/LICENSES.HEADERS/MIT/Special2",
                        "project/src/specialized/stuff/nested1/nested2",
                        new[]
                        {
                            "Special2 line1",
                            "Special2 line2",
                            "Special2 line3",
                        })
                    ,
                    }
                ).SetName("TwoOverriddenLevels_ThirdLevel_MIT");

                yield return new TestCaseData(
                    FileSystemXml.With2OverriddenLevels,
                    "MIT",
                    "project/src/specialized/stuff/nested1/nested2/nested3/nested4/nested5/nested6/nested.cs",
                    TopLevelDirectoryName,
                    new[]
                    {
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/src/specialized/stuff/nested1/nested2/LICENSES.HEADERS/MIT/Special1",
                        "project/src/specialized/stuff/nested1/nested2",
                        new[]
                        {
                            "Special1 line1",
                            "Special1 line2",
                            "Special1 line3",
                        })
                    ,
                    new LicenseHeaderCacheText(
                        "MIT",
                        "project/src/specialized/stuff/nested1/nested2/LICENSES.HEADERS/MIT/Special2",
                        "project/src/specialized/stuff/nested1/nested2",
                        new[]
                        {
                            "Special2 line1",
                            "Special2 line2",
                            "Special2 line3",
                        })
                    ,
                    }
                ).SetName("TwoOverriddenLevels_ThirdLevel_MIT_DeepNestedSource");
            }
        }

        [TestCaseSource(nameof(MatchingLicenseHeaderCases))]
        public void TestGetMatchingLicenseHeaders(string fileSystemXml, string spdxLicenseIdentifier, string codeFilePath, string topLevelDirectoryName, LicenseHeaderCacheText[] expected)
        {
            codeFilePath = NormalizePath(codeFilePath);

            LicenseHeaderCache target = InitializeCache(fileSystemXml, codeFilePath, topLevelDirectoryName);

            var actual = target.GetMatchingLicenseHeaders(spdxLicenseIdentifier.AsSpan(), codeFilePath);

            DumpToConsole(actual);

            Assert.Multiple(() =>
            {
                Assert.That(actual.Count, Is.EqualTo(expected.Length), "Count mismatch");

                for (int i = 0; i < expected.Length; i++)
                {
                    var exp = expected[i];
                    var act = actual[i];

                    Assert.That(act.SpdxLicenseIdentifier, Is.EqualTo(exp.SpdxLicenseIdentifier), $"Mismatch at [{i}].SpdxLicenseIdentifier");
                    Assert.That(act.FullFilePath, Is.EqualTo(NormalizePath(exp.FullFilePath)), $"Mismatch at [{i}].FullFilePath");
                    Assert.That(act.MatchDirectoryPath, Is.EqualTo(NormalizePath(exp.MatchDirectoryPath)), $"Mismatch at [{i}].MatchDirectoryPath");

                    Assert.That(act.Lines.Count, Is.EqualTo(exp.Lines.Count), $"Line count mismatch at [{i}]");

                    for (int j = 0; j < exp.Lines.Count; j++)
                    {
                        Assert.That(act.Lines[j], Is.EqualTo(exp.Lines[j]), $"Mismatch at [{i}].Lines[{j}]");
                    }
                }
            });
        }

        private static void DumpToConsole(IReadOnlyList<LicenseHeaderCacheText> actual)
        {
            string basePath = Path.GetFullPath(".");

            Console.WriteLine("new[]");
            Console.WriteLine("{");

            foreach (var item in actual)
            {
                Console.WriteLine("    new LicenseHeaderCacheText(");
                Console.WriteLine($"        \"{Escape(item.SpdxLicenseIdentifier)}\",");
                Console.WriteLine($"        \"{Escape(Path.GetRelativePath(basePath, item.FullFilePath))}\",");
                Console.WriteLine($"        \"{Escape(Path.GetRelativePath(basePath, item.MatchDirectoryPath))}\",");
                Console.WriteLine("        new[]");
                Console.WriteLine("        {");
                foreach (var line in item.Lines)
                {
                    Console.WriteLine($"            \"{Escape(line)}\",");
                }
                Console.WriteLine("        })");
                Console.WriteLine("    ,");
            }

            Console.WriteLine("}");

            static string Escape(string s)
            {
                return s
                    .Replace("\\", "/")            // Normalize paths
                    .Replace("\"", "\\\"");        // Escape quotes
            }
        }

        private static LicenseHeaderCache InitializeCache(string fileSystemXml, string codeFilePath, string topLevelDirectoryName)
        {
            var fileSystem = new FsmlFileSystem(fileSystemXml);
            var reader = new LicenseHeaderConfigurationReader(fileSystem);
            var loader = new LicenseHeaderCacheLoader(reader);
            var cache = new LicenseHeaderCache();

            cache.EnsureInitialized(loader, codeFilePath, topLevelDirectoryName);
            return cache;
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}
