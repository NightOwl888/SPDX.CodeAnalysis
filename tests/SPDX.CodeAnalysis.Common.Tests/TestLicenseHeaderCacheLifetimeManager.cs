// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using SPDX.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SPDX.Common.Tests
{
    public class TestLicenseHeaderCacheLifetimeManager
    {
        [Test]
        public void VerifyProcessArchitecture_ARM64()
        {
            Console.WriteLine($"OS Architecture: {RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine($"Framework Description: {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"Is 64-bit Process: {Environment.Is64BitProcess}");

            Assert.That(RuntimeInformation.ProcessArchitecture, Is.EqualTo(Architecture.Arm64));
        }

        [Test]
        public void VerifyProcessArchitecture_X86()
        {
            Console.WriteLine($"OS Architecture: {RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine($"Framework Description: {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"Is 64-bit Process: {Environment.Is64BitProcess}");

            Assert.That(RuntimeInformation.ProcessArchitecture, Is.EqualTo(Architecture.X86));
        }

        [Test]
        public void VerifyProcessArchitecture_X64()
        {
            Console.WriteLine($"OS Architecture: {RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine($"Framework Description: {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"Is 64-bit Process: {Environment.Is64BitProcess}");

            Assert.That(RuntimeInformation.ProcessArchitecture, Is.EqualTo(Architecture.X64));
        }

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
