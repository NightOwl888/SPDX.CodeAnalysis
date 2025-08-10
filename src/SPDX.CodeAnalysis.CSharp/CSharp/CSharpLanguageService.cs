// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace SPDX.CodeAnalysis.CSharp
{
    public class CSharpLanguageService : ILanguageService
    {
        public bool IsKind(SyntaxTrivia trivia, LanguageAgnosticSyntaxKind kind)
        {
            return trivia.IsKind(ToSyntaxKind(kind));
        }

        public bool IsTypeDeclarationSyntax(SyntaxNode? node)
        {
            return node is TypeDeclarationSyntax;
        }

        private SyntaxKind ToSyntaxKind(LanguageAgnosticSyntaxKind kind)
        {
            return kind switch
            {
                LanguageAgnosticSyntaxKind.SingleLineCommentTrivia => SyntaxKind.SingleLineCommentTrivia,
                LanguageAgnosticSyntaxKind.MultiLineCommentTrivia => SyntaxKind.MultiLineCommentTrivia,
                LanguageAgnosticSyntaxKind.WhitespaceTrivia => SyntaxKind.WhitespaceTrivia,
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
        }
    }
}
