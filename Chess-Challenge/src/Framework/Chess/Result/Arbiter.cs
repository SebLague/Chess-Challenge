namespace ChessChallenge.Chess
{
    using System.Linq;

    public static class Arbiter
    {
        public static bool IsDrawResult(GameResult result)
        {
            return result is GameResult.DrawByArbiter or GameResult.FiftyMoveRule or
                GameResult.Repetition or GameResult.Stalemate or GameResult.InsufficientMaterial;
        }

        public static bool IsWinResult(GameResult result)
        {
            return IsWhiteWinsResult(result) || IsBlackWinsResult(result);
        }

        public static bool IsWhiteWinsResult(GameResult result)
        {
            return result is GameResult.BlackIsMated or GameResult.BlackTimeout or GameResult.BlackIllegalMove;
        }

        public static bool IsBlackWinsResult(GameResult result)
        {
            return result is GameResult.WhiteIsMated or GameResult.WhiteTimeout or GameResult.WhiteIllegalMove;
        }


        public static GameResult GetGameState(Board board)
        {
            MoveGenerator moveGenerator = new MoveGenerator();
            var moves = moveGenerator.GenerateMoves(board);

            // Look for mate/stalemate
            if (moves.Length == 0)
            {
                if (moveGenerator.InCheck())
                {
                    return (board.IsWhiteToMove) ? GameResult.WhiteIsMated : GameResult.BlackIsMated;
                }
                return GameResult.Stalemate;
            }

            // Fifty move rule
            if (board.currentGameState.fiftyMoveCounter >= 100)
            {
                return GameResult.FiftyMoveRule;
            }

            // Threefold repetition
            int repCount = board.RepetitionPositionHistory.Count((x => x == board.currentGameState.zobristKey));
            if (repCount == 3)
            {
                return GameResult.Repetition;
            }

            // Look for insufficient material
            if (InsufficentMaterial(board))
            {
                return GameResult.InsufficientMaterial;
            }
            return GameResult.InProgress;
        }

        // Test for insufficient material (Note: not all cases are implemented)
        public static bool InsufficentMaterial(Board board)
        {
            // Can't have insufficient material with pawns on the board
            if (board.pawns[Board.WhiteIndex].Count > 0 || board.pawns[Board.BlackIndex].Count > 0)
            {
                return false;
            }

            // Can't have insufficient material with queens/rooks on the board
            if (board.FriendlyOrthogonalSliders != 0 || board.EnemyOrthogonalSliders != 0)
            {
                return false;
            }

            // If no pawns, queens, or rooks on the board, then consider knight and bishop cases
            int numWhiteBishops = board.bishops[Board.WhiteIndex].Count;
            int numBlackBishops = board.bishops[Board.BlackIndex].Count;
            int numWhiteKnights = board.knights[Board.WhiteIndex].Count;
            int numBlackKnights = board.knights[Board.BlackIndex].Count;
            int numWhiteMinors = numWhiteBishops + numWhiteKnights;
            int numBlackMinors = numBlackBishops + numBlackKnights;

            // King v King
            if (numWhiteMinors == 0 && numBlackMinors == 0)
            {
                return true;
            }

            // Single minor piece vs lone king
            if ((numWhiteMinors == 1 && numBlackMinors == 0) || (numBlackMinors == 1 && numWhiteMinors == 0))
            {
                return true;
            }


            return false;


        }
    }
}