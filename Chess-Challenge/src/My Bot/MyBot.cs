﻿using ChessChallenge.API;
using System;
using System.Diagnostics;

public class MyBot : IChessBot
{
    private readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    private int numEvals = 0;

    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;
        for (int depth = 0; depth <= 10; depth++)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 30) break;
            bestMove = Search(board, depth);

            sw.Stop();
            Console.WriteLine($"evals={numEvals} depth={depth} time={sw.ElapsedMilliseconds}ms");
        }

        return bestMove;
    }

    private Move Search(Board board, int maxDepth)
    {
        int bestEval = int.MinValue;
        Move bestMove = Move.NullMove;
        foreach (Move aMove in board.GetLegalMoves())
        {
            int moveEval = -Negamax(board, aMove, maxDepth);
            if (moveEval > bestEval)
            {
                bestEval = moveEval;
                bestMove = aMove;
            }
        }

        return bestMove;
    }

    private int Negamax(Board board, Move move, int depth)
    {
        board.MakeMove(move);
        int bestEval = int.MinValue;

        if (depth <= 0) bestEval = Evaluate(board);
        else
        {
            foreach (Move aMove in board.GetLegalMoves())
            {
                int moveEval = -Negamax(board, aMove, depth - 1);
                bestEval = Math.Max(moveEval, bestEval);
            }
        }

        board.UndoMove(move);
        return bestEval;
    }

    private int Evaluate(Board board)
    {
        numEvals++;
        int boardEval = 0;

        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            int materialEval = pieceValues[(int)pieceList.TypeOfPieceInList] * pieceList.Count;
            materialEval *= pieceList.IsWhitePieceList ? 1 : -1;
            boardEval += materialEval;
        }

        return boardEval * (board.IsWhiteToMove ? 1 : -1);
    }
}