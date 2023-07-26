using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{

    //                             .  P   K   B   R   Q   K
    private int[] _pieceValues = { 0, 10, 30, 30, 50, 90, 2000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Move bestMove = Move.NullMove;
        int bestScore = Int32.MinValue;
        foreach (Move move in moves) {
          board.MakeMove(move);
          int score = -Evaluate(board);
          board.UndoMove(move);
          if (score > bestScore) {
            bestScore = score;
            bestMove = move;
          }
        }
        return bestMove;
    }

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
      int pieceScore = pieceScores.Select((pieceScore, index) => pieceScore * _pieceValues[index]).Sum();
      return (myMobility - theirMobility) + pieceScore;
    }
}
