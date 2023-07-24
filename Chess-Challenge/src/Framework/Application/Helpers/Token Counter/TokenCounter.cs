using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace ChessChallenge.Application
{
    public class TokenCount {
        public int total;
        public int excludingLogs;
    }
    public static class TokenCounter
    {

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

        public static TokenCount CountTokens(string code)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            SyntaxNode root = tree.GetRoot();
            return CountTokens(root);
        }

        static TokenCount CountTokens(SyntaxNodeOrToken syntaxNode)
        {
            SyntaxKind kind = syntaxNode.Kind();
            int numTokensInChildren = 0;
            int numTokensExcludingLogs = 0;


            foreach (var child in syntaxNode.ChildNodesAndTokens())
            {
                if (!(child.ToString().StartsWith("System.Console.Write") || child.ToString().StartsWith("Console.Write"))) {
                    numTokensExcludingLogs += CountTokens(child).excludingLogs;
                }
                numTokensInChildren += CountTokens(child).total;
            }

            if (syntaxNode.IsToken && !tokensToIgnore.Contains(kind))
            {
                //Console.WriteLine(kind + "  " + syntaxNode.ToString());

                // String literals count for as many chars as are in the string
                if (kind is SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringTextToken)
                {
                    int length = syntaxNode.ToString().Length;
                    return new TokenCount{total = length, excludingLogs = length};
                }

                // Regular tokens count as just one token
                return new TokenCount{total = 1, excludingLogs = 1};
            }

            return new TokenCount{total = numTokensInChildren, excludingLogs = numTokensExcludingLogs};
        }

    }
}