// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections.Immutable;

namespace SPDX.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LicenseHeaderMustBeCorrectFormat : DiagnosticAnalyzer
    {
        private const string LicenseIdentifierToken = "SPDX-License-Identifier:";
        private const string LicenseCopyrightToken = "SPDX-FileCopyrightText:";
        private static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray.Create<DiagnosticDescriptor>(
            Descriptors.SPDX_1000_LicenseIdentifierMustExist,
            Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue,
            Descriptors.SPDX_1002_FileCopyrightTextMustExist,
            Descriptors.SPDX_1003_FileCopyrightTextMustHaveValue,
            Descriptors.SPDX_1004_LicenseCopyrightTextMustPrecedeLicenseIdentifier,
            Descriptors.SPDX_1005_LicenseTextMustExist
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => supportedDiagnostics;

        // Dependency injection
        private readonly ILicenseHeaderProvider provider;
        private readonly LicenseAnalyzerOptions options;

        public LicenseHeaderMustBeCorrectFormat()
            : this(LicenseHeaderProviderLoader.GetProvider(), null)
        {
        }

        // Dependency injection constructor (for testing)
        internal LicenseHeaderMustBeCorrectFormat(ILicenseHeaderProvider provider, LicenseAnalyzerOptions? options)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.options = options ?? new LicenseAnalyzerOptions();
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken);
            //var firstToken = root.GetFirstToken(includeZeroWidth: true);

            //var triviaList = firstToken.LeadingTrivia;
            string? codeFilePath = context.Tree.FilePath;


            string? copyrightContainingText = null; // Used to keep the text in scope while slicing it.
            string? licenseIdContainingText = null; // Used to keep the text in scope while slicing it.

            TextSpan? copyrightValueLocation = null;
            TextSpan? licenseIdValueLocation = null;

            // Used to store the start/length of the current line (relative to the beginnning of the containing text rather than relative to the file start)
            TextSpan? copyrightValueSpan = null;
            TextSpan? licenseIdValueSpan = null;


            //ReadOnlySpan<char> copyrightValueSpan = default;
            //ReadOnlySpan<char> licenseIdValueSpan = default;
            //ReadOnlySpan<char> licenseTextSpan = default;

            //int? copyrightIndex = null;
            //int? licenseIdIndex = null;

            bool hasLicenseId = false;
            bool hasCopyright = false;
            bool hasLicenseText = false;
            bool hasLicenseTextPartial = false;

            bool hasLicenseIdValue = true;
            bool hasCopyrightValue = true;
            

            //bool insideNamespace = false;


            foreach (var token in root.DescendantTokens())
            {
                //// Track whether we're inside a namespace block (optional)
                //if (!insideNamespace && token.Parent is BaseNamespaceDeclarationSyntax)
                //    insideNamespace = true;

                // Process leading trivia for each token
                if (ProcessSpdxTag(token.LeadingTrivia))
                    break;

                //// Process trailing trivia for each token
                //if (ProcessTriviaList(token.TrailingTrivia))
                //    break;

                // Stop scanning once we hit the first type declaration
                if (token.Parent is TypeDeclarationSyntax)
                    break;
            }

            // Second pass - If we found the SPDX tags, then look for the correct license header text.
            // This is configured as a file in a LICENSES.HEADERS directory which may be levels above
            // the current directory.
            if (hasLicenseId && hasLicenseIdValue)
            {
                //ILicenseHeaderProvider provider = LicenseHeaderProviderLoader.GetProvider();

                string licenseHeaderDirectory = "LICENSES.HEADERS"; // TODO: Get from configuration setting, default to this value
                if (!licenseIdValueSpan.HasValue)
                {
                    // TODO: We should report this somehow, but this is a bug with our analyzer, not something the user did.
                }

                ReadOnlySpan<char> spdxLicenseIdentifier = licenseIdContainingText.AsSpan(licenseIdValueSpan!.Value.Start, licenseIdValueSpan.Value.Length).Trim();

                string codeFileDirectory = Path.GetDirectoryName(codeFilePath);

                if (!provider.TryGetLicenseHeader(codeFileDirectory, licenseHeaderDirectory, spdxLicenseIdentifier, out IReadOnlyList<IReadOnlyList<string>> result))
                {
                    hasLicenseText = false;
                }

                hasLicenseText = true;

                // TODO: Re-implement this so we have reporting on license header matching
                //if (!provider.TryGetLicenseHeader(directory, spdxLicenseIdentifier, out IReadOnlyList<string> licenseTextLines))
                //{
                //    // TODO: Report this as an unconfigured license header. For now, we have no fallback values.
                //}

                //foreach (var token in root.DescendantTokens())
                //{
                //    if (ProcessLicenseText(licenseTextLines, token.LeadingTrivia))
                //        break;

                //    // Stop scanning once we hit the first type declaration
                //    if (token.Parent is TypeDeclarationSyntax)
                //        break;
                //}
            }


            void ProcessSpdxTagLine(SyntaxTrivia trivia, string text, ReadOnlySpan<char> line, int separatorOffset = 0)
            {
                ReadOnlySpan<char> licenseIdentifier = LicenseIdentifierToken.AsSpan();
                ReadOnlySpan<char> licenseCopyrightText = LicenseCopyrightToken.AsSpan();

                int offset = line.IndexOf(licenseIdentifier, StringComparison.Ordinal);
                if (offset > -1)
                {
                    hasLicenseId = true;
                    int tokenLength = licenseIdentifier.Length;
                    int lineStart = offset + tokenLength;
                    int lineLength = line.Length - lineStart;
                    int start = trivia.SpanStart + offset + separatorOffset + tokenLength;
                    int length = Math.Max(line.Length - (offset + separatorOffset + tokenLength), 0);
                    licenseIdValueLocation = new TextSpan(start, length);
                    licenseIdContainingText = text; // Used to keep the text in scope while slicing it.
                    //licenseIdSpan = licenseIdContainingText.AsSpan(start, length); // TODO: Figure out how to access this text later
                    licenseIdValueSpan = new TextSpan(lineStart, lineLength);
                    hasLicenseIdValue = !licenseIdContainingText.AsSpan(lineStart, lineLength).Trim().IsEmpty;
                }
                offset = line.IndexOf(licenseCopyrightText, StringComparison.Ordinal);
                if (offset > -1)
                {
                    hasCopyright = true;
                    int tokenLength = licenseCopyrightText.Length;
                    int lineStart = offset + tokenLength;
                    int lineLength = line.Length - lineStart;
                    int start = trivia.SpanStart + offset + separatorOffset + tokenLength;
                    int length = Math.Max(line.Length - (offset + separatorOffset + tokenLength), 0);
                    copyrightValueLocation = new TextSpan(start, length);
                    copyrightContainingText = text; // Used to keep the text in scope while slicing it.
                    //copyrightSpan = copyrightContainingText.AsSpan(start, length); // TODO: Figure out how to access this text later
                    copyrightValueSpan = new TextSpan(lineStart, lineLength);
                    hasCopyrightValue = !copyrightContainingText.AsSpan(lineStart, lineLength).Trim().IsEmpty;
                }
            }

            bool ProcessSpdxTag(SyntaxTriviaList triviaList)
            {
                foreach (var trivia in triviaList)
                {
                    if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                    {
                        string text = trivia.ToString(); // safe to reference span from here
                        ProcessSpdxTagLine(trivia, text, text.AsSpan());

                        //ReadOnlySpan<char> comment = text.AsSpan();

                        //int offset = comment.IndexOf(licenseIdentifier, StringComparison.Ordinal);
                        //if (offset > -1)
                        //{
                        //    hasLicenseId = true;
                        //    int tokenLength = licenseIdentifier.Length;
                        //    int start = trivia.SpanStart + offset + tokenLength;
                        //    int length = Math.Max(comment.Length - (offset + tokenLength), 0);
                        //    licenseIdSpan = new TextSpan(start, length);
                        //    licenseIdContainingText = text; // Used to keep the text in scope while slicing it.
                        //    //licenseIdSpan = licenseIdContainingText.AsSpan(start, length); // TODO: Figure out how to access this text later
                        //    hasLicenseIdValue = !licenseIdContainingText.AsSpan(start, length).Trim().IsEmpty;
                        //}
                        //offset = comment.IndexOf(licenseCopyrightText, StringComparison.Ordinal);
                        //if (offset > -1)
                        //{
                        //    hasCopyright = true;
                        //    int tokenLength = licenseCopyrightText.Length;
                        //    int start = trivia.SpanStart + offset + tokenLength;
                        //    int length = Math.Max(comment.Length - (offset + tokenLength), 0);
                        //    copyrightSpan = new TextSpan(start, length);
                        //    copyrightContainingText = text; // Used to keep the text in scope while slicing it.
                        //    //copyrightSpan = copyrightContainingText.AsSpan(start, length); // TODO: Figure out how to access this text later
                        //}

                        //if (MatchesConfiguredLicenseText(context, comment))
                        //{
                        //    hasLicenseText = true;
                        //}

                        //var commentText = trivia.ToString().TrimStart('/', ' ').Trim();

                        ////trivia.GetLocation()

                        //if (commentText.StartsWith("SPDX-License-Identifier:"))
                        //    hasLicenseId = true;

                        //if (commentText.StartsWith("SPDX-FileCopyrightText:"))
                        //    hasCopyright = true;
                    }
                    else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                    {
                        string text = trivia.ToString(); // safe to reference span from here
                        //ReadOnlySpan<char> comment = text.AsSpan();

                        int separatorOffset = 0;
                        foreach (var line in text.SplitLines())
                        {
                            ProcessSpdxTagLine(trivia, text, line, separatorOffset);

                            // Stop scanning if found
                            if (hasLicenseId && hasCopyright)
                                return true;

                            separatorOffset += line.Separator.Length;
                        }

                        //var lines = trivia.ToString().Split('\n');
                        //foreach (var line in lines)
                        //{
                        //    var trimmed = line.Trim('/', '*', ' ');
                        //    if (trimmed.StartsWith("SPDX-License-Identifier:"))
                        //        hasLicenseId = true;
                        //    if (trimmed.StartsWith("SPDX-FileCopyrightText:"))
                        //        hasCopyright = true;
                        //}
                    }

                    // Stop scanning if found
                    if (hasLicenseId && hasCopyright)
                        return true;
                }
                return false;
            }

            //bool ProcessLicenseTextLine(IReadOnlyList<string> licenseTextLines, ref int licenseTextLinesIndex, SyntaxTrivia trivia, string text, ReadOnlySpan<char> line, int separatorOffset = 0)
            //{
            //    ReadOnlySpan<char> licenseLine = licenseTextLines[licenseTextLinesIndex].AsSpan();

            //    int offset = line.IndexOf(licenseLine, StringComparison.Ordinal);
            //    if (offset > -1)
            //    {
            //        licenseTextLinesIndex++;
            //        if (licenseTextLinesIndex == licenseTextLines.Count)
            //            return true;
            //    }

            //    return false;
            //}

            bool ProcessLicenseText(IReadOnlyList<string> licenseTextLines, SyntaxTriviaList triviaList)
            {
                var matchSession = new LicenseHeaderMatchSession(licenseTextLines);

                // Keep track of the line to match in licenseTextLines
                //int licenseTextLinesIndex = 0;

                foreach (var trivia in triviaList)
                {
                    if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                    {
                        string text = trivia.ToString(); // safe to reference span from here
                        if (matchSession.MatchNextLine(text.AsSpan()))
                            hasLicenseText = true;

                        //ProcessLicenseTextLine(licenseTextLines, ref licenseTextLinesIndex, trivia, text, text.AsSpan());
                    }
                    else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                    {
                        string text = trivia.ToString(); // safe to reference span from here

                        //int separatorOffset = 0;
                        foreach (var line in text.SplitLines())
                        {
                            //ProcessLicenseTextLine(licenseTextLines, ref licenseTextLinesIndex, trivia, text, line, separatorOffset);
                            if (matchSession.MatchNextLine(line))
                            {
                                hasLicenseText = true;
                                break;
                            }


                            //// Stop scanning if found
                            //if (hasLicenseText)
                            //    return true;

                            //separatorOffset += line.Separator.Length;
                        }
                    }

                    // Stop scanning if found
                    if (hasLicenseText)
                        break;
                }

                if (matchSession.IsComplete)
                {
                    hasLicenseTextPartial = false;
                }
                else if (matchSession.HasMatchedAny)
                {
                    hasLicenseTextPartial = true;
                }
                return false;
            }

            if (!hasLicenseId)
            {
                ReportHasNoLicenseIdentifier(context);
            }
            else if (!hasLicenseIdValue)
            {
                ReportHasNoLicenseIdentifierValue(context, licenseIdValueSpan!.Value);
            }

            if (!hasCopyright)
            {
                ReportHasNoFileCopyrightText(context);
            }
            else if (!hasCopyrightValue)
            {
                ReportHasNoFileCopyrightTextValue(context, copyrightValueSpan!.Value);
            }

            if (!hasLicenseText)
            {
                ReportHasNoLicenseText(context);
            }
        }

        

        private bool MatchesConfiguredLicenseText(SyntaxTreeAnalysisContext context, ReadOnlySpan<char> text)
        {
            //var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
            //config.TryGetValue("dotnet_diagnostic.MyRules0001.use_multiple_namespaces_in_a_file", out var configValue);


            // TODO: Make this configurable with fallback to the correct license
            var configuredLicenseLines = new string[]
            {
                "Licensed under the Apache License, Version 2.0",
                "http://www.apache.org/licenses/LICENSE-2.0"
            };

            foreach (string expected in  configuredLicenseLines)
            {
                if (text.Contains(expected.AsSpan(), StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        //private string? GetProjectDirectory(AnalyzerOptions options)
        //{
        //    foreach (var file in options.AdditionalFiles)
        //    {
        //        if (options.AnalyzerConfigOptionsProvider.GetOptions(file)
        //            .TryGetValue("build_property.MSBuildProjectDirectory", out var projectDir))
        //        {
        //            return projectDir;
        //        }
        //    }
        //    return null;
        //}

        //private static string? FindLicenseHeadersDirectory(string startDirectory)
        //{
        //    var dir = new DirectoryInfo(startDirectory);

        //    while (dir != null)
        //    {
        //        var candidate = Path.Combine(dir.FullName, "LICENSES.HEADERS");
        //        if (Directory.Exists(candidate))
        //            return candidate;

        //        dir = dir.Parent;
        //    }

        //    return null; // not found
        //}



        private static void ValidateLicenseIdentifierValue(SyntaxTreeAnalysisContext context, ReadOnlySpan<char> span)
        {

        }

        private void ReportDiagnostic(SyntaxTreeAnalysisContext context, DiagnosticDescriptor descriptor, TextSpan span)
        {
            var location = options.SuppressLocation
                ? Location.None
                : Location.Create(context.Tree, span);

            var diagnostic = Diagnostic.Create(descriptor, location);
            context.ReportDiagnostic(diagnostic);
        }

        private void ReportHasNoLicenseIdentifier(SyntaxTreeAnalysisContext context)
            => ReportDiagnostic(context, Descriptors.SPDX_1000_LicenseIdentifierMustExist, new TextSpan(0, 0));

        private void ReportHasNoLicenseIdentifierValue(SyntaxTreeAnalysisContext context, TextSpan span)
            => ReportDiagnostic(context, Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue, span);

        private void ReportHasNoFileCopyrightText(SyntaxTreeAnalysisContext context)
            => ReportDiagnostic(context, Descriptors.SPDX_1002_FileCopyrightTextMustExist, new TextSpan(0, 0));

        private void ReportHasNoFileCopyrightTextValue(SyntaxTreeAnalysisContext context, TextSpan span)
            => ReportDiagnostic(context, Descriptors.SPDX_1003_FileCopyrightTextMustHaveValue, span);

        private void ReportHasNoLicenseText(SyntaxTreeAnalysisContext context)
            => ReportDiagnostic(context, Descriptors.SPDX_1005_LicenseTextMustExist, new TextSpan(0, 0));
    }
}
