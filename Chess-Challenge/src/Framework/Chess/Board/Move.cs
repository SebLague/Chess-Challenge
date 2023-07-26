/*
Compact (16bit) move representation to preserve memory during search.

The format is as follows (ffffttttttssssss)
Bits 0-5: start square index
Bits 6-11: target square index
Bits 12-15: flag (promotion type, etc)
*/
namespace ChessChallenge.Chess
{
    public readonly struct Move
    {
        // 16bit move value
        readonly ushort moveValue;

        // Flags
        public const int NoFlag = 0b0000;
        public const int EnPassantCaptureFlag = 0b0001;
        public const int CastleFlag = 0b0010;
        public const int PawnTwoUpFlag = 0b0011;

        public const int PromoteToQueenFlag = 0b0100;
        public const int PromoteToKnightFlag = 0b0101;
        public const int PromoteToRookFlag = 0b0110;
        public const int PromoteToBishopFlag = 0b0111;

        // Masks
        const ushort startSquareMask = 0b0000000000111111;
        const ushort targetSquareMask = 0b0000111111000000;
        const ushort flagMask = 0b1111000000000000;

        public Move(ushort moveValue)
        {
            this.moveValue = moveValue;
        }

        public Move(int startSquare, int targetSquare)
        {
            moveValue = (ushort)(startSquare | targetSquare << 6);
        }

        public Move(int startSquare, int targetSquare, int flag)
        {
            moveValue = (ushort)(startSquare | targetSquare << 6 | flag << 12);
        }

        public ushort Value => moveValue;
        public bool IsNull => moveValue == 0;

        public int StartSquareIndex => moveValue & startSquareMask;
        public int TargetSquareIndex => (moveValue & targetSquareMask) >> 6;
        public bool IsPromotion => MoveFlag >= PromoteToQueenFlag;
        public bool IsEnPassant => MoveFlag == EnPassantCaptureFlag;
        public int MoveFlag => moveValue >> 12;

        public int PromotionPieceType
        {
            get
            {
                switch (MoveFlag)
                {
                    case PromoteToRookFlag:
                        return PieceHelper.Rook;
                    case PromoteToKnightFlag:
                        return PieceHelper.Knight;
                    case PromoteToBishopFlag:
                        return PieceHelper.Bishop;
                    case PromoteToQueenFlag:
                        return PieceHelper.Queen;
                    default:
                        return PieceHelper.None;
                }
            }
        }

        public static Move NullMove => new Move(0);
        public static bool SameMove(Move a, Move b) => a.moveValue == b.moveValue;


    }
}