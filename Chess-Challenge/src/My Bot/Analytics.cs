using ChessChallenge.API;
using System;
using System.Diagnostics;
using static ChessChallenge.Application.ConsoleHelper;

namespace ChessChallenge.DriedCod
{
    public class Analytics
    {
      private const int MIN_DEPTH = 2;
      private const int MAX_DEPTH = 20;
      private const int MAX_TIME = 45_000;
      private const int STOP_EARLY_TIME = 20_000;

      public Analytics()
      {
          Log("movesDone,legalMoves,pieceCount,ourPieceCount,theirPieceCount,depth,time");
      }

      public void AnalyzeSearchTime(Board board, MyBot myBot)
      {
        int oldTimeForMove = myBot.timeForMove;
        int oldDepth = myBot.depth;

        int movesDone = board.GameMoveHistory.Length;
        int legalMoves = board.GetLegalMoves().Length;
        int pieceCount = BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard);
        int ourPieceCount = BitboardHelper.GetNumberOfSetBits(board.IsWhiteToMove ? board.WhitePiecesBitboard : board.BlackPiecesBitboard);
        int theirPieceCount = BitboardHelper.GetNumberOfSetBits(board.IsWhiteToMove ? board.BlackPiecesBitboard : board.WhitePiecesBitboard);

        Stopwatch sw = new();

        for (int depth = MIN_DEPTH; depth <= MAX_DEPTH; depth++)
        {
          myBot.depth = depth;
          myBot.timeForMove = myBot.gameTimer.MillisecondsElapsedThisTurn + MAX_TIME;
          sw.Restart();
          myBot.Search(board, depth, -500_000_000, 500_000_000, 0);
          sw.Stop();

          if (sw.ElapsedMilliseconds >= MAX_TIME)
          {
              break;
          }
          else
          {
              Log($"{movesDone},{legalMoves},{pieceCount},{ourPieceCount},{theirPieceCount},{depth},{sw.ElapsedMilliseconds}");
          }
          // stop early
          if (sw.ElapsedMilliseconds > STOP_EARLY_TIME)
          {
              break;
          }
        }

        myBot.timeForMove = oldTimeForMove;
        myBot.depth = oldDepth;
      }
    }

}
