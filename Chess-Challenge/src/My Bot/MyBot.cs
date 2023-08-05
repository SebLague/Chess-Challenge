using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    private int numEvals; // #DEBUG
    private readonly int[] pieceValues = { 0, 100, 300, 330, 500, 800, 10000 };

    private int searchDepth;
    private Move bestMove;

    public Move Think(Board board, Timer timer)
    {
        for (int depth = 1; depth <= 5; depth++)
        {
            numEvals = 0; // #DEBUG

            if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 30) break;

            searchDepth = depth;
            Negamax(board, int.MinValue, int.MaxValue, depth);

            //Console.WriteLine($"{numEvals} evals at {depth} depth"); // #DEBUG
        }

        return bestMove;
    }

    private int Negamax(Board board, int alpha, int beta, int depth)
    {
        if (depth <= 0) return Evaluate(board);

        int bestEval = int.MinValue;
        Move[] moves = board
            .GetLegalMoves()
            .OrderByDescending(x => pieceValues[(int)x.CapturePieceType] - pieceValues[(int)x.MovePieceType]).ToArray();

        foreach (Move aMove in moves)
        {
            board.MakeMove(aMove);
            int moveEval = -Negamax(board, -beta, -alpha, depth - 1);
            board.UndoMove(aMove);

            if (moveEval > bestEval)
            {
                bestEval = moveEval;

                // alpha/beta pruning
                alpha = Math.Max(bestEval, alpha);
                if (depth == searchDepth) bestMove = aMove;
                if (alpha >= beta) break;
            }
        }

        return bestEval;
    }

    private int Evaluate(Board board)
    {
        numEvals++; //#DEBUG
        int boardEval = 0;

        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            int materialEval = pieceValues[(int)pieceList.TypeOfPieceInList] * pieceList.Count;
            materialEval *= pieceList.IsWhitePieceList ? 1 : -1;
            boardEval += materialEval;
        }

        return boardEval * (board.IsWhiteToMove ? 1 : -1);
    }
}