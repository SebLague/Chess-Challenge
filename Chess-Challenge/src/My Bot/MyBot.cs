using ChessChallenge.API;
using System;
/* using System.Linq; */
/* using System.Data; */
/* using static ChessChallenge.Application.ConsoleHelper; */
using static ChessChallenge.DriedCod.PieceSquareTables;

public class MyBot : IChessBot
{
    //                                    .  P    K    B    R    Q    K
    private static int[] PIECE_VALUES = { 0, 100, 320, 330, 500, 900, 20000 };
    private static int[][] PSTS;

    private static int WORST_SCORE = -Int32.MaxValue;

    /// <summary>the depth to which the bot searches</summary>
    private int DEPTH = 4;
    private Move bestMove = Move.NullMove;
    private double alpha = -Int32.MaxValue;
    private double beta = Int32.MaxValue;

    // two for each piece type (beginning and end game)
    int[] PAWN_PST_START = {
            0,   0,   0,   0,   0,   0,   0,  0,   //
            98,  134, 61,  95,  68,  126, 34, -11, //
            -6,  7,   26,  31,  65,  56,  25, -20, //
            -14, 13,  6,   21,  23,  12,  17, -23, //
            -27, -2,  -5,  12,  17,  6,   10, -25, //
            -26, -4,  -4,  -10, 3,   3,   33, -12, //
            -35, -1,  -20, -23, -15, 24,  38, -22, //
            0,   0,   0,   0,   0,   0,   0,  0,   //
        };
    int[] PAWN_PST_END = {
            0,   0,   0,   0,   0,   0,   0,   0,   //
            178, 173, 158, 134, 147, 132, 165, 187, //
            94,  100, 85,  67,  56,  53,  82,  84,  //
            32,  24,  13,  5,   -2,  4,   17,  17,  //
            13,  9,   -3,  -7,  -7,  -8,  3,   -1,  //
            4,   7,   -6,  1,   0,   -5,  -1,  -8,  //
            13,  8,   8,   10,  13,  0,   2,   -7,  //
            0,   0,   0,   0,   0,   0,   0,   0,   //
        };
    int[] KNIGHT_PST_START = {
            -167, -89, -34, -49, 61,  -97, -15, -107, //
            -73,  -41, 72,  36,  23,  62,  7,   -17,  //
            -47,  60,  37,  65,  84,  129, 73,  44,   //
            -9,   17,  19,  53,  37,  69,  18,  22,   //
            -13,  4,   16,  13,  28,  19,  21,  -8,   //
            -23,  -9,  12,  10,  19,  17,  25,  -16,  //
            -29,  -53, -12, -3,  -1,  18,  -14, -19,  //
            -105, -21, -58, -33, -17, -28, -19, -23,  //
        };
    int[] KNIGHT_PST_END = {
            -58, -38, -13, -28, -31, -27, -63, -99, //
            -25, -8,  -25, -2,  -9,  -25, -24, -52, //
            -24, -20, 10,  9,   -1,  -9,  -19, -41, //
            -17, 3,   22,  22,  22,  11,  8,   -18, //
            -18, -6,  16,  25,  16,  17,  4,   -18, //
            -23, -3,  -1,  15,  10,  -3,  -20, -22, //
            -42, -20, -10, -5,  -2,  -20, -23, -44, //
            -29, -51, -23, -15, -22, -18, -50, -64, //
        };

    int[] BISHOP_PST_START = {
            -29, 4,  -82, -37, -25, -42, 7,   -8,  //
            -26, 16, -18, -13, 30,  59,  18,  -47, //
            -16, 37, 43,  40,  35,  50,  37,  -2,  //
            -4,  5,  19,  50,  37,  37,  7,   -2,  //
            -6,  13, 13,  26,  34,  12,  10,  4,   //
            0,   15, 15,  15,  14,  27,  18,  10,  //
            4,   15, 16,  0,   7,   21,  33,  1,   //
            -33, -3, -14, -21, -13, -12, -39, -21, //
        };
    int[] BISHOP_PST_END = {
            -14, -21, -11, -8,  -7, -9,  -17, -24, //
            -8,  -4,  7,   -12, -3, -13, -4,  -14, //
            2,   -8,  0,   -1,  -2, 6,   0,   4,   //
            -3,  9,   12,  9,   14, 10,  3,   2,   //
            -6,  3,   13,  19,  7,  10,  -3,  -9,  //
            -12, -3,  8,   10,  13, 3,   -7,  -15, //
            -14, -18, -7,  -1,  4,  -9,  -15, -27, //
            -23, -9,  -23, -5,  -9, -16, -5,  -17, //
        };
    int[] ROOK_PST_START = {
            32,  42,  32,  51,  63, 9,  31,  43,  //
            27,  32,  58,  62,  80, 67, 26,  44,  //
            -5,  19,  26,  36,  17, 45, 61,  16,  //
            -24, -11, 7,   26,  24, 35, -8,  -20, //
            -36, -26, -12, -1,  9,  -7, 6,   -23, //
            -45, -25, -16, -17, 3,  0,  -5,  -33, //
            -44, -16, -20, -9,  -1, 11, -6,  -71, //
            -19, -13, 1,   17,  16, 7,  -37, -26, //
        };
    int[] ROOK_PST_END = {
            13, 10, 18, 15, 12, 12,  8,   5,   //
            11, 13, 13, 11, -3, 3,   8,   3,   //
            7,  7,  7,  5,  4,  -3,  -5,  -3,  //
            4,  3,  13, 1,  2,  1,   -1,  2,   //
            3,  5,  8,  4,  -5, -6,  -8,  -11, //
            -4, 0,  -5, -1, -7, -12, -8,  -16, //
            -6, -6, 0,  2,  -9, -9,  -11, -3,  //
            -9, 2,  3,  -1, -5, -13, 4,   -20, //
        };

