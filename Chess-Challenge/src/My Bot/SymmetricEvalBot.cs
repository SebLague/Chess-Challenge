using ChessChallenge.API;
using ChessChallenge.Application; //DELETE
using System;

namespace Bots;

public class MyBot3 : IChessBot //Nik I want to change this class name >:(
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        //FOR EVERY MOVE check the affect it would have on the board. (This obviously needs to be optimised).
        float best_eval = 0;
        Move best_move = moves[0];
        foreach(Move m in moves)
        {
            board.MakeMove(m);
            float e = Eval(board);
            float abs_e = Math.Abs(e);

            if(abs_e > best_eval)
            {
                best_eval = abs_e; 
                best_move = m;
            }
            //ConsoleHelper.Log(m.ToString() + " gives eval: " + e.ToString());

            board.UndoMove(m);//This is daft
        }
        ConsoleHelper.Log(best_move.ToString() + " was the best move found with eval: " + Math.Abs(best_eval).ToString());

        return best_move;
    }

    //Rudimentary evaluation function as proposed here
    // https://www.chessprogramming.org/Evaluation
    private float Eval(Board board)
    {
        int P = board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count;
        int N = board.GetPieceList(PieceType.Knight, true).Count - board.GetPieceList(PieceType.Knight, false).Count;
        int B = board.GetPieceList(PieceType.Bishop, true).Count - board.GetPieceList(PieceType.Bishop, false).Count;
        int R = board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count;
        int Q = board.GetPieceList(PieceType.Queen, true).Count - board.GetPieceList(PieceType.Queen, false).Count;

        //Multiply the material differences by their respective weights.
        float result = (9 * Q) + 
            (5 * R) + 
            (3 * (B + N)) + 
            (1 * P);

        if(!board.IsWhiteToMove)
        {
            result *= -1; //Adjusting result for when playing the black pieces.
        }

        return result;
    }
}
