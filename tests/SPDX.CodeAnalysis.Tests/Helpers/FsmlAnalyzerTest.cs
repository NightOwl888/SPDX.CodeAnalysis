// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    //public class FsmlAnalyzerTest : FsmlAnalyzerTest<Verifier>
    //{
    //    public FsmlAnalyzerTest(string fsmlXml, CodeLanguage language) : base(fsmlXml, language)
    //    {
    //    }

    //    protected override DiagnosticAnalyzer CreateCSharpAnalyzer()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override DiagnosticAnalyzer CreateVisualBasicAnalyzer()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public abstract class FsmlAnalyzerTest<TVerifier> : FilteredLanguageAnalyzerTest<TVerifier>
        where TVerifier : IVerifier, new()
    {
        private readonly string fsmlXml;
        private readonly IFileSystem fileSystem;
        private readonly ILicenseHeaderConfigurationReader licenseHeaderConfiguration;
        private readonly string topLevelDirectoryName;
        private string testCode;
        private string testCodeFilePath;
        private bool sourcesAdded;
        private bool additionalFilesAdded;

        protected FsmlAnalyzerTest(string fsmlXml, CodeLanguage language, string topLevelDirectoryName)
            : base(language)
        {
            this.fsmlXml = fsmlXml ?? throw new ArgumentNullException(nameof(fsmlXml));
            this.topLevelDirectoryName = topLevelDirectoryName ?? throw new ArgumentNullException(nameof(topLevelDirectoryName));
            this.fileSystem = new FsmlFileSystem(fsmlXml);
            this.licenseHeaderConfiguration = new LicenseHeaderConfigurationReader(fileSystem);
        }

        // TODO: Remove this
        protected virtual IFileSystem CreateFileSystem() => fileSystem;


        /// <summary>
        /// Gets or sets the input source file for analyzer or code fix testing.
        /// </summary>
        /// <seealso cref="TestState"/>
        public new string TestCode
        {
            get => testCode;
            set => testCode = value;
        }

        /// <summary>
        /// Gets or sets the input source file path for analyzer or code fix testing.
        /// </summary>
        public string TestCodeFilePath
        {
            get => testCodeFilePath;
            set => testCodeFilePath = value;
        }

        protected override Task RunImplAsync(CancellationToken cancellationToken)
        {
            bool hasTestCodeFilePath = !string.IsNullOrEmpty(testCodeFilePath);

            if (!sourcesAdded && testCode is not null)
            {
                if (hasTestCodeFilePath)
                {
                    TestState.Sources.Add((Path.GetFullPath(testCodeFilePath), SourceText.From(testCode)));
                }
                else
                {
                    TestState.Sources.Add(testCode);
                }
                sourcesAdded = true;
            }
            // else assume that the user configured it directly using TestState.Sources (which may mean that they configured more than one source file)

            if (!additionalFilesAdded && hasTestCodeFilePath)
            {
                foreach (LicenseHeaderFile file in licenseHeaderConfiguration.GetLicenseHeaderFiles(testCodeFilePath, topLevelDirectoryName))
                {
                    TestState.AdditionalFiles.Add((file.FullFilePath, file.Content));
                }
                additionalFilesAdded = true;
            }

            return base.RunImplAsync(cancellationToken);
        }
    }
}
