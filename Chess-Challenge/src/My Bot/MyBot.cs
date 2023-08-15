using ChessChallenge.API;
using System;

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
        },
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0
        }
    };


    public Move Think(Board board, Timer timer)
    {
        EvaluatedMove best = NegaMax(board, 5, double.NegativeInfinity, double.PositiveInfinity);
        best.Eval *= -1;
        return best.Move;
    }
    

    private EvaluatedMove NegaMax(Board board, int depth, double alpha, double beta)
    {
        // Check terminal node
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw()) { return new(default, Evaluate(board)); }

        Move[] moves = board.GetLegalMoves();
        // Order moves here

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
        return best;
    }


    private static double Evaluate(Board board)
    {
        double evaluatedScore = 0;

        // Check end conditions
        if (board.IsDraw()) { return evaluatedScore; }
        if (board.IsInCheckmate()) { return board.IsWhiteToMove ? Double.NegativeInfinity: Double.PositiveInfinity; }

        PieceList[] pieces = board.GetAllPieceLists();

        // Use a weighted piece differential & also add lookup table values
        int[] values = { 94, 281, 297, 512, 1025, 100000 };
        for (int i = 0; i < 6; i++)
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