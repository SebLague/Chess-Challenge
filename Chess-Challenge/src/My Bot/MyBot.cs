using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    private class EvaluatedMove : IComparable<EvaluatedMove>
    {
        public EvaluatedMove(Move move, int eval)
        {
            Move = move;
            Eval = eval;
        }

        public Move Move { get; set; }
        public int Eval { get; set; }

        public int CompareTo(EvaluatedMove? other)
        {
            return Eval.CompareTo(other.Eval);
        }
    }

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        Move bestMove = moves[0];
        int bestMoveEvaluation = int.MinValue;
        foreach (Move move in moves)
        {
            board.MakeMove(move);

            // Evaluate the board
            EvaluatedMove evaluation = NegaMax(board, 3, int.MinValue, int.MaxValue);
            Console.WriteLine(-evaluation.Eval);
            if (-evaluation.Eval > bestMoveEvaluation)
            {
                bestMove = move;
                bestMoveEvaluation = -evaluation.Eval;
            }

            board.UndoMove(move);
        }
        Console.WriteLine("Best: " + bestMoveEvaluation);
        return bestMove;
    }


    private EvaluatedMove NegaMax(Board board, int depth, int alpha, int beta)
    {
        // Check terminal node
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw()) { return new(default, Evaluate(board)); }

        Move[] moves = board.GetLegalMoves();
        // Order moves here

        EvaluatedMove best = new(default, int.MinValue);
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            EvaluatedMove temp = NegaMax(board, depth - 1, alpha, beta);
            temp.Eval *= -1;
            board.UndoMove(move);

            best = (temp.Eval > best.Eval) ? temp : best;
            alpha = Math.Max(alpha, best.Eval);
            if (alpha >= beta) { break; }
        }
        return best;
    }


    private static int Evaluate(Board board)
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