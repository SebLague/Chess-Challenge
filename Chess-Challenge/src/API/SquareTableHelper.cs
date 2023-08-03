using System;
using System.Linq;

namespace ChessChallenge.API
{
    using ChessChallenge.Application.APIHelpers;

    /// <summary>
    /// Helper class for debugging Piece-Square-Tables.
    /// </summary>
    public static class SquareTableHelper
    {
        /// <summary>
        /// A debug function for visualizing Piece-Square-Tables.
        /// </summary>
        public static void VisualizeSquareTable(int[] squareTable, int? XORValue = 0)
        {
            SquareTableDebugState.Floating = false;
            SquareTableDebugState.SquareTableDebugVisualizationRequested = true;
            SquareTableDebugState.SquareTableToVisualize = squareTable.Select(value => (float)value).ToArray();
            SquareTableDebugState.XORValue = XORValue ?? 0;
        }
        public static void VisualizeSquareTable(float[] squareTable, int? XORValue = 0)
        {
            SquareTableDebugState.Floating = true;
            SquareTableDebugState.SquareTableDebugVisualizationRequested = true;
            SquareTableDebugState.SquareTableToVisualize = squareTable;
            SquareTableDebugState.XORValue = XORValue ?? 0;

        }

        /// <summary>
        /// Clears the Piece-Square-Table debug visualization
        /// </summary>
        public static void StopVisualizingSquareTable() => SquareTableDebugState.SquareTableDebugVisualizationRequested = false;

        /// <summary>
        /// Returns the squareTable with the values normalized to the range [-128, 127].
        /// 0 is mapped to 0, min is mapped to -128 or max is mapped to 127, whichever makes the whole range fit
        /// </summary>
        public static int[] NormalizeTable(int[] squareTable, int min, int max)
        {
            double upperFactor = max > 0 ? 127d / max : 1;
            double lowerFactor = min < 0 ? -128d / min : 1;
            double factor = Math.Max(upperFactor, lowerFactor);
            if (factor * max > 127 || factor * min < -128)
            {
                factor = Math.Min(upperFactor, lowerFactor);
            }
            return squareTable.Select(value => (int)Math.Round(factor * value)).ToArray();

        }

        public static ulong[] EncodeTable(int[] table, int XORValue = 0)
        {
            ulong[] encodedTable = new ulong[table.Length / 8];
            for (int rank = 0; rank < table.Length / 8; rank++)
            {
                ulong row = 0;
                for (int file = 0; file < 8; file++)
                {
                    int index = (rank * 8 + file) ^ XORValue;
                    int value = table[index];
                    row = (row >> 8) | ((ulong)(byte)value << 56);
                }
                encodedTable[rank] = row;
            }
            return encodedTable;
        }

        public static int[] DecodeTable(ulong[] table)
        {
            return table
              .Aggregate(new int[0], (decoded, rank) =>
              {
                  return decoded.Concat(
                      Enumerable.Range(0, 8)
                          .Select(file =>
                          {
                              return (int)(sbyte)((rank & (255UL << 8 * file)) >> 8 * file);
                          })
                  ).ToArray();
              });
        }

        public static string GetBinaryString(ulong value)
        {
            string binaryString = Convert //
              .ToString((long)value, 2) //
              .PadLeft(64, '0') //
              .Select((b, i) => (i % 8 == 0) ? $"_{b}" : $"{b}") //
              .Aggregate((acc, b) => acc + b);
            return $"0b{binaryString}";
        }

        public static string GetHexString(ulong value)
        {
          string hexString = Convert //
            .ToString((long)value, 16) //
            .PadLeft(16, '0');
          return $"0x{hexString}";
        }

    }
}
