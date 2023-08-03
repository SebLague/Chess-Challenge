using System.Linq;

namespace ChessChallenge.Application.APIHelpers
{
    public static class SquareTableDebugState
    {
        public static bool SquareTableDebugVisualizationRequested { get; set; }
        public static float[] SquareTableToVisualize {get; set;} = Enumerable.Repeat(0f, 64).ToArray();
        public static bool Floating = false;
        public static int XORValue {get; set;} = 0;
    }
}
