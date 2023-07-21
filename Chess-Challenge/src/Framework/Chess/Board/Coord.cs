using System;
namespace ChessChallenge.Chess
{
    // Structure for representing squares on the chess board as file/rank integer pairs.
    // (0, 0) = a1, (7, 7) = h8.
    // Coords can also be used as offsets. For example, while a Coord of (-1, 0) is not
    // a valid square, it can be used to represent the concept of moving 1 square left.

    public readonly struct Coord : IComparable<Coord>
    {
        public readonly int fileIndex;
        public readonly int rankIndex;

        public Coord(int fileIndex, int rankIndex)
        {
            this.fileIndex = fileIndex;
            this.rankIndex = rankIndex;
        }

        public Coord(int squareIndex)
        {
            this.fileIndex = BoardHelper.FileIndex(squareIndex);
            this.rankIndex = BoardHelper.RankIndex(squareIndex);
        }

        public bool IsLightSquare()
        {
            return (fileIndex + rankIndex) % 2 != 0;
        }

        public int CompareTo(Coord other)
        {
            return (fileIndex == other.fileIndex && rankIndex == other.rankIndex) ? 0 : 1;
        }

        public static Coord operator +(Coord a, Coord b) => new Coord(a.fileIndex + b.fileIndex, a.rankIndex + b.rankIndex);
        public static Coord operator -(Coord a, Coord b) => new Coord(a.fileIndex - b.fileIndex, a.rankIndex - b.rankIndex);
        public static Coord operator *(Coord a, int m) => new Coord(a.fileIndex * m, a.rankIndex * m);
        public static Coord operator *(int m, Coord a) => a * m;

        public bool IsValidSquare() => fileIndex >= 0 && fileIndex < 8 && rankIndex >= 0 && rankIndex < 8;
        public int SquareIndex => BoardHelper.IndexFromCoord(this);
    }
}