namespace TeenyLynx.Test;
public class StaticEvaluationTests : BaseTest
{
    [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 0)]           // Startpos
    [TestCase("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1", 0)]

    [TestCase("rnbqkbnr/pppppppp/8/8/8/5N2/PPPPPPPP/RNBQKB1R w KQkq - 1 1", +40)]       // Nf3 - white adv.
    [TestCase("rnbqkbnr/pppppppp/8/8/8/5N2/PPPPPPPP/RNBQKB1R b KQkq - 1 1", -40)]

    [TestCase("r1bqkbnr/pppppppp/2n5/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 1 1", -40)]       // Nf6 - black adv.
    [TestCase("r1bqkbnr/pppppppp/2n5/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 1 1", +40)]

    [TestCase("rnbqkbnr/pppppppp/8/8/8/7N/PPPPPPPP/RNBQKB1R w KQkq - 1 1", +15)]        // Na6 - white adv.
    [TestCase("rnbqkbnr/pppppppp/8/8/8/7N/PPPPPPPP/RNBQKB1R b KQkq - 1 1", -15)]

    [TestCase("q7/8/8/4k3/8/4K3/8/7Q w - - 0 1", 0)]                                    // King regular positional value table
    [TestCase("q7/8/8/4k3/8/4K3/8/7Q b - - 0 1", 0)]

    [TestCase("q7/8/8/4k3/8/8/4K3/7Q w - - 0 1", +30)]                                  // King regular positional value table
    [TestCase("q7/8/8/4k3/8/8/4K3/7Q b - - 0 1", -30)]

    [TestCase("r7/8/8/4k3/8/4K3/8/7R w - - 0 1", -20)]                                  // King endgame positional value table
    [TestCase("r7/8/8/4k3/8/4K3/8/7R b - - 0 1", +20)]

    [TestCase("r7/8/8/4k3/8/8/4K3/7R w - - 0 1", -30)]                                  // King endgame positional value table
    [TestCase("r7/8/8/4k3/8/8/4K3/7R b - - 0 1", +30)]

    [TestCase("3k3n/8/8/3N4/8/8/8/3K4 w - - 0 1", +60)]                                 // Knight positional value table  <-----
    [TestCase("3k3n/8/8/3N4/8/8/8/3K4 b - - 0 1", -60)]

    [TestCase("3k3N/8/8/3n4/8/8/8/3K4 w - - 0 1", -90)]                                 // Knight positional value table
    [TestCase("3k3N/8/8/3n4/8/8/8/3K4 b - - 0 1", +90)]

    [TestCase("3k4/8/8/8/8/5Q2/q7/3K4 w - - 0 1", +15)]                                 // Queen positional value table
    [TestCase("3k4/8/8/8/8/5Q2/q7/3K4 b - - 0 1", -15)]

    [TestCase("3k4/8/8/8/8/5q2/Q7/3K4 w - - 0 1", -15)]
    [TestCase("3k4/8/8/8/8/5q2/Q7/3K4 b - - 0 1", +15)]                                 // Queen positional value table
    public void StaticEvaluation(string fen, int expectedEval)
    {
        // Arrange
        var teenyLynx = GetBot(fen);

        // Act
        var staticEval = teenyLynx.QuiescenceSearch(0, int.MinValue, beta: int.MinValue);    // Causing beta cutoff and getting static eval back

        // Assert
        Assert.AreEqual(expectedEval, staticEval);
    }
}
