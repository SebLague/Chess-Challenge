using ChessChallenge.API;

namespace TeenyLynx.Test;
public abstract class BaseTest
{
    protected static MyBot GetBot(string fen)
    {
        var teenyLynx = new MyBot
        {
            _position = GetBoard(fen)
        };

        return teenyLynx;
    }

    protected static Board GetBoard(string fen)
    {
        var internalBoard = new ChessChallenge.Chess.Board();
        internalBoard.LoadPosition(fen);

        return new(internalBoard);
    }
}
