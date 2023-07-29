using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ChessChallenge.Application
{
    public static class TokenCounter
    {
        // Including this in a comment will cause all tokens on the same line to be excluded from the count.
        // Note: these tokens will still be counted in your final submission. 
        const string ignoreString = "#DEBUG";

        static readonly HashSet<SyntaxKind> tokensToIgnore = new(new SyntaxKind[]
        {
            SyntaxKind.PrivateKeyword,
            SyntaxKind.PublicKeyword,
            SyntaxKind.SemicolonToken,
            SyntaxKind.CommaToken,
            SyntaxKind.ReadOnlyKeyword,
            // only count open brace since I want to count the pair as a single token
            SyntaxKind.CloseBraceToken, 
            SyntaxKind.CloseBracketToken,
            SyntaxKind.CloseParenToken
        });

        public static (int totalCount, int debugCount) CountTokens(string code)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            SyntaxNode root = tree.GetRoot();
            TextLineCollection lines = tree.GetText().Lines;
            IEnumerable<SyntaxToken> allTokens = root.DescendantTokens();

            // Total token count
            int totalTokenCount = CountTokens(allTokens);

            // Debug token count (tokens explicitly excluded by user for testing purposes)
            var ignoreInstructions = root.DescendantTrivia().Where(t => IsIgnoreTokenInstruction(t));
            int debugTokenCount = 0;
            foreach(var ignore in ignoreInstructions)
            {
                int lineNumber = lines.GetLineFromPosition(ignore.SpanStart).LineNumber;
                var debugTokens = allTokens.Where(t => lines.GetLineFromPosition(t.SpanStart).LineNumber == lineNumber);
                debugTokenCount += CountTokens(debugTokens);
            }

            return (totalTokenCount, debugTokenCount);
        }

        static int CountTokens(IEnumerable<SyntaxToken> tokens)
        {
            int tokenCount = 0;
            foreach (var token in tokens)
            {
                tokenCount += GetTokenCountValue(token);
            }
            return tokenCount;
        }

        static int GetTokenCountValue(SyntaxToken token)
        {
            SyntaxKind kind = token.Kind();
            if (tokensToIgnore.Contains(kind))
            {
                return 0;
            }

            // String literals count for as many chars as are in the string
            if (kind is SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringTextToken)
            {
                return token.ToString().Length;
            }

            // Regular tokens count as just one token
            return 1;
        }

        static bool IsIgnoreTokenInstruction(SyntaxTrivia trivia)
        {
            var compareType = StringComparison.InvariantCultureIgnoreCase;
            return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.ToString().Contains(ignoreString, compareType);
        }
    }
}