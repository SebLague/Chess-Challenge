/*
 *
 *             (Victims)   Pawn Knight Bishop  Rook Queen King
 * (Attackers)
 * 	    Pawn			     6    16     26    36    46    56
 *     Knight                5    15     25    35    45    55
 *     Bishop                4    14     24    34    44    54
 *       Rook                3    13     23    33    43    53
 *      Queen                2    12     22    32    42    52
 *       King                1    11     21    31    41    51
 *
 *
 */

using ChessChallenge.API;

namespace TeenyLynx.Test;

public class MVVLVATests
{
    [TestCase(PieceType.Pawn, PieceType.Pawn, 6)]
    [TestCase(PieceType.Pawn, PieceType.Knight, 16)]
    [TestCase(PieceType.Pawn, PieceType.Bishop, 26)]
    [TestCase(PieceType.Pawn, PieceType.Rook, 36)]
    [TestCase(PieceType.Pawn, PieceType.Queen, 46)]
    [TestCase(PieceType.Pawn, PieceType.King, 56)]

    [TestCase(PieceType.Knight, PieceType.Pawn, 5)]
    [TestCase(PieceType.Knight, PieceType.Knight, 15)]
    [TestCase(PieceType.Knight, PieceType.Bishop, 25)]
    [TestCase(PieceType.Knight, PieceType.Rook, 35)]
    [TestCase(PieceType.Knight, PieceType.Queen, 45)]
    [TestCase(PieceType.Knight, PieceType.King, 55)]

    [TestCase(PieceType.Bishop, PieceType.Pawn, 4)]
    [TestCase(PieceType.Bishop, PieceType.Knight, 14)]
    [TestCase(PieceType.Bishop, PieceType.Bishop, 24)]
    [TestCase(PieceType.Bishop, PieceType.Rook, 34)]
    [TestCase(PieceType.Bishop, PieceType.Queen, 44)]
    [TestCase(PieceType.Bishop, PieceType.King, 54)]

    [TestCase(PieceType.Rook, PieceType.Pawn, 3)]
    [TestCase(PieceType.Rook, PieceType.Knight, 13)]
    [TestCase(PieceType.Rook, PieceType.Bishop, 23)]
    [TestCase(PieceType.Rook, PieceType.Rook, 33)]
    [TestCase(PieceType.Rook, PieceType.Queen, 43)]
    [TestCase(PieceType.Rook, PieceType.King, 53)]

    [TestCase(PieceType.Queen, PieceType.Pawn, 2)]
    [TestCase(PieceType.Queen, PieceType.Knight, 12)]
    [TestCase(PieceType.Queen, PieceType.Bishop, 22)]
    [TestCase(PieceType.Queen, PieceType.Rook, 32)]
    [TestCase(PieceType.Queen, PieceType.Queen, 42)]
    [TestCase(PieceType.Queen, PieceType.King, 52)]

    [TestCase(PieceType.King, PieceType.Pawn, 1)]
    [TestCase(PieceType.King, PieceType.Knight, 11)]
    [TestCase(PieceType.King, PieceType.Bishop, 21)]
    [TestCase(PieceType.King, PieceType.Rook, 31)]
    [TestCase(PieceType.King, PieceType.Queen, 41)]
    [TestCase(PieceType.King, PieceType.King, 51)]
    public void MVVLVA(PieceType sourcePiece, PieceType targetPiece, int expectedValue)
    {
        Assert.AreEqual(expectedValue,
            //MyBot.Magic[441 + (int)targetPiece + 6 * (int)sourcePiece]);
            new MyBot().Magic[441 + (int)targetPiece + 6 * (int)sourcePiece]);
    }
}
