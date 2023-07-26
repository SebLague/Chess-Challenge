using System;
using ChessChallenge.API;

namespace Bots;

public class RandomBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Random rnd = new Random();
        Move[] moves = board.GetLegalMoves();
        return moves[rnd.Next(0,moves.Length)];
    }
}
