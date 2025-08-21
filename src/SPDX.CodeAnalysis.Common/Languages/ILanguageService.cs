// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;

namespace SPDX.CodeAnalysis.Languages
{
    public interface ILanguageService
    {
        bool IsKind(SyntaxTrivia trivia, LanguageAgnosticSyntaxKind kind);
        bool IsTypeDeclarationSyntax(SyntaxNode? node);
    }
}
