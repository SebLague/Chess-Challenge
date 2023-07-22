using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private Random _rng;

    public MyBot()
    {
        _rng = new Random();
    }

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        var moveIndex = _rng.Next(moves.Length);
        return moves[moveIndex];
    }
}