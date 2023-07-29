using ChessChallenge.API;
using System;
using System.Linq;

public class CompareBot: IChessBot
{
    //                                    .  P    K    B    R    Q    K
    private static int[] PIECE_VALUES = { 0, 100, 320, 330, 500, 900, 20000 };
    private static int WORST_SCORE = -Int32.MaxValue;
    /// <summary>the depth to which the bot searches</summary>
    private int DEPTH = 4;
    private Move bestMove = Move.NullMove;
    private double alpha = -Int32.MaxValue;
    private double beta = Int32.MaxValue;
    private float progress = 0;
    int[] PIECE_SQUARE_TABLE;

    public CompareBot()
    {

        PIECE_SQUARE_TABLE = PIECE_SQUARE_TABLE_RAW.Aggregate(new int[0], (decoded, rank) =>
              {
                  return decoded.Concat(
                      Enumerable.Range(0, 8).Select(file =>
                          {
                              return (int)(sbyte)((rank & (255UL << 8 * file)) >> 8 * file);
                          })
                  ).ToArray();
              });
    }

    private float Lerp(float a, float b, float t)
    {
        t = Math.Min(1, Math.Max(t, 0));
        return a + (b - a) * t;
    }

    private float GetPieceSquareValue(Piece piece)
    {
        int index = piece.Square.Index ^ (piece.IsWhite ? 0 : 56);
        return Lerp( //
            PIECE_SQUARE_TABLE[((int)piece.PieceType - 1) * 64 + index],
            PIECE_SQUARE_TABLE[(int)piece.PieceType * 64 + index],
            progress);
    }

    public Move Think(Board board, Timer timer)
    {
        progress = Math.Min(board.GameMoveHistory.Length / 65f, 1);
        Search(board, DEPTH, alpha, beta);
        return bestMove;
    }

    /// <summary>
    /// Search is a recursive function that searches for the best move at a given depth.
    /// </summary>
    /// <param name="board">current board</param>
    /// <param name="depth">current search depth</param>
    /// <remarks>the depth is decreased by 1 for each recursive call</remarks>
    /// <param name="maxDepth">maximal depth to be searched + the depth at which the boards are ultimately evaluated</param>
    /// <returns>score of the board at depth=0 with the best score obtained</returns>
    double Search(Board board, int depth, double alpha, double beta)
    {
        //Log("alpha" + alpha);
        //Log("Beta" + beta);
        // we have reached the depth - evaluate the board for the current color
        if (depth == 0)
            return Evaluate(board);

        double bestScore = WORST_SCORE;
        Move[] moves = board.GetLegalMoves();
        if (depth == DEPTH)
            bestMove = moves[0];
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            // negate the score because after making a move,
            // we are looking at the board from the other player's perspective
            double score = -Search(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);
            if (score > bestScore)
            {
                bestScore = score;
                if (depth == DEPTH)
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
    private double Evaluate(Board board)
    {
        if (board.IsInCheckmate())
            return WORST_SCORE;
        if (board.IsRepeatedPosition())
            return WORST_SCORE / 4;
        if (board.IsDraw())
            return 0;

        int mobilityScore = CalculateMobilityScore(board);

        int materialScore = 0;
        float positionScore = 0;
        ulong pieces = board.AllPiecesBitboard;
        while (pieces > 0)
        {
            Piece piece = board.GetPiece(
                              new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref pieces)));
            int factor = piece.IsWhite == board.IsWhiteToMove ? 1 : -1;
            materialScore += PIECE_VALUES[(int)piece.PieceType] * factor;
            positionScore += GetPieceSquareValue(piece) * factor;
        }
        return 60 * materialScore //
           + 20 * mobilityScore //
          + 20 * positionScore;
    }

    private int CalculateMobilityScore(Board board)
    {
        // beware of effect on isInCheck
        board.ForceSkipTurn();
        int theirMobility = board.GetLegalMoves().Length;
        board.UndoSkipTurn();
        return -theirMobility + board.GetLegalMoves().Length;
    }

    ulong[] PIECE_SQUARE_TABLE_RAW = {
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
