using ChessChallenge.API;
using System;
using System.Xml.Linq;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        Move bestMove = moves[0];
        int bestMoveEvaluation = int.MinValue;
        foreach (Move move in moves)
        {
            board.MakeMove(move);

            // Evaluate the board
            int evaluation = NegaMax(board, 3, int.MinValue, int.MaxValue);
            Console.WriteLine(evaluation);
            if (evaluation > bestMoveEvaluation)
            {
                bestMove = move; 
                bestMoveEvaluation = evaluation;
            }

            board.UndoMove(move);
        }
        Console.WriteLine("Best: " + bestMoveEvaluation);
        return bestMove;
    }

    
    private int NegaMax(Board board, int depth, int a, int b)
    {
        if (depth == 0 || board.IsInCheckmate()) { return Evaluate(board); }


        Move[] moves = board.GetLegalMoves();
        // Order moves here

        int value = int.MinValue;
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            value = Math.Max(value, -NegaMax(board, depth - 1, a, b));
            a = Math.Max(a, value);
            board.UndoMove(move);
            if (a >= b) { break; }
        }

        return value;
    }


    private static int Evaluate(Board board)
    {
        int evaluatedScore = 0;

        if (board.IsDraw()) { return evaluatedScore; }

        PieceList[] pieces = board.GetAllPieceLists();

        // Use a weighted piece differential
        int[] values = { 94, 281, 297, 512, 1025, 100000 };
        for (int i = 0; i < 6; i++)
        {
            evaluatedScore += (pieces[i].Count - pieces[i + 6].Count) * values[i];
        }

        // Check if a player is in checkmate

        return evaluatedScore;
    }
}