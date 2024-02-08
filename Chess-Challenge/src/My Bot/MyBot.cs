using ChessChallenge.API;

public class MyBot : IChessBot
{
    /// <summary>
    /// This method is called automatically by the framework whenever it's this bot's turn to move.
    /// Illegal moves will be rejected by the framework.
    /// Use the methods in <see cref="Board"/> such as GetLegalMoves() to help in making a decision for the bot's next move.
    /// </summary>
    /// <param name="board">The current board's status.</param>
    /// <param name="timer">This bot's timer.</param>
    /// <returns></returns>
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return moves[0];
    }
}