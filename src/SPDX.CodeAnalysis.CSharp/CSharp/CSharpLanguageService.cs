// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SPDX.CodeAnalysis.CSharp
{
    public sealed class CSharpLanguageService : ILanguageService
    {
        private const int SingleLineCommentTriviaKind = (int)SyntaxKind.SingleLineCommentTrivia;
        private const int MultiLineCommentTriviaKind = (int)SyntaxKind.MultiLineCommentTrivia;
        private const int WhitespaceTriviaKind = (int)SyntaxKind.WhitespaceTrivia;

        private CSharpLanguageService()
        {
        }

        public static ILanguageService Instance => new CSharpLanguageService();

        public bool IsKind(SyntaxTrivia trivia, LanguageAgnosticSyntaxKind kind)
        {
            int rawKind = trivia.RawKind;
            if (kind == LanguageAgnosticSyntaxKind.SingleLineCommentTrivia)
                return rawKind == SingleLineCommentTriviaKind;
            else if (kind == LanguageAgnosticSyntaxKind.MultiLineCommentTrivia)
                return rawKind == MultiLineCommentTriviaKind;
            else if (kind == LanguageAgnosticSyntaxKind.WhitespaceTrivia)
                return rawKind == WhitespaceTriviaKind;

            return false;
        }

        public bool IsTypeDeclarationSyntax(SyntaxNode? node)
            => node is TypeDeclarationSyntax;
    }
}
