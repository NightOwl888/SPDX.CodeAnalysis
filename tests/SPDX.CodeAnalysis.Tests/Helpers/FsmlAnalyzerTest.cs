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
        private string testCode;
        private string testCodeFilePath;
        private bool sourcesAdded;

        protected FsmlAnalyzerTest(string fsmlXml, CodeLanguage language)
            : base(language)
        {
            this.fsmlXml = fsmlXml ?? throw new ArgumentNullException(nameof(fsmlXml));
        }

        protected virtual IFileSystem CreateFileSystem() => new FsmlFileSystem(fsmlXml);


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
            if (!sourcesAdded && testCode is not null)
            {
                if (!string.IsNullOrEmpty(testCodeFilePath))
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

            return base.RunImplAsync(cancellationToken);
        }
    }
}
