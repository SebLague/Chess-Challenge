using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    private readonly int[] pieceValues = { 0, 100, 300, 300, 500, 800, 10000 };

    // pst bitmaps based on PeSTOs evaluation tables
    // 2 bits per square per piece type = 128 bits per piece
    private readonly UInt64[] pstMidgame = {
        0, 0,
        6149102307313723048, 186899762495640917,
        54059362791861178, 1920796957746397184,
        1154083379876469668, 3074105500802547712,
        12383403614401332896, 19140642015807120,
        1939363942547259425, 1143599624421888,
        2884133349471289344, 1342515274,
    };

    private readonly UInt64[] pstEndgame = {
        0, 0,
        6149102341220968730, 10377508779214525781,
        151001764, 766740585338896384,
        4399188744613, 1900526258299600896,
        12296421366206323025, 6124913085493810180,
        3074316876229733102, 3074280042210000896,
        10180011657669289, 766749536160841728,
    };

    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;

        // iterative deepening
        for (int depth = 0; depth <= 3; depth++)
        {
            if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 30) break;
            bestMove = Search(board, depth);
        }

        return bestMove;
    }


    // TODO: merge with Negamax somehow, this costs 60+ tokens
    private Move Search(Board board, int maxDepth)
    {
        int bestEval = int.MinValue;
        Move bestMove = Move.NullMove;

        Move[] moves = board
            .GetLegalMoves()
            .OrderByDescending(x => pieceValues[(int)x.CapturePieceType] - pieceValues[(int)x.MovePieceType]).ToArray();

        foreach (Move aMove in moves)
        {
            int moveEval = -Negamax(board, aMove, int.MinValue, int.MaxValue, maxDepth);
            if (moveEval > bestEval)
            {
                bestEval = moveEval;
                bestMove = aMove;
            }
        }

        return bestMove;
    }

    private int Negamax(Board board, Move move, int alpha, int beta, int depth)
    {
        board.MakeMove(move);
        int bestEval = int.MinValue;

        if (depth <= 0) bestEval = Evaluate(board);
        else
        {
            Move[] moves = board
                .GetLegalMoves()
                .OrderByDescending(x => pieceValues[(int)x.CapturePieceType] - pieceValues[(int)x.MovePieceType]).ToArray();

            foreach (Move aMove in moves)
            {
                int moveEval = -Negamax(board, aMove, alpha, beta, depth - 1);
                if (moveEval > bestEval)
                {
                    bestEval = moveEval;

                    // alpha/beta pruning
                    alpha = Math.Max(bestEval, alpha);
                    if (alpha >= beta)
                    {
                        bestEval = alpha;
                        break;
                    }
                }
            }
        }

        board.UndoMove(move);
        return bestEval;
    }

    private int Evaluate(Board board)
    {
        int boardEval = 0;

        if (board.IsInCheckmate()) return int.MaxValue;
        if (board.IsRepeatedPosition() || board.IsInStalemate()) return int.MinValue;

        int gamePhase = board.PlyCount / Math.Min(24, board.PlyCount);
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            int materialEval = 0;
            foreach (Piece piece in pieceList)
            {
                int midGameEval = GetValueForPiece(piece, pstMidgame);
                int endGameEval = GetValueForPiece(piece, pstEndgame);
                materialEval += midGameEval * (1 - gamePhase) + endGameEval * gamePhase;
            }

            materialEval *= pieceList.IsWhitePieceList ? 1 : -1;
            boardEval += materialEval;
        }

        return boardEval * (board.IsWhiteToMove ? 1 : -1);
    }

    private int GetValueForPiece(Piece piece, UInt64[] pstValues)
    {
        int pieceEval = pieceValues[(int)piece.PieceType];

        int x = piece.Square.File;
        int y = piece.Square.Rank;
        if (!piece.IsWhite) y = 7 - y;

        int pos = y * 8 + x;
        int pstIdx = (int)piece.PieceType * 2;
        if (pos >= 32)
        {
            pstIdx += 1;
            pos -= 32;
        };

        UInt64 pst = pstMidgame[pstIdx];
        pieceEval += (int)(((pst >> (62 - pos * 2)) & 3) - 1) * 40;

        return pieceEval;
    }
}