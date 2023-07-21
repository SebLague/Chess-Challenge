using System;

namespace ChessChallenge.API
{
    public readonly struct Piece : IEquatable<Piece>
    {
        public readonly bool IsWhite;
        public readonly PieceType PieceType;
        /// <summary>
        /// The square that the piece is on. Note that this value will not be updated if the
        /// piece is moved, it is a snapshot of the state of the piece when it was looked up.
        /// </summary>
        public readonly Square Square;

        public bool IsNull => PieceType is PieceType.None;
        public bool IsRook => PieceType is PieceType.Rook;
        public bool IsKnight => PieceType is PieceType.Knight;
        public bool IsBishop => PieceType is PieceType.Bishop;
        public bool IsQueen => PieceType is PieceType.Queen;
        public bool IsKing => PieceType is PieceType.King;
        public bool IsPawn => PieceType is PieceType.Pawn;

        /// <summary>
        /// Create a piece from its type, colour, and square
        /// </summary>
        public Piece(PieceType pieceType, bool isWhite, Square square)
        {
            PieceType = pieceType;
            Square = square;
            IsWhite = isWhite;
        }

        public override string ToString()
        {
            if (IsNull)
            {
                return "Null";
            }
            string col = IsWhite ? "White" : "Black";
            return $"{col} {PieceType}";
        }

        // Comparisons:
        public static bool operator ==(Piece lhs, Piece rhs) => lhs.Equals(rhs);
        public static bool operator !=(Piece lhs, Piece rhs) => !lhs.Equals(rhs);
        public override bool Equals(object? obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public bool Equals(Piece other)
        {
            return IsWhite == other.IsWhite && PieceType == other.PieceType && Square == other.Square;
        }

    }
}