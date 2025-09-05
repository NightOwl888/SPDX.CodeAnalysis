// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using SPDX.CodeAnalysis;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;

namespace SPDX.Common.Tests
{
    public class TestLicenseHeaderCacheLifetimeManager
    {
        [Test]
        public void TestGetHash()
        {
            string expected = "Tju+cJ0oFTSO6Yjw233FrrgzGFxwneoafsVwo7xKu4c=";
            string hashFilePath = NormalizePath("obj/Release/netstandard2.0/DummyProject.SpdxLicenseHeaderFilesHash.txt");

            AdditionalText hashFile = new MockAdditionalText(hashFilePath, expected);
            ImmutableArray<AdditionalText> hashFiles = ImmutableArray.Create(hashFile);

            string? actual = LicenseHeaderCacheLifetimeManager.GetHash(hashFiles);

            Assert.That(actual, Is.EqualTo(expected));
        }

        private class MockAdditionalText : AdditionalText
        {
            private readonly SourceText _sourceText;

            public MockAdditionalText(string path, string content)
            {
                Path = path;
                _sourceText = SourceText.From(content, Encoding.UTF8);
            }

            public override string Path { get; }

            public override SourceText GetText(CancellationToken cancellationToken = default)
                => _sourceText;
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}
