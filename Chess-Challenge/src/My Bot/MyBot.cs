using ChessChallenge.API;
using System.Collections;

public class MyBot : IChessBot
{   

    public Move Think(Board board, Timer timer)
    {  
        System.Random rand = new System.Random();
        Move[] moves = board.GetLegalMoves();
        
        int N = rand.Next(moves.Length);
        return moves[N];

    }
}
