using ChessChallenge.API;
using System;
using ChessChallenge.Application;

//using static ChessChallenge.Application.ConsoleHelper;

public class MyBot : IChessBot
{
    double[] pieceValues = { 0, 1, 3.2, 3, 5, 0, 500 };

    public Move Think(Board board, Timer timer)
    {
        int moveTime = 500;

        //ConsoleHelper.Log("Thinking");

        Move[] allMoves = board.GetLegalMoves();

        Move bestMove = allMoves[0];
        double bestMoveEval = 1000000;
        foreach (var move in allMoves)
        {
            board.MakeMove(move);
            var eval = Search(board, 3);
            if (eval < bestMoveEval)
            {
                bestMoveEval = eval;
                bestMove = move;
            }
            board.UndoMove(move);
        }

        return bestMove;
    }

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    double Search(Board board, int depth)
    {
        if (depth == 0)
        {
            return EvaluatePosition(board);
        }

        Move[] allMoves = board.GetLegalMoves();
        if (allMoves.Length == 0)
        {
            return EvaluatePosition(board);
        }
        Move moveToPlay = allMoves[0];
        var opponentsEvaluation = 1000000.0;

        foreach (Move move in allMoves)
        {
            board.MakeMove(move);
            var evaluation = Search(board, depth - 1);
            if (evaluation < opponentsEvaluation)
            {
                opponentsEvaluation = evaluation;
            }
            board.UndoMove(move);
        }

        return -opponentsEvaluation;
    }


    double EvaluatePosition(Board board)
    {
        if (board.IsInCheckmate()) { return -100000; }

        var allPieceLists = board.GetAllPieceLists();

        var evaluation = 0.0;

        foreach (var pieceList in allPieceLists)
        {
            double pieceListValue = pieceList.Count * pieceValues[(int)pieceList.TypeOfPieceInList];

            if (!pieceList.IsWhitePieceList)
            {
                pieceListValue *= -1;
            }

            evaluation += pieceListValue;
        }

        if (!board.IsWhiteToMove)
        {
            evaluation *= -1;
        }

        return evaluation;
    }
}