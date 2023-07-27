
namespace ChessChallenge.API
{
    using ChessChallenge.Application.APIHelpers;
    using ChessChallenge.Chess;

    /// <summary>
    /// Helper class for working with bitboards.
    /// Bitboards are represented with the ulong type (unsigned 64 bit integer).
    /// </summary>
    public static class BitboardHelper
    {
        /// <summary>
        /// Set the given square on the bitboard to 1.
        /// </summary>
		public static void SetSquare(ref ulong bitboard, Square square)
        {
            bitboard |= 1ul << square.Index;
        }
        /// <summary>
        /// Clear the given square on the bitboard to 0.
        /// </summary>
        public static void ClearSquare(ref ulong bitboard, Square square)
        {
            bitboard &= ~(1ul << square.Index);
        }

        /// <summary>
        /// Toggle the given square on the bitboard between 0 and 1.
        /// </summary>
        public static void ToggleSquare(ref ulong bitboard, Square square)
        {
            bitboard ^= 1ul << square.Index;
        }

        /// <summary>
        /// Returns true if the given square is set to 1 on the bitboard, otherwise false.
        /// </summary>
        public static bool SquareIsSet(ulong bitboard, Square square)
        {
            return ((bitboard >> square.Index) & 1) != 0;
        }

        public static int ClearAndGetIndexOfLSB(ref ulong bitboard)
        {
            return BitBoardUtility.PopLSB(ref bitboard);
        }

        public static int GetNumberOfSetBits(ulong bitboard)
        {
            return BitBoardUtility.PopCount(bitboard);
        }

        /// <summary>
        /// Returns a bitboard where each bit that is set to 1 represents a square that the given
        /// sliding piece type is able to attack. These attacks are calculated from the given square,
        /// and take the given board state into account (so attacks will be blocked by pieces that are in the way).
        /// Valid only for sliding piece types (queen, rook, and bishop).
        /// </summary>
        public static ulong GetSliderAttacks(PieceType pieceType, Square square, Board board)
        {
            return pieceType switch
            {
                PieceType.Rook => GetRookAttacks(square, board.AllPiecesBitboard),
                PieceType.Bishop => GetBishopAttacks(square, board.AllPiecesBitboard),
                PieceType.Queen => GetQueenAttacks(square, board.AllPiecesBitboard),
                _ => 0
            };
        }

        /// <summary>
        /// Returns a bitboard where each bit that is set to 1 represents a square that the given
        /// sliding piece type is able to attack. These attacks are calculated from the given square,
        /// and take the given blocker bitboard into account (so attacks will be blocked by pieces that are in the way).
        /// Valid only for sliding piece types (queen, rook, and bishop).
        /// </summary>
        public static ulong GetSliderAttacks(PieceType pieceType, Square square, ulong blockers)
        {
            return pieceType switch
            {
                PieceType.Rook => GetRookAttacks(square, blockers),
                PieceType.Bishop => GetBishopAttacks(square, blockers),
                PieceType.Queen => GetQueenAttacks(square, blockers),
                _ => 0
            };
        }
        /// <summary>
        /// Gets a bitboard of squares that a knight can attack from the given square.
        /// </summary>
        public static ulong GetKnightAttacks(Square square) => Bits.KnightAttacks[square.Index];
        /// <summary>
        /// Gets a bitboard of squares that a king can attack from the given square.
        /// </summary>
        public static ulong GetKingAttacks(Square square) => Bits.KingMoves[square.Index];

        /// <summary>
        /// Gets a bitboard of squares that a pawn (of the given colour) can attack from the given square.
        /// </summary>
        public static ulong GetPawnAttacks(Square square, bool isWhite)
        {
            return isWhite ? Bits.WhitePawnAttacks[square.Index] : Bits.BlackPawnAttacks[square.Index];
        }

        /// <summary>
        /// A debug function for visualizing bitboards.
        /// Highlights the squares that are set to 1 in the given bitboard with a red colour.
        /// Highlights the squares that are set to 0 in the given bitboard with a blue colour.
        /// </summary>
        public static void VisualizeBitboard(ulong bitboard)
        {
            BitboardDebugState.BitboardDebugVisualizationRequested = true;
            BitboardDebugState.BitboardToVisualize = bitboard;
        }

        /// <summary>
        /// Clears the bitboard debug visualization
        /// </summary>
        public static void StopVisualizingBitboard() => BitboardDebugState.BitboardDebugVisualizationRequested = false;

        static ulong GetRookAttacks(Square square, ulong blockers)
        {
            ulong mask = Magic.RookMask[square.Index];
            ulong magic = PrecomputedMagics.RookMagics[square.Index];
            int shift = PrecomputedMagics.RookShifts[square.Index];

            ulong key = ((blockers & mask) * magic) >> shift;
            return Magic.RookAttacks[square.Index][key];
        }

        static ulong GetBishopAttacks(Square square, ulong blockers)
        {
            ulong mask = Magic.BishopMask[square.Index];
            ulong magic = PrecomputedMagics.BishopMagics[square.Index];
            int shift = PrecomputedMagics.BishopShifts[square.Index];

            ulong key = ((blockers & mask) * magic) >> shift;
            return Magic.BishopAttacks[square.Index][key];
        }

        static ulong GetQueenAttacks(Square square, ulong blockers)
        {
            return GetRookAttacks(square, blockers) | GetBishopAttacks(square, blockers);
        }
    }
}