using ChessChallenge.API;
using System;
using System.Diagnostics;

public class MyBot : IChessBot
{
    //bool ranoBool = false; 4 tokens!
    public Move Think(Board board, Timer timer)
    {

        if(Dynamic(board) != null){
            return Dynamic(board).Value;
        }else{
            return RandoChoice(board.GetLegalMoves(false));
        }
        
    }

    private static Move RandoChoice(Move[] moves){
        Random rnd = new Random();
        return moves[rnd.Next() % moves.Length];
    }

    private Move? Dynamic(Board board){
        Move[] moves = board.GetLegalMoves(true);

        if(moves.Length == 0){
            return null;
        }

        int side;
        if(board.IsWhiteToMove){
            side = 1;
        }else{
            side = -1;
        }

        int bestMove = side*Eval(board);
        Move? thatMove =  null;

        foreach (Move move in moves)
        {
            board.MakeMove(move);

            int tempSide = side;
            Board tempBoard = Board.CreateBoardFromFEN(board.GetFenString());
            Move? nextMove = Dynamic(tempBoard);

            int depth = 1;
            while(nextMove != null && depth < 3){
                System.Console.WriteLine(tempBoard.GetFenString());
                tempBoard.MakeMove(nextMove.Value);
                tempSide *= -1;
                nextMove = Dynamic(tempBoard);
                depth++;
            }

            if(tempSide * Eval(tempBoard) > bestMove){
                bestMove = tempSide*Eval(tempBoard);
                thatMove = move;
            }

            board.UndoMove(move);
        }

        return thatMove;
    }

    private static int Eval(Board board){
        PieceList[] pieces = board.GetAllPieceLists();

        int[] weights = {1,3,3,5,11,0,-1,-3,-3,-5,-11,0};

        int sum = 0;
        for(int i =0;i<pieces.Length;i++){
            sum += weights[i]*pieces[i].Count;
        }

        return sum;
    }
}