// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using System;

namespace SPDX.CodeAnalysis.Languages
{
    public sealed class CSharpLanguageService : ILanguageService
    {
        private const int SingleLineCommentTriviaKind = 8541; // (int)SyntaxKind.SingleLineCommentTrivia;
        private const int MultiLineCommentTriviaKind = 8542; //(int)SyntaxKind.MultiLineCommentTrivia;
        private const int WhitespaceTriviaKind = 8540; //(int)SyntaxKind.WhitespaceTrivia;

        private readonly Type typeSyntaxType;

        private CSharpLanguageService()
        {
            typeSyntaxType = Type.GetType("Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax", throwOnError: false);
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
        {
            if (node is null) return false;
            if (typeSyntaxType is null) return false;

            return typeSyntaxType.IsInstanceOfType(node);
        }
    }
}
