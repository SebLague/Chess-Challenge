using System;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 0 };

    private int Evaluate(Board board)
    {
        int score = 0;
        for (var color = 0; color < 2; color++)
        {
            var isWhite = color == 0;
            for (var piece = PieceType.Pawn; piece <= PieceType.King; piece++)
            {
                var pieceIndex = (int)piece;
                var bitboard = board.GetPieceBitboard(piece, isWhite);

                while (bitboard != 0)
                {
                    var sq = BitOperations.TrailingZeroCount(bitboard);
                    bitboard &= bitboard - 1;

                    // Material
                    score += pieceValues[pieceIndex];

                    // Centrality
                    var rank = sq >> 3;
                    var file = sq & 7;
                    var centrality = -Math.Abs(7 - rank - file) - Math.Abs(rank - file);
                    score += centrality * (6 - pieceIndex);
                }
            }

            score = -score;
        }

        if (!board.IsWhiteToMove)
        {
            score = -score;
        }

        return score;
    }

    public int Search(Board board, Timer timer, int totalTime, int depth, out Move bestMove)
    {
        bestMove = Move.NullMove;
        if (depth == 0)
        {
            var score = Evaluate(board);
            return score;
        }

        var moves = board.GetLegalMoves();
        var bestScore = int.MinValue;
        var movesEvaluated = 0;

        // Loop over each legal move
        foreach (var move in moves)
        {
            // If we are out of time, stop searching
            if (timer.MillisecondsElapsedThisTurn * 30 > totalTime)
            {
                return bestScore;
            }

            board.MakeMove(move);
            var score = -Search(board, timer, totalTime, depth - 1, out _);
            board.UndoMove(move);

            // If the move is better than our current best, update our best move
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            // Count the number of moves we have evaluated for detecting mates and stalemates
            movesEvaluated++;
        }

        if (movesEvaluated == 0)
        {
            if (board.IsInCheck())
            {
                // Checkmate
                return -1000000;
            }
            else
            {
                // Stalemate
                return 0;
            }
        }

        return bestScore;
    }

    public Move Think(Board board, Timer timer)
    {
        var totalTime = timer.MillisecondsRemaining;
        var bestMove = Move.NullMove;
        // Iterative deepening
        for (var depth = 1; depth < 128; depth++)
        {
            var score = Search(board, timer, totalTime, depth, out var move);

            // If we are out of time, we cannot trust the move that was found during this iteration
            if (timer.MillisecondsElapsedThisTurn * 30 > totalTime)
            {
                break;
            }

            bestMove = move;
            Console.WriteLine($"{score} {move}");
        }

        return bestMove;
    }
}