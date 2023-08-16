using System;
using ChessChallenge.API;
using Stockfish.NET;

public class StockfishBot : IChessBot
{
    const int STOCKFISH_LEVEL = 0;
    IStockfish mStockFish;

    public StockfishBot()
    {
        // Create Stockfish.NET.Models.Settings object and set the SkillLevel option
        Stockfish.NET.Models.Settings stockfishSettings = new Stockfish.NET.Models.Settings();
        stockfishSettings.SkillLevel = STOCKFISH_LEVEL;

        // Use the stockfishSettings object when creating the Stockfish instance
        mStockFish = new Stockfish.NET.Stockfish(@"resources\stockfish-16.exe", 1, stockfishSettings);
        // The second argument (1 in this case) represents the depth.
        // Adjust this value depending on the desired search depth.
    }

    public Move Think(Board board, Timer timer)
    {
        string fen = board.GetFenString();
        mStockFish.SetFenPosition(fen);

        string bestMove = mStockFish.GetBestMoveTime(GetTime(board, timer));

        return new Move(bestMove, board);
    }

    // Basic time management
    public int GetTime(Board board, Timer timer)
    {
        return Math.Min(board.PlyCount * 150 + 100, timer.MillisecondsRemaining / 20);
    }
}
