using System.Threading.Tasks;
using Chess_Challenge.Cli;
using ChessChallenge.API;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ChessChallenge.Application
{
    static class Program
    {
        public static async Task Main()
        {
            IChessBot player1 = await CSharpScript.EvaluateAsync<IChessBot>("code + return new MyBot();",
                ScriptOptions.Default.WithReferences(typeof(IChessBot).Assembly));
            var uci = new Uci(player1);
            uci.Run();
        }
    }
}