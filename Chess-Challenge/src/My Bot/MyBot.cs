using ChessChallenge.API;
using System;
using System.Linq;
using System.Data;
using static ChessChallenge.Application.ConsoleHelper;

public class MyBot : IChessBot
{
    //                                    .  P   K   B   R   Q   K
    private static int[] PIECE_VALUES = { 0, 10, 30, 30, 50, 90, 2000 };
    private static int WORST_SCORE = -Int32.MaxValue;
    /// <summary>the depth to which the bot searches</summary>
    private static int DEPTH = 3;
    private Move bestMove = Move.NullMove;

    public Move Think(Board board, Timer timer)
    {
        Search(board, DEPTH);
        return bestMove;
    }

    /// <summary>
    /// Search is a recursive function that searches for the best move at a given depth.
    /// </summary>
    /// <param name="board">current board</param>
    /// <param name="depth">current search depth</param>
    /// <remarks>the depth is decreased by 1 for each recursive call</remarks>
    /// <param name="maxDepth">maximal depth to be searched + the depth at which the boards are ultimately evaluated</param>
    /// <returns>score of the board at depth=0 with the best score obtained</returns>
    double Search(Board board, int depth)
    {
        // we have reached the depth - evaluate the board for the current color
        if (depth == 0)
            return Evaluate(board);

        double bestScore = WORST_SCORE;
        Move[] moves = board.GetLegalMoves();
        if (depth == DEPTH)
            bestMove = moves[0];
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            // negate the score because after making a move,
            // we are looking at the board from the other player's perspective
            double score = -Search(board, depth - 1);
            if (score > bestScore)
            {
                bestScore = score;
                if (depth == DEPTH)
                    this.bestMove = move;
            }
            board.UndoMove(move);
        }
        return bestScore;
    }

    /// <summary>
    /// Evaluate evaluates a board and returns a score.
    /// The higher the score, the better the board is for the current color
    /// </summary>
    /// <param name="board">board to be evaluated</param>
    /// <returns>score of the board</returns>
    private double Evaluate(Board board)
    {
        if (board.IsInCheckmate()) return WORST_SCORE;
        if (board.IsInCheck()) return WORST_SCORE / 2;
        if (board.IsRepeatedPosition()) return WORST_SCORE / 4;
        if (board.IsDraw()) return 0;

        int myMobility = board.GetLegalMoves().Length;
        board.TrySkipTurn(); // we already checked for IsInCheck above
        int theirMobility = board.GetLegalMoves().Length;
        board.UndoSkipTurn();

        double score = board.GetLegalMoves().Length;
        double[] pieceScores = { 0, 0, 0, 0, 0, 0, 0 };
        foreach (PieceList list in board.GetAllPieceLists())
        {
            pieceScores[(int)list.TypeOfPieceInList] += list.Count * (list.IsWhitePieceList == board.IsWhiteToMove ? 1 : -1);
        }
        double pieceScore = pieceScores.Select((pieceScore, index) => pieceScore * PIECE_VALUES[index]).Sum();
        return (myMobility - theirMobility) + pieceScore;
    }
}
