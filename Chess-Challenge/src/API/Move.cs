using ChessChallenge.Chess;
using System;

namespace ChessChallenge.API
{
	public readonly struct Move : IEquatable<Move>
	{
		public Square StartSquare => new Square(move.StartSquareIndex);
		public Square TargetSquare => new Square(move.TargetSquareIndex);
		public PieceType MovePieceType => (PieceType)(pieceTypeData & 0b111);
		public PieceType CapturePieceType => (PieceType)(pieceTypeData >> 3);
		public PieceType PromotionPieceType => (PieceType)move.PromotionPieceType;
		public bool IsCapture => (pieceTypeData >> 3) != 0;
		public bool IsEnPassant => move.MoveFlag == Chess.Move.EnPassantCaptureFlag;

		public bool IsPromotion => move.IsPromotion;
		public bool IsCastles => move.MoveFlag == Chess.Move.CastleFlag;
		public bool IsNull => move.IsNull;
		public ushort RawValue => move.Value;
		public static readonly Move NullMove = new();

		readonly Chess.Move move;
		readonly ushort pieceTypeData;

		/// <summary>
		/// Create a null/invalid move.
		/// This is simply an invalid move that can be used as a placeholder until a valid move has been found
		/// </summary>
		public Move()
		{
			move = Chess.Move.NullMove;
			pieceTypeData = 0;
		}

		/// <summary>
		/// Create a move from UCI notation, for example: "e2e4" to move a piece from e2 to e4.
		/// If promoting, piece type must be included, for example: "d7d8q".
		/// </summary>
        public Move(string moveName, Board board)
        {
			var data = Application.APIHelpers.MoveHelper.CreateMoveFromName(moveName, board);
			move = data.move;
			pieceTypeData = (ushort)((int)data.pieceType | ((int)data.captureType << 3));

        }

        /// <summary>
        /// Internal move constructor. Do not use.
        /// </summary>
        public Move(Chess.Move move, int movePieceType, int capturePieceType)
		{
			this.move = move;
			pieceTypeData = (ushort)(movePieceType | (capturePieceType << 3));
		}

		public override string ToString()
		{
			string moveName = MoveUtility.GetMoveNameUCI(move);
			return $"Move: '{moveName}'";
		}

		/// <summary>
		/// Tests if two moves are the same.
		/// This is true if they move to/from the same square, and move/capture/promote the same piece type
		/// </summary>
		public bool Equals(Move other)
		{
			return RawValue == other.RawValue && pieceTypeData == other.pieceTypeData;
		}

		public static bool operator ==(Move lhs, Move rhs) => lhs.Equals(rhs);
		public static bool operator !=(Move lhs, Move rhs) => !lhs.Equals(rhs);
		public override bool Equals(object? obj) => base.Equals(obj);
		public override int GetHashCode() => RawValue;
	}
}