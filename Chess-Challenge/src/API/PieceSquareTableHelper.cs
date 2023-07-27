
namespace ChessChallenge.API
{
    using ChessChallenge.Application.APIHelpers;

    /// <summary>
    /// Helper class for debugging Piece-Square-Tables.
    /// </summary>
    public static class PieceSquareTableHelper
    {
        /// <summary>
        /// A debug function for visualizing Piece-Square-Tables.
        /// </summary>
        public static void VisualizePieceSquareTable(int[] pieceSquareTable, int? XORValue=0)
        {
            PieceSquareTableDebugState.PieceSquareTableDebugVisualizationRequested = true;
            PieceSquareTableDebugState.PieceSquareTableToVisualize = pieceSquareTable;
            PieceSquareTableDebugState.XORValue = XORValue ?? 0;
        }

        /// <summary>
        /// Clears the Piece-Square-Table debug visualization
        /// </summary>
        public static void StopVisualizingPieceSquareTable() => PieceSquareTableDebugState.PieceSquareTableDebugVisualizationRequested = false;

    }
}
