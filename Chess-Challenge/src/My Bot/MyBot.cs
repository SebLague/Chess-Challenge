using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        StartSearch(board, timer);
        return bestMovesByDepth[0];
    }


    const int immediateMateScore = 100000;
    const int positiveInfinity = 9999999;
    const int negativeInfinity = -positiveInfinity;
    const int maxSearchDepth = int.MaxValue;
    const int maxMillisecondsPerSearch = 1500;

    List<Move> bestMovesByDepth;
    int bestEval;

    bool isSearchCancelled;


    void StartSearch(Board board, Timer timer)
    {
        bestMovesByDepth = new List<Move>();
        bestEval = 0;
        isSearchCancelled = false;

        for (int searchDepth = 1; searchDepth < int.MaxValue; searchDepth++)
        {
            bestMovesByDepth.Add(Move.NullMove);
            Search(board, timer, searchDepth, 0, negativeInfinity, positiveInfinity);

            if (isSearchCancelled || IsMateScore(bestEval)) break;
        }
    }

    int Search(Board board, Timer timer, int plyRemaining, int plyFromRoot, int alpha, int beta)
    {
        if (timer.MillisecondsElapsedThisTurn > maxMillisecondsPerSearch) // Cancel the search if we are out of time
        {
            isSearchCancelled = true;
            return 0;
        }

        if (board.IsInCheckmate()) return -immediateMateScore + plyFromRoot; // Check for Checkmate before we do anything else.


        // Once we reach target depth, search all captures to make the evaluation more accurate
        if (plyRemaining == 0) return QuiescenceSearch(board, alpha, beta);

        Move[] unorderedMoves = board.GetLegalMoves();
        if (unorderedMoves.Length == 0) return 0; // Stalemate

        // Order the moves, making sure to put the best move from the previous iteration first
        Move[] orderedMoves = Order(unorderedMoves, board, bestMovesByDepth[plyFromRoot]);

        foreach (Move move in orderedMoves)
        {
            board.MakeMove(move);
            int eval = -Search(board, timer, plyRemaining - 1, plyFromRoot + 1, -beta, -alpha);
            board.UndoMove(move);

            Console.WriteLine("-------");
            Console.WriteLine("Board FEN String: " + board.GetFenString());
            Console.WriteLine("Depth: " + plyFromRoot);
            Console.WriteLine("Eval: " + eval + ", Alpha: " + alpha + ", Beta: " + beta);

            if (eval >= beta) return beta;
            if (eval > alpha)
            {
                alpha = eval;
                bestMovesByDepth[plyFromRoot] = move;
            }
        }

        return alpha;
    }

    int QuiescenceSearch(Board board, int alpha, int beta)
    {
        int eval = Evaluate(board);
        if (eval >= beta) return beta;
        alpha = Math.Max(alpha, eval);

        // Order the moves
        Move[] orderedMoves = Order(board.GetLegalMoves(true), board, Move.NullMove);

        foreach (Move move in orderedMoves)
        {
            board.MakeMove(move);
            eval = -QuiescenceSearch(board, -beta, -alpha);
            board.UndoMove(move);

            if (eval >= beta) return beta;
            alpha = Math.Max(alpha, eval);
        }

        return alpha;
    }

    Move[] Order(Move[] moves, Board board, Move putThisFirst)
    {
        if (moves.Length == 0) return new Move[0];
        Move[] returnThis = new Move[moves.Length];

        Dictionary<Move, int> orderedMoves = new Dictionary<Move, int>();
        foreach (Move move in moves)
        {
            if (move.IsNull) continue;

            int moveScoreGuess = 0;
            if (move.IsCapture) moveScoreGuess += 10 * GetPointValue(move.CapturePieceType) - GetPointValue(move.MovePieceType);
            if (move.IsPromotion) moveScoreGuess += GetPointValue(move.PromotionPieceType);
            if (board.SquareIsAttackedByOpponent(move.TargetSquare)) moveScoreGuess -= GetPointValue(move.MovePieceType);
            if (move == putThisFirst) moveScoreGuess += 100000;
            orderedMoves.Add(move, moveScoreGuess);
        }
        int counter = 0;
        foreach (var k in orderedMoves.OrderByDescending(x => x.Value))
        {
            returnThis[counter] = k.Key;
            counter++;
        }

        return returnThis;

    }

    int[] POINT_VALUES = { 100, 350, 350, 525, 1000 };
    int GetPointValue(PieceType type)
    {
        switch (type)
        {
            case PieceType.None: return 0;
            case PieceType.King: return positiveInfinity;
            default: return POINT_VALUES[(int)type - 1];
        }
    }

    bool IsMateScore(int score)
    {
        return Math.Abs(score) > immediateMateScore - 1000;
    }

    #region Evalution

    const float endgameMaterialStart = 1750;

    int[] kingMidgameTable = new int[]
    {
        20, 30, 10,  0,  0, 10, 30, 20,
        20, 20,  0,  0,  0,  0, 20, 20,
        -10,-20,-20,-20,-20,-20,-20,-10,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
    };

    //Represent the rank scores as a 64-bit int. NEED TO FINISH
    ulong[] kingMidgameTable_v2 = new ulong[]
    {
        0b0001010000011110000010100000000000000000000010100001111000010100L,
        0b0001010000010100000000000000000000000000000000000001010000010100L,
    };
    int[] kingEndgameTable = new int[]
    {
        -50,-30,-30,-30,-30,-30,-30,-50,
        -30,-30,  0,  0,  0,  0,-30,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-20,-10,  0,  0,-10,-20,-30,
        -50,-40,-30,-20,-20,-30,-40,-50,
    };

    // Performs static evaluation of the current position.
    // The position is assumed to be 'quiet', i.e no captures are available that could drastically affect the evaluation.
    // The score that's returned is given from the perspective of whoever's turn it is to move.
    // So a positive score means the player who's turn it is to move has an advantage, while a negative score indicates a disadvantage.
    public int Evaluate(Board board)
    {
        int whiteEval = 0;
        int blackEval = 0;

        int whiteMaterial = CountMaterial(board, true);
        int blackMaterial = CountMaterial(board, false);

        int whiteMaterialWithoutPawns = whiteMaterial - board.GetPieceList(PieceType.Pawn, true).Count * POINT_VALUES[0];
        int blackMaterialWithoutPawns = blackMaterial - board.GetPieceList(PieceType.Pawn, false).Count * POINT_VALUES[0];
        float whiteEndgamePhaseWeight = EndgamePhaseWeight(whiteMaterialWithoutPawns);
        float blackEndgamePhaseWeight = EndgamePhaseWeight(blackMaterialWithoutPawns);

        // Material
        whiteEval += whiteMaterial;
        blackEval += blackMaterial;

        // King Safety
        int whiteKingRelativeIndex = board.GetKingSquare(true).Index;
        int blackKingRelativeIndex = new Square(board.GetKingSquare(false).File, 7 - board.GetKingSquare(false).Rank).Index;
        whiteEval += (int)Lerp(kingMidgameTable[whiteKingRelativeIndex], kingEndgameTable[whiteKingRelativeIndex], whiteEndgamePhaseWeight);
        blackEval += (int)Lerp(kingMidgameTable[blackKingRelativeIndex], kingEndgameTable[blackKingRelativeIndex], blackEndgamePhaseWeight);

        // Endgame Bonuses
        whiteEval += GetEndgameBonus(board, blackEndgamePhaseWeight, true);
        blackEval += GetEndgameBonus(board, whiteEndgamePhaseWeight, false);


        return (whiteEval - blackEval) * ((board.IsWhiteToMove) ? 1 : -1);
    }

    float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    float EndgamePhaseWeight(int materialCountWithoutPawns)
    {
        return 1 - Math.Min(1, materialCountWithoutPawns / endgameMaterialStart);
    }


    int CountMaterial(Board board, bool isWhite)
    {
        return board.GetPieceList(PieceType.Pawn, isWhite).Count * POINT_VALUES[0]
            + board.GetPieceList(PieceType.Knight, isWhite).Count * POINT_VALUES[1]
            + board.GetPieceList(PieceType.Bishop, isWhite).Count * POINT_VALUES[2]
            + board.GetPieceList(PieceType.Rook, isWhite).Count * POINT_VALUES[3]
            + board.GetPieceList(PieceType.Queen, isWhite).Count * POINT_VALUES[4];
    }

    int GetEndgameBonus(Board board, float enemyEndgameWeight, bool isWhite)
    {
        if (enemyEndgameWeight <= 0) return 0;
        ulong ourBB = (isWhite) ? board.WhitePiecesBitboard : board.BlackPiecesBitboard;
        Square enemyKingSquare = board.GetKingSquare(!isWhite);

        int endgameBonus = 0;
        while (ourBB != 0) {
            Square pieceSquare = new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref ourBB));
            switch (board.GetPiece(pieceSquare).PieceType)
            {
                case PieceType.Pawn:
                    // Encourage pawns to move forward
                    endgameBonus += 50 - 10 * ((isWhite) ? 7 - pieceSquare.Rank : pieceSquare.Rank);
                    break;
                case PieceType.Rook:
                    //Encourage rooks to get close to the same rank/file as the king
                    endgameBonus += 50 - 10 * Math.Min(Math.Abs(enemyKingSquare.File - pieceSquare.File), Math.Abs(enemyKingSquare.Rank - pieceSquare.Rank));
                    break;
                default:
                    // In general, we want to get our pieces closer to the enemy king, will give us a better chance of finding a checkmate.
                    // Use power growth so we prioritive
                    endgameBonus += 50 - (int)(10 * Math.Pow(Math.Max(Math.Abs(enemyKingSquare.File - pieceSquare.File), Math.Abs(enemyKingSquare.Rank - pieceSquare.Rank)), 1.5));
                    break;
            }
        }

        return (int)(endgameBonus * enemyEndgameWeight);
    }

    #endregion
}