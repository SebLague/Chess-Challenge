using ChessChallenge.API;

namespace Bots;

public class MyBot3 : IChessBot //Nik I want to change this class name >:(
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        //FOR EVERY MOVE check the affect it would have on the board. (This obviously needs to be streamlined).
        float best_eval = -200;
        Move best_move = new Move();
        foreach(Move m in moves)
        {
            board.MakeMove(m);
            float e = Eval(board);

            if(e > best_eval)
            {
                best_eval = e; best_move = m;
            }
            board.UndoMove(m);//This is daft
        }
        
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
