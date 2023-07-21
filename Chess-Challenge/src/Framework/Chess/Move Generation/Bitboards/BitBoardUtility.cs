namespace ChessChallenge.Chess
{
    using System.Numerics;

    public static class BitBoardUtility
    {

        // Get index of least significant set bit in given 64bit value. Also clears the bit to zero.
        public static int PopLSB(ref ulong b)
        {
            int i = BitOperations.TrailingZeroCount(b);
            b &= (b - 1);
            return i;
        }

        public static int PopCount(ulong x)
        {
            return BitOperations.PopCount(x);
        }

        public static void SetSquare(ref ulong bitboard, int squareIndex)
        {
            bitboard |= 1ul << squareIndex;
        }

        public static void ClearSquare(ref ulong bitboard, int squareIndex)
        {
            bitboard &= ~(1ul << squareIndex);
        }


        public static void ToggleSquare(ref ulong bitboard, int squareIndex)
        {
            bitboard ^= 1ul << squareIndex;
        }

        public static void ToggleSquares(ref ulong bitboard, int squareA, int squareB)
        {
            bitboard ^= (1ul << squareA | 1ul << squareB);
        }

        public static bool ContainsSquare(ulong bitboard, int square)
        {
            return ((bitboard >> square) & 1) != 0;
        }

        public static ulong PawnAttacks(ulong pawnBitboard, bool isWhite)
        {
            // Pawn attacks are calculated like so: (example given with white to move)

            // The first half of the attacks are calculated by shifting all pawns north-east: northEastAttacks = pawnBitboard << 9
            // Note that pawns on the h file will be wrapped around to the a file, so then mask out the a file: northEastAttacks &= notAFile
            // (Any pawns that were originally on the a file will have been shifted to the b file, so a file should be empty).

            // The other half of the attacks are calculated by shifting all pawns north-west. This time the h file must be masked out.
            // Combine the two halves to get a bitboard with all the pawn attacks: northEastAttacks | northWestAttacks

            if (isWhite)
            {
                return ((pawnBitboard << 9) & Bits.NotAFile) | ((pawnBitboard << 7) & Bits.NotHFile);
            }

            return ((pawnBitboard >> 7) & Bits.NotAFile) | ((pawnBitboard >> 9) & Bits.NotHFile);
        }


        public static ulong Shift(ulong bitboard, int numSquaresToShift)
        {
            if (numSquaresToShift > 0)
            {
                return bitboard << numSquaresToShift;
            }
            else
            {
                return bitboard >> -numSquaresToShift;
            }

        }

        public static ulong ProtectedPawns(ulong pawns, bool isWhite)
        {
            ulong attacks = PawnAttacks(pawns, isWhite);
            return attacks & pawns;
        }

        public static ulong LockedPawns(ulong whitePawns, ulong blackPawns)
        {
            ulong pushUp = whitePawns << 8;
            ulong pushDown = blackPawns >> 8;
            return (whitePawns & pushDown) | (blackPawns & pushUp);
        }


        static BitBoardUtility()
        {

        }


    }
}