using System;

namespace ChessChallenge.API
{
    public readonly struct Square : IEquatable<Square>
    {
        /// <summary>
        /// Value from 0 to 7 representing files 'a' to 'h'
        /// </summary>
        public int File => Chess.BoardHelper.FileIndex(Index);
        /// <summary>
		/// Value from 0 to 7 representing ranks '1' to '8'
		/// </summary>
		public int Rank => Chess.BoardHelper.RankIndex(Index);
        /// <summary>
        /// Value from 0 to 63. The values map to the board like so:
		/// 0 – 7 : a1 – h1, 8 – 15 : a2 – h2, ..., 56 – 63 : a8 – h8
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// The algebraic name of the square, e.g. "e4"
        /// </summary>
        public string Name => Chess.BoardHelper.SquareNameFromIndex(Index);

        /// <summary>
        /// Create a square from its algebraic name, e.g. "e4"
        /// </summary>
        public Square(string name)
        {
            Index = Chess.BoardHelper.SquareIndexFromName(name);
        }

        /// <summary>
        /// Create a square from an index [0, 63]
        /// </summary>
        public Square(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Create a square from a file and rank [0, 7]
        /// </summary>
        public Square(int file, int rank)
        {
            Index = Chess.BoardHelper.IndexFromCoord(file, rank);
        }

        public override string ToString()
        {
            return $"'{Name}' (Index = {Index}, File = {File}, Rank = {Rank})";
        }

        // Comparisons
        public bool Equals(Square other) => Index == other.Index;
        public static bool operator ==(Square lhs, Square rhs) => lhs.Equals(rhs);
        public static bool operator !=(Square lhs, Square rhs) => !lhs.Equals(rhs);
        public override bool Equals(object? obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}