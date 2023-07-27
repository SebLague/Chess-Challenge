using System.Linq;

namespace ChessChallenge.Application.APIHelpers
{
    public static class PieceSquareTableDebugState
    {
        public static bool PieceSquareTableDebugVisualizationRequested { get; set; }
        public static int[] PieceSquareTableToVisualize {get; set;} = Enumerable.Repeat(0, 64).ToArray();
        public static int XORValue {get; set;} = 0;
    }
}