    int[] QUEEN_PST_START = {
            -28, 0,   29,  12,  59,  44,  43,  45,  //
            -24, -39, -5,  1,   -16, 57,  28,  54,  //
            -13, -17, 7,   8,   29,  56,  47,  57,  //
            -27, -27, -16, -16, -1,  17,  -2,  1,   //
            -9,  -26, -9,  -10, -2,  -4,  3,   -3,  //
            -14, 2,   -11, -2,  -5,  2,   14,  5,   //
            -35, -8,  11,  2,   8,   15,  -3,  1,   //
            -1,  -18, -9,  10,  -15, -25, -31, -50, //
        };
    int[] QUEEN_PST_END = {
            -9,  22,  22,  27,  27,  19,  10,  20,  //
            -17, 20,  32,  41,  58,  25,  30,  0,   //
            -20, 6,   9,   49,  47,  35,  19,  9,   //
            3,   22,  24,  45,  57,  40,  57,  36,  //
            -18, 28,  19,  47,  31,  34,  39,  23,  //
            -16, -27, 15,  6,   9,   17,  10,  5,   //
            -22, -23, -30, -16, -16, -23, -36, -32, //
            -33, -28, -22, -43, -5,  -32, -20, -41, //
        };
    int[] KING_PST_START = {
            -65, 23,  16,  -15, -56, -34, 2,   13,  //
            29,  -1,  -20, -7,  -8,  -4,  -38, -29, //
            -9,  24,  2,   -16, -20, 6,   22,  -22, //
            -17, -20, -12, -27, -30, -25, -14, -36, //
            -49, -1,  -27, -39, -46, -44, -33, -51, //
            -14, -14, -22, -46, -44, -30, -15, -27, //
            1,   7,   -8,  -64, -43, -16, 9,   8,   //
            -15, 36,  12,  -54, 8,   -28, 24,  14,  //
        };

    int[] KING_PST_END = {
            -74, -35, -18, -18, -11, 15,  4,   -17, //
            -12, 17,  14,  17,  17,  38,  23,  11,  //
            10,  17,  23,  15,  20,  45,  44,  13,  //
            -8,  22,  24,  27,  26,  33,  26,  3,   //
            -18, -4,  21,  24,  27,  23,  9,   -11, //
            -19, -3,  11,  21,  23,  16,  7,   -9,  //
            -27, -11, 4,   13,  14,  4,   -5,  -17, //
            -53, -34, -21, -11, -28, -14, -24, -43, //
        };

    private float progress = 0;

    public MyBot()
    {

        PSTS = new int[][] {
            PAWN_PST_START,  KNIGHT_PST_START, BISHOP_PST_START, ROOK_PST_START,
            QUEEN_PST_START, KING_PST_START,   PAWN_PST_END,     KNIGHT_PST_END,
            BISHOP_PST_END,  ROOK_PST_END,     QUEEN_PST_END,    KING_PST_END,
        };
        GeneratePieceSquareTables();
    }

    private float Lerp(float a, float b, float t)
    {
        t = Math.Min(1, Math.Max(t, 0));
        return a + (b - a) * t;
    }

