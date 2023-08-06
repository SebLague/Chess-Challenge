using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    private int numEvals; // #DEBUG
    private readonly int[] pieceValues = { 0, 82, 337, 365, 477, 1025, 20000 };

    private int searchDepth;
    private Move bestMove;
    private int bigNumber = 500000;

    private ulong[] pstMidgame =
    {
        8608480569177386391, 8685660850885265542, 7455859293954733975, 7450775221175809911,
        244781470246865543, 6528739145396427400, 8613284402485037191, 7311444969172399718,
        7441747066522933893, 8762203425850628487, 8685359588994222216, 8685060590264547174,
        11068062936632834697, 8685624712745224566, 7383519125143254902, 6298135045248419942,
        7464902858680793738, 8613304275280820343, 8536422973693196167, 7460081354432607845,
        5226240968522954598, 8680521733502297718, 6297815023207405174, 8679658625515751048,
    };

    private ulong[] pstEndgame =
    {
        8608480570020589039, 13599369747493255048, 9833459666392676215, 9838262333166024567,
        6230279642916812389, 7388286466382137478, 7460362824862697318, 6230578787355420244,
        8536422969399277431, 8608481667260647543, 8613284334033995639, 8536422969128744823,
        9838263501683455879, 9837963266004318071, 8612984167358494583, 8608480567731124086,
        8685342001104267415, 7532720663170754985, 7532720731856603271, 7378996696646514277,
        5072874484798097800, 9838264673941096839, 7460362897878190215, 7455858129732331365,
    };

    public Move Think(Board board, Timer timer)
    {
        for (int depth = 1; depth <= 4; depth++)
        {
            numEvals = 0; // #DEBUG

            if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 30) break;

            searchDepth = depth;
            Negamax(board, -bigNumber, bigNumber, depth);

             Console.WriteLine($"{numEvals} evals at {depth} depth"); // #DEBUG
        }

        return bestMove;
    }

    private int Negamax(Board board, int alpha, int beta, int depth)
    {
        if (depth <= 0) return Evaluate(board);

        int bestEval = int.MinValue;
        Move[] moves = board
            .GetLegalMoves()
            .OrderByDescending(x => pieceValues[(int)x.CapturePieceType] - pieceValues[(int)x.MovePieceType]).ToArray();

        foreach (Move aMove in moves)
        {
            board.MakeMove(aMove);
            int moveEval = -Negamax(board, -beta, -alpha, depth - 1);
            board.UndoMove(aMove);

            if (moveEval > bestEval)
            {
                bestEval = moveEval;
                if (depth == searchDepth) bestMove = aMove;

                // TODO alpha/beta pruning
                if (bestEval >= beta) break;
                alpha = Math.Max(bestEval, alpha);
            }
        }

        return bestEval;
    }

    private int Evaluate(Board board)
    {
        numEvals++; // #DEBUG
        int boardEval = 0;

        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            int materialEval = 0;
            foreach (Piece piece in pieceList)
            {
                materialEval += GetValueOfPiece(piece);
            }

            materialEval *= pieceList.IsWhitePieceList ? 1 : -1;
            boardEval += materialEval;
        }

        return boardEval * (board.IsWhiteToMove ? 1 : -1);
    }

    // TODO: Optimise this func with better bit operations
    // TODO: Game Phase
    private int GetValueOfPiece(Piece piece)
    {
        int rank = piece.IsWhite ? piece.Square.Rank : (7 - piece.Square.Rank);

        int pieceIdx = rank * 8 + piece.Square.File;
        int pstIdx = (pieceIdx / 16) + ((int)piece.PieceType - 1) * 4;
        ulong pst = pstMidgame[pstIdx];
        int bitmapOffset = 60 - (pieceIdx % 16) * 4;
        return pieceValues[(int)piece.PieceType] + (int)((pst >> bitmapOffset) & 15) * 23 - 167;
    }
}