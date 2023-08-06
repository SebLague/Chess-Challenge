using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    private int numEvals; // #DEBUG
    private readonly int[] pieceValues = {
        0, 82, 337, 365, 477, 1025, 20000, // opening
        0, 94, 281, 297, 512, 936, 20000, // endgame
    };

    private readonly int[] phaseTransitions = { 0, 0, 1, 1, 2, 4, 0 };

    private int searchDepth;
    private Move bestMove;
    private int bigNumber = 500000;

    // pESTO PSTs compacted to 4-bit per square
    // in piece value order pawn > knight > bishop > rook > queen > king
    // 4 ulongs per piece, each ulong covering 16 squares in ascending orders
    // 12 ulongs for opening phase, 12 ulongs for endgame
    private ulong[] psts =
    {
        // opening
        8608480569177386391, 8685660850885265542, 7455859293954733975, 7450775221175809911,
        244781470246865543, 6528739145396427400, 8613284402485037191, 7311444969172399718,
        7441747066522933893, 8762203425850628487, 8685359588994222216, 8685060590264547174,
        11068062936632834697, 8685624712745224566, 7383519125143254902, 6298135045248419942,
        7464902858680793738, 8613304275280820343, 8536422973693196167, 7460081354432607845,
        5226240968522954598, 8680521733502297718, 6297815023207405174, 8679658625515751048,

        // endgame
        8608480570020589039, 13599369747493255048, 9833459666392676215, 9838262333166024567,
        6230279642916812389, 7388286466382137478, 7460362824862697318, 6230578787355420244,
        8536422969399277431, 8608481667260647543, 8613284334033995639, 8536422969128744823,
        9838263501683455879, 9837963266004318071, 8612984167358494583, 8608480567731124086,
        8685342001104267415, 7532720663170754985, 7532720731856603271, 7378996696646514277,
        5072874484798097800, 9838264673941096839, 7460362897878190215, 7455858129732331365,
    };

    public Move Think(Board board, Timer timer)
    {
        // Feature: Iterative Deepening
        for (int depth = 1; depth <= 4; depth++)
        {
            numEvals = 0; // #DEBUG

            // TODO: figure out how to add timer into iterative deepening
            searchDepth = depth;
            Negamax(board, -bigNumber, bigNumber, depth);

            Console.WriteLine($"{numEvals} evals at {depth} depth"); // #DEBUG
        }

        return bestMove;
    }

    // Feature: Negamax
    private int Negamax(Board board, int alpha, int beta, int depth)
    {
        if (depth <= 0) return Evaluate(board);

        // TODO: Feature Transposition Tables

        // Feature: Move Ordering
        // TODO: better move ordering
        Move[] moves = board
            .GetLegalMoves()
            .OrderByDescending(x => pieceValues[(int)x.CapturePieceType] - pieceValues[(int)x.MovePieceType]).ToArray();

        int bestEval = int.MinValue;
        foreach (Move aMove in moves)
        {
            board.MakeMove(aMove);
            int moveEval = -Negamax(board, -beta, -alpha, depth - 1);
            board.UndoMove(aMove);

            if (moveEval > bestEval)
            {
                bestEval = moveEval;
                if (depth == searchDepth) bestMove = aMove;

                // Feature: Alpha/Beta Pruning
                if (bestEval >= beta) break;
                alpha = Math.Max(bestEval, alpha);
            }
        }

        return bestEval;
    }

    private int Evaluate(Board board)
    {
        numEvals++; // #DEBUG

        if (board.IsRepeatedPosition() || board.IsInStalemate()) return -bigNumber;

        int phase = 0,
            openingEval = 0,
            endgameEval = 0;

        // Feature: PST-based Material Value
        foreach (bool isWhite in new[] { true, false })
        {
            int mul = isWhite ? 1 : -1;
            for (PieceType pieceType = PieceType.Pawn; pieceType <= PieceType.King; pieceType++)
            {
                ulong bitboard = board.GetPieceBitboard(pieceType, isWhite);
                int pieceIdx = (int)pieceType;
                while (bitboard != 0)
                {
                    phase += phaseTransitions[pieceIdx];
                    int squareIdx = BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard) ^ (isWhite ? 0 : 56);
                    int pstIdx = (squareIdx / 16) + (pieceIdx - 1) * 4;
                    openingEval += GetValueOfPiece(squareIdx, pieceIdx, pstIdx) * mul;
                    endgameEval += GetValueOfPiece(squareIdx, pieceIdx + 7, pstIdx + 24) * mul;
                }
            }
        }

        // Feature: Tapered Eval
        return (openingEval * phase + endgameEval * (24 - phase)) / 24 * (board.IsWhiteToMove ? 1 : -1);
    }

    // PST are 4-bit based
    // Original pESTO PST maps have to be widended to range -167 to 178
    private int GetValueOfPiece(int squareIdx, int pieceIdx, int pstIdx)
    {
        return pieceValues[pieceIdx] + (int)((psts[pstIdx] >> (60 - (squareIdx % 16) * 4)) & 15) * 23 - 167;
    }
}