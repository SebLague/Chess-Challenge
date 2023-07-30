using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ChessChallenge.API;
using Timer = ChessChallenge.API.Timer;

public class MyBot : IChessBot
{
    private const int Inf = 2000000;
    private const int Mate = 1000000;

    const int TTSize = 1048576;
    (ulong, Move)[] TT = new (ulong, Move)[TTSize];

    int[] pieceValues = { 0, 151, 419, 458, 731, 1412, 0 };

    // PSTs are encoded with the following format:
    // Every rank or file is encoded as a byte, with the first rank/file being the LSB and the last rank/file being the MSB.
    // For every value to fit inside a byte, the values are divided by 2, and multiplication inside evaluation is needed.
    ulong[] pstRanks = {0, 32973249741911296, 16357091511995071475, 17581496622553367027, 724241724997039354, 432919517870226424, 17729000522595302646 };
    ulong[] pstFiles = {0, 17944594909985834239, 17438231369917791979, 17799354947352068342, 17580088143863153148, 217585671819360496, 17944030877684269297 };

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

                    if (color == 1)
                    {
                        sq ^= 56;
                    }

                    var rank = sq >> 3;
                    var file = sq & 7;

                    // Material
                    score += pieceValues[pieceIndex];

                    // Rank PST
                    var rankScore = (sbyte)((pstRanks[pieceIndex] >> (rank * 8)) & 0xFF) * 2;
                    score += rankScore;

                    // File PST
                    var fileScore = (sbyte)((pstFiles[pieceIndex] >> (file * 8)) & 0xFF) * 2;
                    score += fileScore;
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

    private int Search(Board board, Timer timer, int totalTime, int ply, int depth, int alpha, int beta, out Move bestMove)
    {
        ulong key = board.ZobristKey;
        bestMove = Move.NullMove;

        // Repetition detection
        if (ply > 0 && board.IsRepeatedPosition())
        {
            return 0;
        }

        // If we are in check, we should search deeper
        if (board.IsInCheck())
            depth++;

        var inQsearch = (depth <= 0);

        var (ttKey, ttMove) = TT[key % TTSize];
        if (ttKey != key)
            ttMove = Move.NullMove;

        // Move generation, best-known move then MVV-LVA ordering
        var moves = board.GetLegalMoves(inQsearch).OrderByDescending(move => move == ttMove ? 1000000 : (int)move.CapturePieceType * 1000 - (int)move.MovePieceType);

        var bestScore = -Inf;
        var movesEvaluated = 0;

        if (inQsearch)
        {
            bestScore = Evaluate(board);
            if (bestScore >= beta)
                return bestScore;

            if (bestScore > alpha)
                alpha = bestScore;
        }

        // Loop over each legal move
        foreach (var move in moves)
        {
            // If we are out of time, stop searching
            if (depth > 2 && timer.MillisecondsElapsedThisTurn * 30 > totalTime)
            {
                return bestScore;
            }

            board.MakeMove(move);
            var score = -Search(board, timer, totalTime, ply + 1, depth - 1, -beta, -alpha, out _);
            board.UndoMove(move);

            // Count the number of moves we have evaluated for detecting mates and stalemates
            movesEvaluated++;

            // If the move is better than our current best, update our best move
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;

                // If the move is better than our current alpha, update alpha
                if (score > alpha)
                {
                    alpha = score;

                    // If the move is better than our current beta, we can stop searching
                    if (score >= beta)
                    {
                        break;
                    }
                }
            }
        }

        if (!inQsearch && movesEvaluated == 0)
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

        TT[key % TTSize] = (key, bestMove);

        return bestScore;
    }

    public Move Think(Board board, Timer timer)
    {
        var totalTime = timer.MillisecondsRemaining;

        var bestMove = Move.NullMove;
        // Iterative deepening
        for (var depth = 1; depth < 128; depth++)
        {
            var score = Search(board, timer, totalTime, 0, depth, -Inf, Inf, out var move);

            // If we are out of time, we cannot trust the move that was found during this iteration
            if (timer.MillisecondsElapsedThisTurn * 30 > totalTime)
            {
                break;
            }

            bestMove = move;

            // For debugging purposes, can be removed if lacking tokens
            // Move is not printed in the usual pv format, because the API does not support easy conversion to UCI notation
            Console.WriteLine($"info depth {depth} cp {score} time {timer.MillisecondsElapsedThisTurn} {bestMove}"); // #DEBUG
        }

        return bestMove;
    }
}
