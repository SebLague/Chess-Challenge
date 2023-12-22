using ChessEngine;

namespace ChessChallenge.Bots;

public class DefaultBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return moves[0];
    }
}