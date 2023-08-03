using ChessChallenge.API;
using System;
using System.Diagnostics;
using System.Linq;
using static ChessChallenge.Application.ConsoleHelper;

public class CompareBot : IChessBot
{
    //                                    .  P    K    B    R    Q    K
    private int[] PIECE_VALUES = { 0, 100, 320, 330, 500, 900, 20000 };

    private int depth = 5,
            lastThinkTime = 0,
            timeForMove = 5_000,
            WORST_SCORE = -Int32.MaxValue;

    private float progress = 0;
    private bool aborted = false;

    private Move bestMove = Move.NullMove;
    private Timer gameTimer;

    private byte[] PIECE_SQUARE_TABLE;

    #pragma warning disable CS8618
    public CompareBot()
    {
        PIECE_SQUARE_TABLE = PIECE_SQUARE_TABLE_RAW.SelectMany(BitConverter.GetBytes).ToArray();
    }

    public Move Think(Board board, Timer timer)
    {
        aborted = false;
        gameTimer = timer;
        int movesDone = board.GameMoveHistory.Length;
        progress = Math.Min(movesDone / 65f, 1);
        if (movesDone > 60)
            timeForMove = timer.MillisecondsRemaining / 40;
        bestMove = board.GetLegalMoves()[0];

        Search(board, depth, WORST_SCORE, -WORST_SCORE, 0);

        lastThinkTime = timer.MillisecondsElapsedThisTurn;
        double diff = lastThinkTime - 1200 * Math.Exp(-Math.Pow((movesDone - 25) / 55, 2));
        if (diff > 0 || aborted)
            depth = Math.Max(depth - 1, 2);
        else
            depth = Math.Min(depth + 1, BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard) < 15 ? 6 : 5);
        return bestMove;
    }

    /// <summary>
    /// Search is a recursive function that searches for the best move at a given depth.
    /// </summary>
    /// <param name="board">current board</param>
    /// <param name="currentDepth">current search depth</param>
    /// <remarks>the depth is decreased by 1 for each recursive call</remarks>
    /// <param name="maxDepth">maximal depth to be searched + the depth at which the boards are ultimately evaluated</param>
    /// <returns>score of the board at depth=0 with the best score obtained</returns>
    private double Search(Board board, int currentDepth, double alpha, double beta, int castled)
    {
        if (board.IsInCheckmate())
            return WORST_SCORE;
        if (board.IsRepeatedPosition())
            return 0;
        if (board.IsDraw())
            return 0;
        // we have reached the depth - evaluate the board for the current color
        if (currentDepth == 0)
            return Evaluate(board, castled);

        double bestScore = WORST_SCORE;

        Move[] moves = board.GetLegalMoves();
        Array.Sort(moves.Select(move =>
              Convert.ToInt32(move.IsCastles) * -50
              - PIECE_VALUES[(int)move.CapturePieceType]
              - PIECE_VALUES[(int)move.PromotionPieceType]).ToArray(), moves);

        foreach (Move move in moves)
        {
            if (gameTimer.MillisecondsElapsedThisTurn > timeForMove)
            {
                aborted = true;
                break;
            }

            // negate the score because after making a move,
            // we are looking at the board from the other player's perspective
            board.MakeMove(move);
            double score = -Search(board, currentDepth - 1, -beta, -alpha, -(castled + Convert.ToInt32(move.IsCastles)));
            board.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                if (currentDepth == depth)
                    this.bestMove = move;
            }
            if (bestScore > alpha)
                alpha = bestScore;
            if (alpha >= beta)
                return alpha;
        }
        return bestScore;
    }

    /// <summary>
    /// Evaluate evaluates a board and returns a score.
    /// The higher the score, the better the board is for the current color
    /// </summary>
    /// <param name="board">board to be evaluated</param>
    /// <returns>score of the board</returns>
    private double Evaluate(Board board, int castlingScore)
    {

        int materialScore = 0, mobilityScore = 0;
        float positionScore = 0;

        ulong pieces = board.AllPiecesBitboard;
        while (pieces > 0)
        {
            Piece piece = board.GetPiece(new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref pieces)));
            int factor = piece.IsWhite == board.IsWhiteToMove ? 1 : -1;
            materialScore += PIECE_VALUES[(int)piece.PieceType] * factor;
            positionScore += GetPieceSquareValue(piece) * factor;
            mobilityScore += BitboardHelper.GetNumberOfSetBits(((piece.IsKnight ? BitboardHelper.GetKnightAttacks(piece.Square) : 0)
                | BitboardHelper.GetSliderAttacks(piece.PieceType, piece.Square, board))
                & ~(piece.IsWhite ? board.WhitePiecesBitboard : board.BlackPiecesBitboard)) * factor;
        }

        return 50 * materialScore
           + 25 * mobilityScore
           + 500 * castlingScore
           + 25 * Math.Min(progress * 2, 1) * positionScore
           + 50 * (board.IsInCheck() ? progress : 0);
    }


    private float ProgressLerp(float a, float b) => a + (b - a) * progress;

    private float GetPieceSquareValue(Piece piece)
    {
        int index = piece.Square.Index ^ (piece.IsWhite ? 0 : 56);
        return ProgressLerp(
            // (type - 1) * 2 * 64 + index
            (sbyte)PIECE_SQUARE_TABLE[128 * (int)piece.PieceType - 128 + index],
            // ((type - 1) * 2 + 1) * 64 + index
            (sbyte)PIECE_SQUARE_TABLE[128 * (int)piece.PieceType - 64 + index]);
    }

    private ulong[] PIECE_SQUARE_TABLE_RAW = {
        0x0000000000000000, 0xf11a10f6f0f2ffe8, 0xf8160202f9fdfdee, 0xef07040c08fdffee, 0xf00c08100e0409f6, 0xf211262c151205fc, 0xf917562e41295b43, 0x0000000000000000, //
        0x0000000000000000, 0xfb01000907050509, 0xfbfffd0001fc0503, 0xff02fbfbfbfe0609, 0x0c0c03ff03091016, 0x393824262e3a4440, 0x7f705a645b6b7579, 0x0000000000000000, //
        0xf0f3edf4ead9f2b9, 0xf3f60cfffef8dcec, 0xf5110c0d0708faf0, 0xfb0e0d13090b03f7, 0x0f0c2f19240d0cfa, 0x1e3258392c1929e0, 0xf4052a101831e4ce, 0xb7f6be29dfe9c48f, //
        0xd5def4f1f6f0ddec, 0xe2f0f2fffdf9f2e3, 0xf1f2fe070afffef0, 0xf4030c0b110bfcf4, 0xf405070f0f0f02f4, 0xe4f3faff0607f2f0, 0xddf0effaffeffbef, 0xbdd5eeebedf7e6d9, //
        0xf2e6f8f7f2f6feea, 0x01160e05000b0a03, 0x070c120a0a0a0a00, 0x03070817120909fc, 0xff051919220d03fd, 0xff1922181b1d19f5, 0xe00c2814f7f40bee, 0xfb05e3efe7c803ec, //
        0xf4fdf5fafdf0faf0, 0xeef6fa03fffbf4f6, 0xf6fb02090705fef8, 0xfafe07050d0902fc, 0x0102070a060806fe, 0x030004ffff00fb01, 0xf6fdf7fef805fdfb, 0xf0f4fafbfbf9f2f6, //
        0xeee7050b0c01f7f3, 0xd0fc07fffaf2f5e2, 0xeafd0002f4f5efe1, 0xf004fb06fff8eee8, 0xf2fb18101205f9f0, 0x0b291f0c18120dfd, 0x1e122e362a271612, 0x1d15062b23161d16, //
        0xf203f7fdff0201fa, 0xfef9fafa0100fcfc, 0xf5fbf8fbfffd00fd, 0xf9fbfcfd03050302, 0x01ff010101090203, 0xfefdfe0303050505, 0x020502fe07090907, 0x030508080a0c0709, //
        0xdeebeff607faf4ff, 0x01fe0a050107fbe8, 0x030a01fdfff901f6, 0xfe02fdfff9faeefa, 0x01ff0cfff5f5eeee, 0x272026140505f4f7, 0x251327f501fde6f0, 0x1f1d1e28081400ed, //
        0xe4f2eafde3f1edea, 0xeae8f0f5f5ecf0f1, 0x03070c06040aeef5, 0x101a1715200d13f4, 0x18271b271f100f02, 0x060d1820210604f2, 0x001411271c160ef4, 0x0e070d12120f0ffa, //
        0x0a10ed05db0818f6, 0x0506f5e3d5fb0501, 0xeef6ece2e1f1f6f6, 0xddeae2e1e6eeffdf, 0xe8f6efeceef8f2f4, 0xf10f04f2f50110fa, 0xece6fdfbfbf2ff14, 0x0901e9daf60b10d4, //
        0xe3f0f6edf9f2e9dc, 0xf4fd030a0903f9ee, 0xfa050b100e07fef3, 0xf9061012100efdf4, 0x0212161212100ffb, 0x091e1f0e0a100c07, 0x07101a0c0c0a0cf8, 0xf4030af9f4f4e8ce, //
    };
}