    public Move Think(Board board, Timer timer)
    {
        progress = Math.Min(board.GameMoveHistory.Length / 40f, 1);
        Search(board, DEPTH, alpha, beta);
        System.Threading.Thread.Sleep(60 * 1000);
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
            {
                alpha = bestScore;
            }
            if (alpha >= beta)
            {
                return alpha;
            }
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
            int type = (int)piece.PieceType;
            materialScore += PIECE_VALUES[type] * factor;
            // -1 because non is not in psts.
            // since we use AllPiecesBitboard, we know that there is a piece there
            int index = piece.Square.Index ^ (piece.IsWhite ? 56 : 0);
            positionScore += Lerp(PSTS[type - 1][index], PSTS[type + 5][index], progress) * factor;

            int piecePositionScore = PSTS[type - 1][index];
        }
        return 50 * materialScore //
           + 25 * mobilityScore //
          + 25 * positionScore;
    }

    private int CalculateMobilityScore(Board board)
    {
        // beware of effect on isInCheck
        board.ForceSkipTurn();
        int theirMobility = board.GetLegalMoves().Length;
        board.UndoSkipTurn();
        return -theirMobility + board.GetLegalMoves().Length;
    }

    static ulong[] PIECE_SQUARE_TABLES = {
        0x0000000000000000, 0xf5227e445f3d8662, 0xec1938411f1a07fa, 0xe9110c1715060df2, 0xe70a06110cfbfee5, 0xf4210303f6fcfce6, 0xea2618f1e9ecffdd, 0x0000000000000000, //
        0x0000000000000000, 0xbba58493869eadb2, 0x545235384355645e, 0x111104fe050d1820, 0xff03f8f9f9fd090d, 0xf8fffb0001fa0704, 0xf902000d0a08080d, 0x0000000000000000, //
        0x95f19f3dcfdea759, 0xef073e172448d7b7, 0x2c49815441253cd1, 0x16124525351311f7, 0xf815131c0d1004f3, 0xf01911130a0cf7e9, 0xedf212fffdf4cbe3, 0xe9ede4efdfc6eb97, //
        0x9dc1e5e1e4f3dac6, 0xcce8e7f7fee7f8e7, 0xd7edf7ff090aece8, 0xee080b16161603ef, 0xee0411101910faee, 0xeaecfd0a0ffffde9, 0xd4e9ecfefbf6ecd6, 0xc0ceeeeaf1e9cde3, //
        0xf807d6e7dbae04e3, 0xd1123b1ef3ee10e6, 0xfe253223282b25f0, 0xfe072525321305fc, 0x040a0c221a0d0dfa, 0x0a121b0e0f0f0f00, 0x0121150700100f04, 0xebd9f4f3ebf2fddf, //
        0xe8eff7f9f8f5ebf2, 0xf2fcf3fdf407fcf8, 0x040006feff00f802, 0x02030a0e090c09fd, 0xf7fd0a07130d03fa, 0xf1f9030d0a08fdf4, 0xe5f1f704fff9eef2, 0xeffbf0f7fbe9f7e9, //
        0x2b1f093f33202a20, 0x2c1a43503e3a201b, 0x103d2d11241a13fb, 0xecf823181a07f5e8, 0xe906f909fff4e6dc, 0xdffb0003eff0e7d3, 0xb9fa0bfff7ecf0d4, 0xe6db07101101f3ed, //
        0x05080c0c0f120a0d, 0x030803fd0b0d0d0b, 0xfdfbfd0405070707, 0x02ff0102010d0304, 0xf5f8fafb04080503, 0xf0f8f4f9fffb00fc, 0xfdf5f7f70200fafa, 0xec04f3fbff0302f7, //
        0x2d2b2c3b0c1d00e4, 0x361c39f001fbd9e8, 0x392f381d0807eff3, 0x01fe11fff0f0e5e5, 0xfd03fcfef6f7e6f7, 0x050e02fbfef502f2, 0x01fd0f08020bf8dd, 0xcee1e7f10af7eeff, //
        0x140a131b1b1616f7, 0x001e193a292014ef, 0x0913232f310906ec, 0x243928392d181603, 0x1727221f2f131cee, 0x050a1109060fe5f0, 0xe0dce9f0f0e2e9ea, 0xd7ece0fbd5eae4df, //
        0x0d02dec8f11017bf, 0xe3dafcf8f9ecff1d, 0xea1606ecf00218f7, 0xdcf2e7e2e5f4ecef, 0xcddfd4d2d9e5ffcf, 0xe5f1e2d4d2eaf2f2, 0x0809f0d5c0f80701, 0x0e18e408ca0c24f1, //
        0xef040ff5eeeeddb6, 0x0b172611110e11f4, 0x0d2c2d140f17110a, 0x031a211a1b1816f8, 0xf509171b1815fcee, 0xf7071017150bfded, 0xeffb040e0d04f5e5, 0xd5e8f2e4f5ebdecb, //
    };
}
