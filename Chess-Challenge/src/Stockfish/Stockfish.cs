using ChessChallenge.API;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;

public class Stockfish : IChessBot
{

    // The name of the stockfish binaries that you downloaded and placed in ...\Chess-Challenge\bin\Debug\net6.0
    const string STOCKFISH_BINARIES = "stockfish";

    // Adjust this so the time it takes to think is adequate
    // Adjusting this also have an effect on how strong it will play
    const string STOCKFISH_DEPTH = "10";

    // Min: 0, Max: 20
    const string STOCKFISH_SKILL_LEVEL = "20";

    // For best performance, set this equal to the number of CPU cores available.
    const string STOCKFISH_THREADS = "6";

    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;

        Process stockfish = new Process();
        stockfish.StartInfo.UseShellExecute = false;
        stockfish.StartInfo.RedirectStandardOutput = true;
        stockfish.StartInfo.RedirectStandardInput = true;
        stockfish.StartInfo.FileName = STOCKFISH_BINARIES;
        stockfish.OutputDataReceived += (sender, args) =>
        {
            /* Example output:
                Stockfish 16 by the Stockfish developers (see AUTHORS file)
                info string NNUE evaluation using nn-5af11540bbfe.nnue enabled
                info depth 1 seldepth 1 multipv 1 score cp 2 nodes 20 nps 20000 hashfull 0 tbhits 0 time 1 pv g1f3
                info depth 2 seldepth 2 multipv 1 score cp 2 nodes 40 nps 40000 hashfull 0 tbhits 0 time 1 pv g1f3
                info depth 3 seldepth 2 multipv 1 score cp 16 nodes 70 nps 70000 hashfull 0 tbhits 0 time 1 pv c2c3
                info depth 4 seldepth 2 multipv 1 score cp 29 nodes 101 nps 101000 hashfull 0 tbhits 0 time 1 pv e2e4
                info depth 5 seldepth 3 multipv 1 score cp 42 nodes 131 nps 131000 hashfull 0 tbhits 0 time 1 pv e2e4 g8f6
                info depth 6 seldepth 4 multipv 1 score cp 59 nodes 489 nps 244500 hashfull 0 tbhits 0 time 2 pv g1f3 d7d5 d2d4
                info depth 7 seldepth 6 multipv 1 score cp 31 nodes 1560 nps 520000 hashfull 1 tbhits 0 time 3 pv e2e4 d7d5 e4d5 d8d5 g1f3
                info depth 8 seldepth 6 multipv 1 score cp 40 nodes 2105 nps 701666 hashfull 1 tbhits 0 time 3 pv e2e4 d7d5 e4d5 d8d5
                info depth 9 seldepth 8 multipv 1 score cp 48 nodes 4500 nps 900000 hashfull 1 tbhits 0 time 5 pv e2e4 e7e5 g1f3 g8f6 f3e5 f6e4 d2d4 b8c6
                info depth 10 seldepth 10 multipv 1 score cp 50 nodes 7548 nps 943500 hashfull 2 tbhits 0 time 8 pv e2e4 e7e5 g1f3 g8f6 b1c3 d7d6 d2d4
                bestmove e2e4 ponder e7e5

            */
            if (args.Data.StartsWith("bestmove")) {
                bestMove = new Move(args.Data.Split(' ')[1], board);
                stockfish.StandardInput.WriteLine("quit");
                stockfish.Close();
            }
        };

        try
        {
            stockfish.Start();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            Console.WriteLine(
                "Unable to find stockfish binaries, expecting a binary file named " 
                + STOCKFISH_BINARIES + " inside of: \n" + Directory.GetCurrentDirectory()
                + "\n\nDownload your stockfish binaries at https://stockfishchess.org/download/"
            );
            Environment.Exit(0);
        }

        stockfish.BeginOutputReadLine();

        stockfish.StandardInput.WriteLine("setoption name Threads value " + STOCKFISH_THREADS);
        stockfish.StandardInput.WriteLine("setoption name Skill Level value " + STOCKFISH_SKILL_LEVEL);
        stockfish.StandardInput.WriteLine("position fen " + board.GetFenString());
        stockfish.StandardInput.WriteLine("go depth " + STOCKFISH_DEPTH);

        stockfish.WaitForExit();
        
        return bestMove;
    }
}