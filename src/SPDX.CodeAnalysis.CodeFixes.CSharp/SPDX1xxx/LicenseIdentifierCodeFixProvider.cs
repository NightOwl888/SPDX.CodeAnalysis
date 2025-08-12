// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LicenseIdentifierCodeFixProvider)), Shared]
    public class LicenseIdentifierCodeFixProvider : CodeFixProvider
    {
        private const string LicenseIdentifierTag = "SPDX-License-Identifier";
        private const string TopLevelDirectoryName = "LICENSES.HEADERS"; // TODO: Make configurable?

        private static readonly ImmutableArray<string> fixableDiagnosticIds = ImmutableArray.Create(
            Descriptors.SPDX1000_LicenseIdentifierMustExist.Id
        );

        public override ImmutableArray<string> FixableDiagnosticIds => fixableDiagnosticIds;

        // Dependency injection
        private readonly ILicenseHeaderCacheLifetimeManager cacheLifetimeManager;

        public LicenseIdentifierCodeFixProvider()
            : this(LicenseHeaderCacheProvider.Instance)
        {
        }

        internal LicenseIdentifierCodeFixProvider(ILicenseHeaderCacheLifetimeManager cacheLifetimeManager)
        {
            this.cacheLifetimeManager = cacheLifetimeManager ?? throw new ArgumentNullException(nameof(cacheLifetimeManager));
        }

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null)
                return;

            var additionalFiles = document.Project.AnalyzerOptions.AdditionalFiles;
            var cache = cacheLifetimeManager.GetCache(additionalFiles, TopLevelDirectoryName, cancellationToken);
            var spdxLicenseIdentifiers = cache.GetAllSpdxLicenseIdentifiers();

            foreach (var spdxLicenseIdentifier in spdxLicenseIdentifiers)
            {
                var title = string.Format(CodeFixResources.SPDX1000_CodeFixTitle, $"{LicenseIdentifierTag}: {spdxLicenseIdentifier}");
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => InsertLicenseCommentAsync(document, root, spdxLicenseIdentifier, diagnostic.Location, c),
                        equivalenceKey: title),
                    diagnostic);
            }
        }

        private async Task<Document> InsertLicenseCommentAsync(Document document, SyntaxNode root, string licenseId, Location diagnosticLocation, CancellationToken cancellationToken)
        {
            var position = diagnosticLocation.SourceSpan.Start;
            var length = diagnosticLocation.SourceSpan.Length;

            // If zero-length span, insert before first using or fallback to start of file
            bool zeroLength = length == 0;

            var tree = await document.GetSyntaxTreeAsync(cancellationToken);
            if (tree is null)
                return document;

            var config = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree);

            config.TryGetValue("dotnet_diagnostic.spdx_license_identifier_position", out string? positionSetting);
            config.TryGetValue("dotnet_diagnostic.spdx_license_identifier_comment_style", out string? commentStyleSetting);

            //var position = (positionSetting ?? "before_usings").ToLowerInvariant();
            var commentStyle = (commentStyleSetting ?? "single_line").ToLowerInvariant();

            var indentation = GetIndentation(document, cancellationToken) ?? "    "; // Default: 4 spaces

            var options = await document.GetOptionsAsync(cancellationToken);
            var newLine = options.GetOption(FormattingOptions.NewLine, LanguageNames.CSharp)
                         ?? Environment.NewLine;

            // Build comment text
            string comment = commentStyle switch
            {
                "multi_line" => BuildMultiLineComment(new[] { $"SPDX-License-Identifier: {licenseId}" }, newLine, indentation),
                _ => $"// SPDX-License-Identifier: {licenseId}"
            };

            var trivia = SyntaxFactory.ParseLeadingTrivia(comment + "\n\n");

            //SyntaxNode newRoot = position switch
            //{
            //    "before_namespace" => InsertBeforeFirstNodeOfType<NamespaceDeclarationSyntax>(root, trivia, newLine),
            //    "before_type" => InsertBeforeFirstNodeOfType<TypeDeclarationSyntax>(root, trivia, newLine, indentation),
            //    _ => InsertBeforeUsings(root, trivia)
            //};

            SyntaxNode newRoot;

            if (zeroLength)
            {
                // Insert at a default position — e.g., before usings
                newRoot = InsertBeforeUsings(root, trivia);
            }
            else
            {
                // If span covers some syntax, insert before that node
                // Example: find the token at the diagnostic span start and insert before it
                var token = root.FindToken(position);
                var node = token.Parent;

                // Insert before this node with trivia
                newRoot = node != null
                    ? node.WithLeadingTrivia(trivia.AddRange(node.GetLeadingTrivia()))
                    : InsertBeforeUsings(root, trivia); // fallback
            }

            return document.WithSyntaxRoot(newRoot);
        }


        //private async Task<Document> InsertLicenseCommentAsync(Document document, SyntaxNode root, string licenseId, Location diagnosticLocation, CancellationToken cancellationToken)
        //{
        //    var tree = await document.GetSyntaxTreeAsync(cancellationToken);
        //    var config = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree);

        //    config.TryGetValue("dotnet_diagnostic.spdx_license_identifier_position", out string? positionSetting);
        //    config.TryGetValue("dotnet_diagnostic.spdx_license_identifier_comment_style", out string? commentStyleSetting);

        //    var position = (positionSetting ?? "before_usings").ToLowerInvariant();
        //    var commentStyle = (commentStyleSetting ?? "single_line").ToLowerInvariant();

        //    var indentation = GetIndentation(document, cancellationToken) ?? "    "; // Default: 4 spaces

        //    var options = await document.GetOptionsAsync(cancellationToken);
        //    var newLine = options.GetOption(FormattingOptions.NewLine, LanguageNames.CSharp)
        //                 ?? Environment.NewLine;

        //    // Build comment text
        //    string comment = commentStyle switch
        //    {
        //        "multi_line" => BuildMultiLineComment(new[] { $"SPDX-License-Identifier: {licenseId}" }, newLine, indentation),
        //        _ => $"// SPDX-License-Identifier: {licenseId}"
        //    };

        //    var trivia = SyntaxFactory.ParseLeadingTrivia(comment + "\n\n");

        //    SyntaxNode newRoot = position switch
        //    {
        //        "before_namespace" => InsertBeforeFirstNodeOfType<NamespaceDeclarationSyntax>(root, trivia, newLine),
        //        "before_type" => InsertBeforeFirstNodeOfType<TypeDeclarationSyntax>(root, trivia, newLine, indentation),
        //        _ => InsertBeforeUsings(root, trivia)
        //    };

        //    return document.WithSyntaxRoot(newRoot);
        //}

        private static string BuildMultiLineComment(IEnumerable<string> lines, string newLine, string indent)
        {
            var formattedLines = lines.Select(l => $"{indent} *  {l}");
            return $"/*{newLine}{string.Join(newLine, formattedLines)}{newLine}{indent} */";
        }

        private static string? GetIndentation(Document document, CancellationToken cancellationToken)
        {
            var workspace = document.Project.Solution.Workspace;
            var options = workspace.Options;

            var useTabs = options.GetOption(FormattingOptions.UseTabs, LanguageNames.CSharp);
            var tabSize = options.GetOption(FormattingOptions.TabSize, LanguageNames.CSharp);

            return useTabs ? "\t" : new string(' ', tabSize);
        }

        private static SyntaxNode InsertBeforeUsings(SyntaxNode root, SyntaxTriviaList trivia)
        {
            var compilationUnit = root as CompilationUnitSyntax;

            var firstUsing = compilationUnit?.Usings.FirstOrDefault();
            var insertBefore = (SyntaxNode?)firstUsing ?? compilationUnit?.Members.FirstOrDefault();

            if (insertBefore != null)
            {
                var newNode = insertBefore.WithLeadingTrivia(trivia.AddRange(insertBefore.GetLeadingTrivia()));
                return root.ReplaceNode(insertBefore, newNode);
            }

            return root; // fallback
        }

        private static SyntaxNode InsertBeforeFirstNodeOfType<T>(SyntaxNode root, SyntaxTriviaList trivia, string newLine, string? indent = null) where T : SyntaxNode
        {
            var node = root.DescendantNodes().OfType<T>().FirstOrDefault();
            if (node is null)
                return root;

            var currentTrivia = node.GetLeadingTrivia();

            // Adjust indentation if needed (e.g. before type)
            if (!string.IsNullOrEmpty(indent))
            {
                trivia = AdjustTriviaIndentation(trivia, newLine, indent!);
            }

            var newNode = node.WithLeadingTrivia(trivia.AddRange(currentTrivia));
            return root.ReplaceNode(node, newNode);
        }

        private static SyntaxTriviaList AdjustTriviaIndentation(SyntaxTriviaList trivia, string newLine, string indent)
        {
            var adjustedTrivia = trivia.Select(t =>
            {
                if (t.RawKind == (int)SyntaxKind.MultiLineCommentTrivia || t.RawKind == (int)SyntaxKind.SingleLineCommentTrivia)
                {
                    var lines = t.ToFullString().Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
                    var reindented = lines.Select(line => indent + line.TrimStart());
                    return SyntaxFactory.Comment(string.Join(newLine, reindented));
                }
                return t;
            });

            return SyntaxFactory.TriviaList(adjustedTrivia);
        }
    }
}
