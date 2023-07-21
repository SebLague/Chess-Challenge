using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves(true);

        if(moves.Length == 0){
            return RandoChoice(board.GetLegalMoves(false));
        }else{
            return RandoChoice(moves);
        }
        
    }

    private Move RandoChoice(Move[] moves){
        Random rnd = new Random();
        return moves[rnd.Next() % moves.Length];
    }
}