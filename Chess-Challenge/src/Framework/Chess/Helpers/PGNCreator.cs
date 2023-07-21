
using System.Text;

namespace ChessChallenge.Chess
{

    public static class PGNCreator
    {

        public static string CreatePGN(Move[] moves)
        {
            return CreatePGN(moves, GameResult.InProgress, FenUtility.StartPositionFEN);
        }

        public static string CreatePGN(Board board, GameResult result, string whiteName = "", string blackName = "")
        {
            return CreatePGN(board.AllGameMoves.ToArray(), result, board.GameStartFen, whiteName, blackName);
        }

        public static string CreatePGN(Move[] moves, GameResult result, string startFen, string whiteName = "", string blackName = "")
        {
            startFen = startFen.Replace("\n", "").Replace("\r", "");

            StringBuilder pgn = new();
            Board board = new Board();
            board.LoadPosition(startFen);
            // Headers
            if (!string.IsNullOrEmpty(whiteName))
            {
                pgn.AppendLine($"[White \"{whiteName}\"]");
            }
            if (!string.IsNullOrEmpty(blackName))
            {
                pgn.AppendLine($"[Black \"{blackName}\"]");
            }

            if (startFen != FenUtility.StartPositionFEN)
            {
                pgn.AppendLine($"[FEN \"{startFen}\"]");
            }
            if (result is not GameResult.NotStarted or GameResult.InProgress)
            {
                pgn.AppendLine($"[Result \"{result}\"]");
            }

            for (int plyCount = 0; plyCount < moves.Length; plyCount++)
            {
                string moveString = MoveUtility.GetMoveNameSAN(moves[plyCount], board);
                board.MakeMove(moves[plyCount]);

                if (plyCount % 2 == 0)
                {
                    pgn.Append((plyCount / 2 + 1) + ". ");
                }
                pgn.Append(moveString + " ");
            }

            return pgn.ToString();
        }

    }
}