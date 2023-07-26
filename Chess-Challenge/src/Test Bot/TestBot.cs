using ChessChallenge.API;
using System;

public class TestBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        return moveToPlay;
    }
}   
