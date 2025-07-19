// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    using DiagnosticResult = Microsoft.CodeAnalysis.Testing.DiagnosticResult;

    /// <summary>
    /// This clas contains the language-agnostic test case code.
    /// </summary>
    public abstract class TestLicenseHeaderMustBeCorrectFormat
    {
        public const string LicenseIdentifierTag = "SPDX-License-Identifier:";
        public const string FileCopyrightTextTag = "SPDX-FileCopyrightText:";

        public const string DefaultLicenseIdentifier = LicenseIdentifierTag + " " + Constants.License.Apache2.SPDX_LicenseIdentifier;
        public const string DefaultFileCopyrightText = FileCopyrightTextTag + " " + Constants.License.Apache2.SPDX_FileCopyrightText;
        public const string DefaultLicenseHeaderText = Constants.License.Apache2.Header;

        public const string DefaultTestFileName = "Test";

        public const IList<DiagnosticResult> NoDiagnosticResults = null;
        public const IList<string> AllDiagnosticsEnabled = null;

        public abstract CodeLanguage Language { get; }

        public abstract FileSystemXml FileSystemXml { get; }



        public static string NormalizePath(string path) => Path.GetFullPath(path);

        public static string FormatMessage(LocalizableString messageFormat, params object[] args)
        {
            if (args is null || args.Length == 0)
            {
                return messageFormat.ToString(CultureInfo.CurrentUICulture);
            }

            // NOTE: Technically, we should be using CurrentCulture here because if there are formatting strings
            // for dates/numbers inside of the localized string it should be using CurrentCulture instead of the
            // CurrentUICulture. This appears to be a bug in Roslyn (TODO: report this upstream). So, to match
            // the broken behavior, we are using CurrentUICulture to format the message, also. This only matters
            // if we put date/number formatting strings in the localized message, though, so as long as we avoid that
            // this is not an issue for us.
            return string.Format(CultureInfo.CurrentUICulture, messageFormat.ToString(CultureInfo.CurrentUICulture), args);
        }

        

        

        //public async Task RunTestAsync(string fileSystemXml, string testCode, string testCodeFilePath)
        //    => await RunTestAsync(fileSystemXml, testCode, testCodeFilePath, enabledDiagnostics: null, expectedDiagnostics: null);

        public async Task RunTestAsync(string fileSystemXml, string testCode, string testCodeFilePath, IList<DiagnosticResult> expectedDiagnostics, bool suppressLocation = false)
            => await RunTestAsync(fileSystemXml, testCode, testCodeFilePath, enabledDiagnostics: null, expectedDiagnostics, suppressLocation);

        public async Task RunTestAsync(string fileSystemXml, string testCode, string testCodeFilePath, IList<string> enabledDiagnostics, IList<DiagnosticResult> expectedDiagnostics, bool suppressLocation = false)
        {
            var test = new LicenseHeaderMustBeCorrectFormatTestDriver(fileSystemXml, Language, suppressLocation)
            {
                TestCode = testCode,
                TestCodeFilePath = testCodeFilePath,
            };

            if (enabledDiagnostics is not null)
            {
                test.EnabledDiagnostics.AddRange(enabledDiagnostics);
            }

            if (expectedDiagnostics is not null)
            {
                test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
            }

            //Console.Write(testCode); // Enable for debugging

            await test.RunAsync();
        }

    }
}
