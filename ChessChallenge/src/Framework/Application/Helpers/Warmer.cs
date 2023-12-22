using ChessEngine;

namespace ChessChallenge.Framework.Application.Helpers
{
    public static class Warmer
    {

        public static void Warm()
        {
            ChessEngine.Internal.Board.Board b = new();
            b.LoadStartPosition();
            Board board = new Board(b);
            Move[] moves = board.GetLegalMoves();

            board.MakeMove(moves[0]);
            board.UndoMove(moves[0]);
        }

    }
}
