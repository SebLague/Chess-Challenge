using System.Collections.Generic;

namespace ChessChallenge.Chess
{
    public static class PGNLoader
    {

        public static Move[] MovesFromPGN(string pgn, int maxPlyCount = int.MaxValue)
        {
            List<string> algebraicMoves = new List<string>();

            string[] entries = pgn.Replace("\n", " ").Split(' ');
            for (int i = 0; i < entries.Length; i++)
            {
                // Reached move limit, so exit.
                // (This is used for example when creating book, where only interested in first n moves of game)
                if (algebraicMoves.Count == maxPlyCount)
                {
                    break;
                }

                string entry = entries[i].Trim();

                if (entry.Contains(".") || entry == "1/2-1/2" || entry == "1-0" || entry == "0-1")
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(entry))
                {
                    algebraicMoves.Add(entry);
                }
            }

            return MovesFromAlgebraic(algebraicMoves.ToArray());
        }

        static Move[] MovesFromAlgebraic(string[] algebraicMoves)
        {
            Board board = new Board();
            board.LoadStartPosition();
            var moves = new List<Move>();

            for (int i = 0; i < algebraicMoves.Length; i++)
            {
                Move move = MoveUtility.GetMoveFromSAN(board, algebraicMoves[i].Trim());
                if (move.IsNull)
                { // move is illegal; discard and return moves up to this point
                    string pgn = "";
                    foreach (string s in algebraicMoves)
                    {
                        pgn += s + " ";
                    }
                    moves.ToArray();
                }
                else
                {
                    moves.Add(move);
                }
                board.MakeMove(move);
            }
            return moves.ToArray();
        }

    }
}