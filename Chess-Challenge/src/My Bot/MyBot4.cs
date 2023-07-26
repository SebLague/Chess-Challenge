using ChessChallenge.API;

namespace Bots;

public class MyBot4 : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return moves[0];
    }
}
