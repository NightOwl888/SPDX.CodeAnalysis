// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using System;

namespace SPDX.CodeAnalysis.Languages
{
    public sealed class VisualBasicLanguageService : ILanguageService
    {
        private const int SingleLineCommentTriviaKind = 732; // (int)SyntaxKind.CommentTrivia;
        private const int MultiLineCommentTriviaKind = -1; // VB doesn't have multiline comments;
        private const int WhitespaceTriviaKind = 729; //(int)SyntaxKind.WhitespaceTrivia;

        private readonly Type typeSyntaxType;

        private VisualBasicLanguageService()
        {
            typeSyntaxType = Type.GetType("Microsoft.CodeAnalysis.VisualBasic.Syntax.TypeBlockSyntax", throwOnError: false);
        }

        public static ILanguageService Instance => new VisualBasicLanguageService();

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
