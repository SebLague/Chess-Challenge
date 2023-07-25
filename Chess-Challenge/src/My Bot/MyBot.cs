using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        System.Random rng = new();
        return moves[rng.Next(moves.Length)];
    }
}