using ChessChallenge.API;

namespace ChessChallenge.Application
{
    public static class Warmer
    {

        public static void Warm()
        {
            Chess.Board b = new();
            b.LoadStartPosition();
            Board board = new Board(b);
            Move[] moves = board.GetLegalMoves();

            board.MakeMove(moves[0]);
            board.UndoMove(moves[0]);
        }

    }
}
