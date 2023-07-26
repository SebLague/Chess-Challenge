using ChessChallenge.API;
using System;
using System.Linq;
using System.Data;
using static ChessChallenge.Application.ConsoleHelper;

public class MyBot : IChessBot
{
    //                             .  P   K   B   R   Q   K
    private int[] _pieceValues = { 0, 10, 30, 30, 50, 90, 2000 };
    private Move bestMove = Move.NullMove;

    public Move Think(Board board, Timer timer)
    {
        int depth = 0;
        int maxdepth = 3;
        int bestScore = Search(board, depth, maxdepth);
        return bestMove;
    }

    int Search(Board board, int depth, int maxdepth)
        /*  Input
         *
         *  board: Board, Current board
         *  depth: int, Current search depth
         *  maxdepth: int, maximal depth to be searches + the depth at which the boards are ultimately evaluated
         *
         *  Output
         *
         *  bestscore: int, the score of the board at depth=maxdepth with the best score obtained
        */
    {
        if (depth == maxdepth)
        /*
         *Evaluate returns int score, scored in respect to the next moving player. The board it is evaluating is the
         *end board of one branch and scores will be handed back to the next node (*-1, since the black moves that creates a bad setting for white will be made and
         *a white move that creates a bad setting for black is our right move)
        */
            return Evaluate(board);

        int bestScore = int.MinValue ;
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = -Search(board, depth+1, maxdepth);
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


