using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Span<Move> moves = stackalloc Move[256];
        board.GetLegalMoves(ref moves);
        return moves[0];
    }
}