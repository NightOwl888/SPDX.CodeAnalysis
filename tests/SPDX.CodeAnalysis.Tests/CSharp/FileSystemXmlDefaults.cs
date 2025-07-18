// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests.CSharp
{
    public static class FileSystemXmlDefaults
    {
        public const string Basic = @"<filesystem>
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

        public static string With1OverriddenLevel = @"<filesystem>
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
</filesystem>";

        public static string With2OverriddenLevels = @"<filesystem>
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
}
