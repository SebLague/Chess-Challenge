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

    // Key, move, depth, score, flag
    (ulong, Move, int, int, byte)[] TT = new (ulong, Move, int, int, byte)[TTSize];

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

    private int Search(Board board, Timer timer, int totalTime, int ply, int depth, int alpha, int beta, long[,] quietHistory, out Move bestMove)
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

        // Look up best move known so far if it is available
        var (ttKey, ttMove, ttDepth, ttScore, ttFlag) = TT[key % TTSize];

        if (ttKey == key)
        {
            if (ply > 0 && ttDepth >= depth && (ttFlag == 0 && ttScore <= alpha || ttFlag == 1 && ttScore >= beta || ttFlag == 2))
                return ttScore;
        }
        else
            ttMove = Move.NullMove;

        var inQsearch = (depth <= 0);
        
        var staticScore = Evaluate(board);
        var bestScore = -Inf;
        if (inQsearch)
        {
            if (staticScore >= beta)
                return staticScore;

            if (staticScore > alpha)
                alpha = staticScore;

            bestScore = staticScore;
        }

        // Reverse futility pruning
        else if (ply > 0 && depth < 5 && staticScore - depth * 150 > beta && !board.IsInCheck())
            return beta;

        // Move generation, best-known move then MVV-LVA ordering then quiet move history
        var moves = board.GetLegalMoves(inQsearch).OrderByDescending(move => move == ttMove ? 9000000000000000000 : move.IsCapture ? 8000000000000000000 + (long)move.CapturePieceType * 1000 - (long)move.MovePieceType : quietHistory[move.StartSquare.Index, move.TargetSquare.Index]);

        var movesEvaluated = 0;
        byte flag = 0; // Upper

        // Loop over each legal move
        foreach (var move in moves)
        {
            board.MakeMove(move);
            var score = -Search(board, timer, totalTime, ply + 1, depth - 1, -beta, -alpha, quietHistory, out _);
            board.UndoMove(move);

            // If we are out of time, stop searching
            if (depth > 2 && timer.MillisecondsElapsedThisTurn * 30 > totalTime)
            {
                return bestScore;
            }

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
                    flag = 2; // Exact

                    // If the move is better than our current beta, we can stop searching
                    if (score >= beta)
                    {
                        // If the move is not a capture, add a bonus to the quiets table
                        if (!move.IsCapture)
                            quietHistory[move.StartSquare.Index, move.TargetSquare.Index] += depth * depth;

                        flag = 1; // Lower

                        break;
                    }
                }
            }
        }

        if (movesEvaluated == 0)
        {
            if(inQsearch)
                return alpha;

            if (board.IsInCheck())
            {
                // Checkmate
                return ply-Mate;
            }
            else
            {
                // Stalemate
                return 0;
            }
        }

        TT[key % TTSize] = (key, bestMove, depth, bestScore, flag);

        return bestScore;
    }

    public Move Think(Board board, Timer timer)
    {
        var totalTime = timer.MillisecondsRemaining;
        var quietHistory = new long[64, 64];
        var bestMove = Move.NullMove;
        // Iterative deepening
        for (var depth = 1; depth < 128; depth++)
        {
            var score = Search(board, timer, totalTime, 0, depth, -Inf, Inf, quietHistory, out var move);

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
