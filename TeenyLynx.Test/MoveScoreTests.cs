// Based on https://github.com/lynx-chess/Lynx/blob/main/tests/Lynx.Test/Model/MoveScoreTest.cs
using Lynx;
using ChessChallenge.API;

namespace TeenyLynx.Test;

public class MoveScoreTest : BaseTest
{
    /// <summary>
    /// 'Tricky position'
    /// 8   r . . . k . . r
    /// 7   p . p p q p b .
    /// 6   b n . . p n p .
    /// 5   . . . P N . . .
    /// 4   . p . . P . . .
    /// 3   . . N . . Q . p
    /// 2   P P P B B P P P
    /// 1   R . . . K . . R
    ///     a b c d e f g h
    /// This tests indirectly <see cref="EvaluationConstants.MostValueableVictimLeastValuableAttacker"/>
    /// </summary>
    /// <param name="fen"></param>
    [TestCase(Constants.TrickyTestPositionFEN)]
    public void MoveScore(string fen)
    {
        var bot = GetBot(fen);

        var allMoves = bot._position.GetLegalMoves().OrderByDescending(move => bot.Score(move, default)).ToList();

        Assert.AreEqual("e2a6", allMoves[0].ToString()[7..^1]);     // BxB
        Assert.AreEqual("f3f6", allMoves[1].ToString()[7..^1]);     // QxN
        Assert.AreEqual("d5e6", allMoves[3].ToString()[7..^1]);     // PxP
        Assert.AreEqual("g2h3", allMoves[2].ToString()[7..^1]);     // PxP
        Assert.AreEqual("e5d7", allMoves[5].ToString()[7..^1]);     // NxP
        Assert.AreEqual("e5f7", allMoves[6].ToString()[7..^1]);     // NxP
        Assert.AreEqual("e5g6", allMoves[4].ToString()[7..^1]);     // NxP
        Assert.AreEqual("f3h3", allMoves[7].ToString()[7..^1]);     // QxP

        foreach (var move in allMoves.Where(move => !move.IsCapture && !move.IsCastles))
        {
            Assert.AreEqual(0, bot.Score(move, default));
        }
    }

    /// <summary>
    /// Only one capture, en passant, both sides
    /// 8   r n b q k b n r
    /// 7   p p p . p p p p
    /// 6   . . . . . . . .
    /// 5   . . . p P . . .
    /// 4   . . . . . . . .
    /// 3   . . . . . . . .
    /// 2   P P P P . P P P
    /// 1   R N B Q K B N R
    ///     a b c d e f g h
    /// </summary>
    /// <param name="fen"></param>
    /// <param name="moveWithHighestScore"></param>
    [TestCase("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 1", "e5d6")]
    [TestCase("rnbqkbnr/ppp1pppp/8/8/3pP3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", "d4e3")]
    public void MoveScoreEnPassant(string fen, string moveWithHighestScore)
    {
        var bot = GetBot(fen);

        var allMoves = bot._position.GetLegalMoves().OrderByDescending(move => bot.Score(move, default)).ToList();

        Assert.AreEqual(moveWithHighestScore, allMoves[0].ToString()[7..^1]);
        Assert.AreEqual(100_000 + new MyBot().Magic[441 + (int)PieceType.Pawn + 6 * (int)PieceType.Pawn], bot.Score(allMoves[0], default));
    }
}
