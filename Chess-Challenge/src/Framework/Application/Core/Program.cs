using System.Threading.Tasks;
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
            IChessBot player2 = await CSharpScript.EvaluateAsync<IChessBot>("code + return new MyBot();",
                ScriptOptions.Default.WithReferences(typeof(IChessBot).Assembly));
            Controller controller = new(player1, player2);
            controller.StartNewGame(); 
        }
    }
}