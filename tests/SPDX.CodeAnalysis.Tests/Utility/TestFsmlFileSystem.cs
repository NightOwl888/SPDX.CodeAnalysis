// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using NUnit.Framework;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis.Tests
{
    public class TestFsmlFileSystem
    {
        public static IEnumerable<TestCaseData> NestedDirctoryTestCases
        {
            get
            {
                // Expect true, 2 headers: Standard & Microsoft
                yield return new TestCaseData(
                   "project/src/specialized/stuff",
                   "MIT",
                   new List<IReadOnlyList<string>>
                   {
                        new List<string> { "Std line1", "Std line2" },
                        new List<string> { "MS line1", "MS line2" }
                   }
                );// .SetName("MIT_with_two_files");

                // Expect true, project-level MIT
                yield return new TestCaseData(
                   "project/src/specialized/",
                   "MIT",
                   new List<IReadOnlyList<string>>
                   {
                        new List<string> { "MIT line1", "MIT line2" }
                   }
                );

                // Expect true, 1 header: project/LICENSES.HEADERS/Apache-2.0.txt
                yield return new TestCaseData(
                   "project/src/specialized/stuff",
                   "Apache-2.0",
                   new List<IReadOnlyList<string>>
                   {
                        new List<string> { "Apache line1", "Apache line2" }
                   }
                );

                // Expect true, project-level MIT
                yield return new TestCaseData(
                   "project/src/normal",
                   "MIT",
                   new List<IReadOnlyList<string>>
                   {
                        new List<string> { "MIT line1", "MIT line2" }
                   }
                );

                // Expect true, Apache at project-level
                yield return new TestCaseData(
                   "project/src/normal",
                   "Apache-2.0",
                   new List<IReadOnlyList<string>>
                   {
                        new List<string> { "Apache line1", "Apache line2" }
                   }
                );
            }
        }

        [TestCaseSource(nameof(NestedDirctoryTestCases))]
        public void TestNestedDirectory(string codeDir, string spdx, IReadOnlyList<IReadOnlyList<string>> expected)
        {
            string fsml = @"
<filesystem>
  <directory name=""project"">
    <directory name=""LICENSES.HEADERS"">
      <file name=""MIT.txt""><![CDATA[MIT line1
MIT line2]]></file>
      <file name=""Apache-2.0.txt""><![CDATA[Apache line1
Apache line2]]></file>
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
          <file name=""foo.cs""/>
        </directory>
        <file name=""baz.cs""/>
      </directory>
      <directory name=""normal"">
        <file name=""bar.cs""/>
      </directory>
    </directory>
  </directory>
</filesystem>
";

            var fs = new FsmlFileSystem(fsml);
            var discovery = new ParentDirectorySpdxDiscoveryStrategy(fs);
            var provider = new LicenseHeaderProvider(fs, discovery);

            bool ok = provider.TryGetLicenseHeader(codeDir, "LICENSES.HEADERS", spdx, out var headers);

            Assert.That(ok, Is.True);
            Assert.That(headers, Is.EqualTo(expected));
        }
    }
}
