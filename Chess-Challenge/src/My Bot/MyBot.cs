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
        int evaluatedScore = 0;

        // Check end conditions
        if (board.IsDraw()) { return evaluatedScore; }
        if (board.IsInCheckmate()) { return board.IsWhiteToMove ? -10000000 : 10000000; }

        PieceList[] pieces = board.GetAllPieceLists();

        // Use a weighted piece differential
        // TODO: Change to use pice locations and: pieces[i].GetPiece(j).Square.Index;
        int[] values = { 94, 281, 297, 512, 1025, 100000 };
        for (int i = 0; i < 6; i++)
        {
            evaluatedScore += (pieces[i].Count - pieces[i + 6].Count) * values[i];
        }

        // Check if a player is in check
        if (board.IsInCheck()) { evaluatedScore += 50000 * (board.IsWhiteToMove ? 1 : -1); }

        return evaluatedScore * (board.IsWhiteToMove ? 1 : -1);
    }
}