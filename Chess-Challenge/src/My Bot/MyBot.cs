using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ChessChallenge.API;
using Timer = ChessChallenge.API.Timer;

public class MyBot : IChessBot
{
    private HashSet<ulong> _repetitions;

    private const int Inf = 2000000;
    private const int Mate = 1000000;

    public MyBot()
    {
        _repetitions = new HashSet<ulong>();
    }

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

    private int Search(Board board, Timer timer, int totalTime, int ply, int depth, int alpha, int beta, HashSet<ulong> repetitions, out Move bestMove)
    {
        bestMove = Move.NullMove;

        if (ply > 0 && repetitions.Contains(board.ZobristKey))
        {
            return 0;
        }

        if (depth == 0)
        {
            var score = Evaluate(board);
            return score;
        }

        var moves = board.GetLegalMoves();
        moves = moves.OrderBy(move => !move.IsCapture).ToArray();
        var bestScore = -Inf;
        var movesEvaluated = 0;

        // Loop over each legal move
        foreach (var move in moves)
        {
            // If we are out of time, stop searching
            if (depth > 2 && timer.MillisecondsElapsedThisTurn * 30 > totalTime)
            {
                return bestScore;
            }

            board.MakeMove(move);
            var score = -Search(board, timer, totalTime, ply + 1, depth - 1, -beta, -alpha, repetitions, out _);
            board.UndoMove(move);

            // Count the number of moves we have evaluated for detecting mates and stalemates
            movesEvaluated++;

            // If the move is better than our current best, update our best move
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
                if (score > alpha)
                {
                    alpha = score;
                    if (score >= beta)
                    {
                        break;
                    }
                }
            }
        }

        if (movesEvaluated == 0)
        {
            if (board.IsInCheck())
            {
                // Checkmate
                return -Mate;
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

        _repetitions.Add(board.ZobristKey);
        var repetitionsCopy = _repetitions.ToHashSet();

        var bestMove = Move.NullMove;
        // Iterative deepening
        for (var depth = 1; depth < 128; depth++)
        {
            var score = Search(board, timer, totalTime, 0, depth, -Inf, Inf, repetitionsCopy, out var move);

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
