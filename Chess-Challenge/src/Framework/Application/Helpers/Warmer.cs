using System;
using ChessChallenge.API;
using ChessChallenge.Application.APIHelpers;

namespace ChessChallenge.Application
{
    public static class Warmer
    {

        public static void Warm()
        {
            Chess.Board b = new();
            b.LoadStartPosition();
            Board board = new Board(b);
            Span<Move> moves = stackalloc Move[APIMoveGen.MaxMoves];
            board.GetLegalMoves(ref moves);

            board.MakeMove(moves[0]);
            board.UndoMove(moves[0]);
        }

    }
}
