using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

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

            if (isSearchCancelled || Math.Abs(bestEval) > immediateMateScore - 1000) break;
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

            if (eval >= beta) return beta;
            if (eval > alpha)
            {
                alpha = eval;
                bestMovesByDepth[plyFromRoot] = move;
                bestEval = (plyFromRoot == 0) ? eval : bestEval;
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


    #region Evalution

    const float endgameMaterialStart = 1750;

    //Represent the rank scores as a 64-bit int. Last couple rows are all copies
    ulong[] kingMidgameTable = new ulong[]
    {
        0b_00010100_00011110_00001010_00000000_00000000_00001010_00011110_00010100L,
        0b_00010100_00010100_00000000_00000000_00000000_00000000_00010100_00010100L,
        0b_11110110_11101100_11101100_11101100_11101100_11101100_11101100_11110110L,
        0b_11101100_11100010_11100010_11011000_11011000_11100010_11100010_11101100L,
        0b_11100010_11011000_11011000_11001110_11001110_11011000_11011000_11100010L
    };

    ulong[] kingEndgameTable = new ulong[]
    {
        0b_11100010_11110110_00011110_00101000_00101000_00011110_11110110_11100010L,
        0b_11100010_11110110_00010100_00011110_00011110_00010100_11110110_11100010L,
        0b_11100010_11100010_00000000_00000000_00000000_00000000_11100010_11100010L,
        0b_11001110_11100010_11100010_11100010_11100010_11100010_11100010_11001110L,
    };

    // Performs static evaluation of the current position.
    // The position is assumed to be 'quiet', i.e no captures are available that could drastically affect the evaluation.
    // The score that's returned is given from the perspective of whoever's turn it is to move.
    // So a positive score means the player who's turn it is to move has an advantage, while a negative score indicates a disadvantage.
    public int Evaluate(Board board)
    {
        Square whiteKingSquare = board.GetKingSquare(true);
        Square blackKingSquare = board.GetKingSquare(false);

        //Mobility
        int mobility = GetMobilityBonus(board);
        if (board.TrySkipTurn())
        {
            mobility -= GetMobilityBonus(board);
            board.UndoSkipTurn();
        }
        else mobility = 0; // ignore mobility if we can't get it for both sides


        return (CountMaterial(board, true) - CountMaterial(board, false)
            + GetKingSafetyScores(whiteKingSquare.File, whiteKingSquare.Rank, EndgamePhaseWeight(board, true))
            - GetKingSafetyScores(blackKingSquare.File, 7 - blackKingSquare.Rank, EndgamePhaseWeight(board, false))
            + GetEndgameBonus(board, true)
            - GetEndgameBonus(board, false)) 
            * ((board.IsWhiteToMove) ? 1 : -1) 
            + mobility;
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

    float EndgamePhaseWeight(Board board, bool isWhite)
    {
        return 1 - Math.Min(1, (CountMaterial(board, isWhite) - board.GetPieceList(PieceType.Pawn, isWhite).Count * 100) / 1750);
    }

    int GetMobilityBonus(Board board)
    {
        int mobility = 0;
        foreach (Move move in board.GetLegalMoves())
        {
            switch (move.MovePieceType)
            {
                case PieceType.Knight:
                    mobility += 100; // More points for knight since it has a smaller maximum of possible moves
                    break;
                case PieceType.Bishop:
                    mobility += 5;
                    break;
                case PieceType.Rook:
                    mobility += 6;
                    break;
                case PieceType.Queen:
                    mobility += 4;
                    break;
            }
        }
        return mobility;
    }

    int GetKingSafetyScores(int file, int relativeRank, float endgameWeight)
    {
        sbyte midgameScore = (sbyte)((kingMidgameTable[Math.Min(relativeRank, 4)] >> file * 8) % 256);
        return (int)(midgameScore + (  midgameScore - (sbyte)( (kingEndgameTable[(int)Math.Abs(3.5 - relativeRank)] >> file * 8) % 256 )  ) * endgameWeight);
    }


    int CountMaterial(Board board, bool isWhite)
    {
        return board.GetPieceList(PieceType.Pawn, isWhite).Count * 100
            + board.GetPieceList(PieceType.Knight, isWhite).Count * 350
            + board.GetPieceList(PieceType.Bishop, isWhite).Count * 350
            + board.GetPieceList(PieceType.Rook, isWhite).Count * 525
            + board.GetPieceList(PieceType.Queen, isWhite).Count * 1000;
    }

    int GetEndgameBonus(Board board, bool isWhite)
    {
        float enemyEndgameWeight = EndgamePhaseWeight(board, !isWhite);
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