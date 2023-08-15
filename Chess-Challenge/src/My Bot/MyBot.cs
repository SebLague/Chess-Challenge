using ChessChallenge.API;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    private class EvaluatedMove : IComparable<EvaluatedMove>
    {
        public EvaluatedMove(Move move, double eval)
        {
            Move = move;
            Eval = eval;
        }

        public Move Move { get; set; }
        public double Eval { get; set; }

        int IComparable<EvaluatedMove>.CompareTo(EvaluatedMove? other)
        {
            return Eval.CompareTo(other.Eval);
        }
    }
    
    // TODO: Implement generation for these to reduce the token count (or convert to Neural Net)
    private static readonly double[,] TABLES =
    { 
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5, 5, 10, 25, 25, 10, 5, 5,
            0, 0, 0, 20, 20, 0, 0, 0,
            5, -5, -10, 0, 0, -10, -5, 5,
            5, 10, 10, -20, -20, 10, 10, 5,
            0, 0, 0, 0, 0, 0, 0, 0
        },
        {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        },
        {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        },
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0,  5,  5,  0,  0,  0
        },
        {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,  0,  5,  5,  5,  5,  0, -5,
            0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        }
    };
    private static readonly int MAX_DEPTH = 6;

    // TODO: Transposition Tables (Zobrist Key -> { depth, flag, move })
    // flag: 0 -> LOWERBOUND, 1 -> EXACT, 2 -> UPPERBOUND
    private readonly Dictionary<ulong, Tuple<int, int, EvaluatedMove>> transpositions = new Dictionary<ulong, Tuple<int, int, EvaluatedMove>>();

    public Move Think(Board board, Timer timer)
    {
        EvaluatedMove best = new(default, 0);
        // TODO: add a method to check for time constraints...
        for (int depth = 1; depth <= MAX_DEPTH; depth++)
        {
            best = NegaMax(board, depth, double.NegativeInfinity, double.PositiveInfinity);
            best.Eval *= -1;
            Console.WriteLine(depth);
        }
        
        return best.Move;
    }
    

    private EvaluatedMove NegaMax(Board board, int depth, double alpha, double beta)
    {
        double alphaOrig = alpha;
        // Perform transposition lookup
        if (transpositions.ContainsKey(board.ZobristKey) && transpositions[board.ZobristKey].Item1 >= depth)
        {
            if (transpositions[board.ZobristKey].Item2 == 0)
            {
                alpha = Math.Max(alpha, transpositions[board.ZobristKey].Item3.Eval);
            }
            else if (transpositions[board.ZobristKey].Item2 == 2)
            {
                beta = Math.Min(beta, transpositions[board.ZobristKey].Item3.Eval);
            }

            if ((transpositions[board.ZobristKey].Item2 == 1) || alpha >= beta)
            {
                return transpositions[board.ZobristKey].Item3;
            }
        }


        // Check terminal node
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw()) { return new(default, Evaluate(board)); }

        Move[] moves = board.GetLegalMoves();
        // Order moves here?? 

        // Perform Negamax with alpha beta pruning
        EvaluatedMove best = new(default, int.MinValue);
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            EvaluatedMove temp = NegaMax(board, depth - 1, -beta, -alpha);
            temp.Move = move;
            temp.Eval *= -1;
            board.UndoMove(move);

            best = (temp.Eval > best.Eval) ? temp : best;
            alpha = Math.Max(alpha, best.Eval);
            if (alpha >= beta) { break; }
        }

        // Perform transposition storage
        transpositions[board.ZobristKey] = new(depth, (best.Eval >= beta ? 2 : (best.Eval <= alphaOrig) ? 0 : 1), best);
        return best;
    }

    // TODO: Add more evaluation metrics OR train a Neural Net to evaluate positions
    private static double Evaluate(Board board)
    {
        double evaluatedScore = 0;

        // Check end conditions
        if (board.IsDraw()) { return evaluatedScore; }
        if (board.IsInCheckmate()) { return board.IsWhiteToMove ? Double.NegativeInfinity: Double.PositiveInfinity; }

        PieceList[] pieces = board.GetAllPieceLists();

        // Use a weighted piece differential & also add lookup table values
        int[] values = { 94, 281, 297, 512, 1025 };
        for (int i = 0; i < 5; i++)
        {
            evaluatedScore += (pieces[i].Count - pieces[i + 6].Count) * values[i];
            foreach (Piece white in pieces[i])
                evaluatedScore += TABLES[i, white.Square.Index];
            foreach (Piece black in pieces[i])
                evaluatedScore -= TABLES[i, 63 - black.Square.Index];
        }

        // Check if a player is in check
        if (board.IsInCheck()) { evaluatedScore += 50000 * (board.IsWhiteToMove ? 1 : -1); }

        return evaluatedScore * (board.IsWhiteToMove ? 1 : -1);
    }
}