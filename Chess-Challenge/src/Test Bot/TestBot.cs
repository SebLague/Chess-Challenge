using ChessChallenge.API;
using System;

public class TestBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        // Pick a random move to play
        Random rng = new();
        Move moveToPlay = moves[rng.Next(moves.Length)];
        return moveToPlay;
    }
}