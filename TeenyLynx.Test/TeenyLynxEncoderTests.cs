using Lynx;
using Lynx.Model;
using TeenyLynx.Encoder;

namespace TeenyLynx.Test;
public class TeenyLynxEncoderTests
{
    /// <summary>
    /// Checks that from Pawn to Queen positional score bitboards are vertically symmetrical (ax == hx, by == gy, etc.)
    /// </summary>
    [Test]
    public void AdjustLynxPawnPositionScores()
    {
        var lynxPositionalScores = TeenyLynxEncoder.AdjustLynxPawnPositionScores(EvaluationConstants.PositionalScore);

        for (int bIndex = (int)Piece.P; bIndex < (int)Piece.K; ++bIndex)
        {
            VerifySymmetry(lynxPositionalScores, bIndex);
        }

        for (int bIndex = (int)Piece.p; bIndex < (int)Piece.k; ++bIndex)
        {
            VerifySymmetry(lynxPositionalScores, bIndex);
        }
    }

    [Test]
    public void EncodeLynxDataOptimized()
    {
        var testedVersion = TeenyLynxEncoder.EncodeLynxData();
        var optimizedVersion = TeenyLynxEncoder.EncodeLynxDataOptimized();

        var decodedTestedVersion = TeenyLynxEncoder.Decode(testedVersion);
        var decodedOptimizedVersion = TeenyLynxEncoder.DecodeOptimized(optimizedVersion);

        Assert.AreEqual(decodedTestedVersion.Length, decodedOptimizedVersion.Length);

        for (int i = 0; i < decodedOptimizedVersion.Length; ++i)
        {
            // We exclude the items that are adjusted in TeenyLynxEncoder.AdjustLynxPawnPositionScores
            if (
                i != 64 * (int)Piece.P + ((int)BoardSquare.c4 ^ 56)
                && i != 64 * (int)Piece.P + ((int)BoardSquare.f4 ^ 56)
                && i != 64 * (int)Piece.P + ((int)BoardSquare.f6 ^ 56)
                && i != 64 * (int)Piece.Q + ((int)BoardSquare.f6 ^ 56)
                )
            {
                var expected = decodedTestedVersion[i];
                var current = decodedOptimizedVersion[i];
                Assert.AreEqual(expected, current);
            }
        }
    }

    private static void VerifySymmetry(int[][] lynxPositionalScores, int bIndex)
    {
        var bitboard = lynxPositionalScores[bIndex];
        Assert.AreEqual(64, bitboard.Length);

        for (int i = 0; i < bitboard.Length - 1; i += 8)
        {
            for (int j = 0; j < 4; ++j)
            {
                Assert.AreEqual(bitboard[i + j], bitboard[i + 7 - j],
                    $"Bitboard {(Piece)bIndex}: " +
                    $"{(BoardSquare)(i + j)} ({bitboard[i + j]}) != {(BoardSquare)(i + 7 - j)} ({bitboard[i + 7 - j]})");
            }
        }
    }
}
