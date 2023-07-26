using ChessChallenge.API;
using Lynx;

namespace TeenyLynx.Test;

/// <summary>
/// Chosen having a look at different parts of <see cref="EvaluationConstants.PositionalScore"/>
/// </summary>
public class PositionalScoresTests
{
    [TestCase("a7", PieceType.Pawn, true, 40)]
    [TestCase("a7", PieceType.Pawn, false, 0)]
    [TestCase("a7", PieceType.Knight, true, -15)]
    [TestCase("a7", PieceType.Knight, false, -15)]
    [TestCase("a7", PieceType.Bishop, true, -5)]
    [TestCase("a7", PieceType.Bishop, false, -5)]
    [TestCase("a7", PieceType.Rook, true, 70)]
    [TestCase("a7", PieceType.Rook, false, 0)]
    [TestCase("a7", PieceType.Queen, true, -10)]
    [TestCase("a7", PieceType.Queen, false, -10)]
    [TestCase("a7", PieceType.King, true, -50)]
    [TestCase("a7", PieceType.King, false, 15)]

    [TestCase("d8", PieceType.Pawn, true, 90)]
    [TestCase("d8", PieceType.Pawn, false, 0)]
    [TestCase("d8", PieceType.Knight, true, -3)]
    [TestCase("d8", PieceType.Knight, false, -5)]
    [TestCase("d8", PieceType.Bishop, true, -10)]
    [TestCase("d8", PieceType.Bishop, false, -10)]
    [TestCase("d8", PieceType.Rook, true, 50)]
    [TestCase("d8", PieceType.Rook, false, 20)]
    [TestCase("d8", PieceType.Queen, true, 5)]
    [TestCase("d8", PieceType.Queen, false, 0)]
    [TestCase("d8", PieceType.King, true, -50)]
    [TestCase("d8", PieceType.King, false, -10)]

    [TestCase("e6", PieceType.Pawn, true, 30)]
    [TestCase("e6", PieceType.Pawn, false, 10)]
    [TestCase("e6", PieceType.Knight, true, 20)]
    [TestCase("e6", PieceType.Knight, false, 10)]
    [TestCase("e6", PieceType.Bishop, true, 10)]
    [TestCase("e6", PieceType.Bishop, false, 10)]
    [TestCase("e6", PieceType.Rook, true, 20)]
    [TestCase("e6", PieceType.Rook, false, 20)]
    [TestCase("e6", PieceType.Queen, true, 10)]
    [TestCase("e6", PieceType.Queen, false, 10)]
    [TestCase("e6", PieceType.King, true, -50)]
    [TestCase("e6", PieceType.King, false, -50)]

    [TestCase("c8", PieceType.Pawn, true, 90)]
    [TestCase("c8", PieceType.Pawn, false, 0)]
    [TestCase("c8", PieceType.Knight, true, -10)]
    [TestCase("c8", PieceType.Knight, false, -10)]
    [TestCase("c8", PieceType.Bishop, true, -10)]
    [TestCase("c8", PieceType.Bishop, false, -20)]
    [TestCase("c8", PieceType.Rook, true, 50)]
    [TestCase("c8", PieceType.Rook, false, 5)]
    [TestCase("c8", PieceType.Queen, true, 5)]
    [TestCase("c8", PieceType.Queen, false, -5)]
    [TestCase("c8", PieceType.King, true, -50)]
    [TestCase("c8", PieceType.King, false, 20)]

    [TestCase("h8", PieceType.Pawn, true, 90)]
    [TestCase("h8", PieceType.Pawn, false, 0)]
    [TestCase("h8", PieceType.Knight, true, -60)]
    [TestCase("h8", PieceType.Knight, false, -30)]
    [TestCase("h8", PieceType.Bishop, true, 0)]
    [TestCase("h8", PieceType.Bishop, false, 0)]
    [TestCase("h8", PieceType.Rook, true, 50)]
    [TestCase("h8", PieceType.Rook, false, 5)]
    [TestCase("h8", PieceType.Queen, true, -10)]
    [TestCase("h8", PieceType.Queen, false, -10)]
    [TestCase("h8", PieceType.King, true, -50)]
    [TestCase("h8", PieceType.King, false, 30)]

    public void PositionalScores(string squareString, PieceType piece, bool isWhite, int expectedValue)
    {
        int coefficient = 1;
        var squareIndex = new Square(squareString).Index;
        int teenyLynxIndex = squareIndex;

        if (!isWhite)
        {
            expectedValue *= -1;        // We expect a negative value, that favours black
            teenyLynxIndex ^= 56;       // We reverse rows
            coefficient = -1;           // Actual operation to do
        }

        Assert.AreEqual(
            expectedValue,
            //coefficient * MyBot.Magic[teenyLynxIndex + 64 * ((int)piece - 1)]);
            coefficient * new MyBot().Magic[teenyLynxIndex + 64 * ((int)piece - 1)]);

        var lynxOffset = Utils.PieceOffset(isWhite ? Lynx.Model.Side.White : Lynx.Model.Side.Black);
        var lynxSquareIndex = squareIndex ^ 56;
        Assert.AreEqual(squareString, ((Lynx.Model.BoardSquare)lynxSquareIndex).ToString());

        var lynxValue = EvaluationConstants.PositionalScore[(int)piece - 1 + lynxOffset][lynxSquareIndex];

        Assert.AreEqual(expectedValue, lynxValue);
    }

    [TestCase("a7", true, 5)]
    [TestCase("a7", false, 5)]
    [TestCase("b7", true, 10)]
    [TestCase("b7", false, 10)]
    [TestCase("d8", true, 12)]
    [TestCase("d8", false, 12)]
    [TestCase("d7", true, 20)]
    [TestCase("d7", false, 20)]
    [TestCase("e6", true, 30)]
    [TestCase("e6", false, 30)]
    [TestCase("d5", true, 50)]
    [TestCase("d5", false, 50)]
    [TestCase("f7", true, 15)]
    [TestCase("f7", false, 15)]
    public void KingEndgamePositionalScores(string squareString, bool isWhite, int expectedValue)
    {
        int coefficient = 1;
        var squareIndex = new Square(squareString).Index;
        int teenyLynxIndex = squareIndex;

        if (!isWhite)
        {
            expectedValue *= -1;        // We expect a negative value, that favours black
            teenyLynxIndex ^= 56;       // We reverse rows
            coefficient = -1;           // Actual operation to do
        }

        Assert.AreEqual(
            expectedValue,
            //coefficient * MyBot.Magic[teenyLynxIndex + 64 * 6]);
            coefficient * new MyBot().Magic[teenyLynxIndex + 64 * 6]);

        var lynxOffset = Utils.PieceOffset(isWhite ? Lynx.Model.Side.White : Lynx.Model.Side.Black);
        var lynxSquare = squareIndex ^ 56;
        Assert.AreEqual(squareString, ((Lynx.Model.BoardSquare)lynxSquare).ToString());

        var lynxValue = EvaluationConstants.EndgamePositionalScore[(int)Lynx.Model.Piece.K + lynxOffset][lynxSquare];

        Assert.AreEqual(expectedValue, lynxValue);
    }
}
