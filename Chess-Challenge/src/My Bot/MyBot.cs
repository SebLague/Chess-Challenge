using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    Random rng = new();

    public Move Think(Board board, Timer timer)
    {
        int score = 0;

        Move[] moves = GetMoves(board);
        Move retMove = moves[0];

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int newScore = AlphaBetaMax(board, int.MinValue, int.MaxValue, 4);
            board.UndoMove(move);
            if (newScore < score)
            {
                score = newScore;
                retMove = move;
            }
        }
        Console.WriteLine(score);
        return retMove;
    }

    Move[] GetMoves(Board board)
    {
        return board.GetLegalMoves().OrderBy(x => rng.Next()).ToArray();
    }

    int Evaluate(Board board)
    {
        if (board.IsInCheckmate()) return -10000;
        if (board.IsDraw()) return 0;
        int score = 0;
        PieceList[] pieceLists = board.GetAllPieceLists();
        for (int i = 0; i < 5; i++)
        {
            int val = pieceValues[i + 1];
            score += (pieceLists[i].Count - pieceLists[i + 6].Count) * val;
        }
        return score * (board.IsWhiteToMove ? 1 : -1);
    }

    int AlphaBetaMax(Board board, int alpha, int beta, int depth_left)
    {
        if (depth_left == 0) return Evaluate(board);
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = AlphaBetaMin(board, alpha, beta, depth_left - 1);
            board.UndoMove(move);
            if (score >= beta) return beta;
            if (score > alpha) alpha = score;
        }
        return alpha;
    }

    int AlphaBetaMin(Board board, int alpha, int beta, int depth_left)
    {
        if (depth_left == 0) return -Evaluate(board);
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = AlphaBetaMax(board, alpha, beta, depth_left - 1);
            board.UndoMove(move);
            if (score <= alpha) return alpha;
            if (score < beta) beta = score;
        }
        return beta;
    }

}
