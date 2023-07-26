using ChessChallenge.API;
using System;
using System.Linq;
using System.Data;
using static ChessChallenge.Application.ConsoleHelper;

public class MyBot : IChessBot
{
    //                             .  P   K   B   R   Q   K
    private static int[] PIECE_VALUES = { 0, 10, 30, 30, 50, 90, 2000 };
    private static int MAX_DEPTH = 3;
    private Move bestMove = Move.NullMove;

    public Move Think(Board board, Timer timer)
    {
        Search(board, 0, MAX_DEPTH);
        return bestMove;
    }

    /// <summary>
    /// Search is a recursive function that searches for the best move at a given depth.
    /// </summary>
    /// <param name="board">current board</param>
    /// <param name="depth">current search depth</param>
    /// <param name="maxDepth">maximal depth to be searched + the depth at which the boards are ultimately evaluated</param>
    /// <returns>score of the board at depth=maxdepth with the best score obtained</returns>
    int Search(Board board, int depth, int maxDepth)
    {
        // we have reached the depth - evaluate the board for the current color
        if (depth == maxDepth)
            return Evaluate(board);

        int bestScore = int.MinValue ;
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            // negate the score because after making a move,
            // we are looking at the board from the other player's perspective
            int score = -Search(board, depth+1, maxDepth);
            if (score > bestScore)
            {
                bestScore = score;
                if (depth == 0)
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
    private int Evaluate(Board board)
    {
      if (board.IsInCheckmate()) return Int32.MinValue;
      if (board.IsInCheck()) return Int32.MinValue / 2;
      if (board.IsRepeatedPosition()) return Int32.MinValue / 4;
      if (board.IsDraw()) return 0;

      int myMobility = board.GetLegalMoves().Length;
      board.TrySkipTurn(); // we already checked for IsInCheck above
      int theirMobility = board.GetLegalMoves().Length;
      board.UndoSkipTurn();

      int score = board.GetLegalMoves().Length;
      int[] pieceScores = { 0, 0, 0, 0, 0, 0, 0 };
      foreach(PieceList list in board.GetAllPieceLists()) {
          pieceScores[(int)list.TypeOfPieceInList] += list.Count * (list.IsWhitePieceList==board.IsWhiteToMove ? 1 : -1);
      }
      int pieceScore = pieceScores.Select((pieceScore, index) => pieceScore * PIECE_VALUES[index]).Sum();
      return (myMobility - theirMobility) + pieceScore;
    }
}
