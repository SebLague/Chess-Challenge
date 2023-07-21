using System.Collections;
using System.Collections.Generic;

namespace ChessChallenge.API
{
    /// <summary>
    /// A special list for storing pieces of a particular type and colour
    /// </summary>
    public sealed class PieceList : IEnumerable<Piece>
    {
        public int Count => list.Count;
        public readonly bool IsWhitePieceList;
        public readonly PieceType TypeOfPieceInList;
        public Piece GetPiece(int index) => this[index];

        readonly Chess.PieceList list;
        readonly Board board;

        /// <summary>
        /// Piece List constructor (you shouldn't be creating your own piece lists in
		/// this challenge, but rather accessing the existing lists from the board).
        /// </summary>
        public PieceList(Chess.PieceList list, Board board, int piece)
        {
            this.board = board;
            this.list = list;
            TypeOfPieceInList = (PieceType)Chess.PieceHelper.PieceType(piece);
            IsWhitePieceList = Chess.PieceHelper.IsWhite(piece);
        }


        public Piece this[int index] => board.GetPiece(new Square(list[index]));

        // Allow piece list to be iterated over with 'foreach'
        public IEnumerator<Piece> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return GetPiece(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}