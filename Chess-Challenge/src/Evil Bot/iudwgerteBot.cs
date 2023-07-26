// https://github.com/iudwgerte/Chess-Challenge

using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
public class iudwgerteBot : IChessBot
{
    static int[] PieceValue = { 0, 100, 320, 330, 500, 1000, 0 };
    static List<Move> OrderMoves(Move[] moves)
    {
        List<Move> orderedMoves = new List<Move>();
        foreach (Move move in moves)
        {
            if (move.IsCapture || move.IsPromotion)
            {
                orderedMoves.Insert(0, move);
            }
            else
            {
                orderedMoves.Add(move);
            }
        }
        return orderedMoves;
    }
    static int[] DistanceToEdge = { 0, 1, 2, 3, 3, 2, 1, 0 };
    static Func<Square, int>[] PSQT_mg = {
                                         sq => 0,                                                                                             //null
                                         sq => sq.Rank*10-10+(sq.Rank==1&&DistanceToEdge[sq.File]!=3?40:0)+(DistanceToEdge[sq.Rank]==3&&DistanceToEdge[sq.File]==3?10:0), //pawn
                                         sq => (DistanceToEdge[sq.Rank]+DistanceToEdge[sq.File])*10-40,                                       //knight
                                         sq => (DistanceToEdge[sq.Rank]+DistanceToEdge[sq.File])*10-40,                                       //bishop
                                         sq => sq.Rank==6?10:0+((sq.Rank==0&&DistanceToEdge[sq.File]==3)?10:0),                               //rook
                                         sq => (DistanceToEdge[sq.Rank]+DistanceToEdge[sq.File])*5-10,                                        //queen
                                         sq => (3-DistanceToEdge[sq.Rank]+3-DistanceToEdge[sq.File])*10-5-(sq.Rank>1?50:0)                    //king
                                        };
    static Func<Square, int>[] PSQT_eg = {
                                         sq => 0,                                                       //null
                                         sq => sq.Rank*10,                                              //pawn
                                         sq => (DistanceToEdge[sq.Rank]+DistanceToEdge[sq.File])*10-40, //knight
                                         sq => (DistanceToEdge[sq.Rank]+DistanceToEdge[sq.File])*10-40, //bishop
                                         sq => (DistanceToEdge[sq.Rank]+DistanceToEdge[sq.File]-3)*5, //rook
                                         sq => (DistanceToEdge[sq.Rank]+DistanceToEdge[sq.File])*5-10,  //queen
                                         sq => (DistanceToEdge[sq.Rank]+DistanceToEdge[sq.File])*5  //king
                                        };
    static int[] EndgameWeight = { 0, 0, 1, 1, 2, 4, 0 };
    static int CenterControl(Board board)
    {
        ulong whitebitboard = board.GetPieceBitboard(PieceType.Pawn, true) | board.GetPieceBitboard(PieceType.Knight, true);
        ulong blackbitboard = board.GetPieceBitboard(PieceType.Pawn, false) | board.GetPieceBitboard(PieceType.Knight, false);
        return BitOperations.PopCount(whitebitboard & 103481868288) - BitOperations.PopCount(blackbitboard & 103481868288) +
               BitOperations.PopCount(whitebitboard & 66229406269440) - BitOperations.PopCount(blackbitboard & 66229406269440);
    }
    static int Eval(Board board)
    {
        if (board.IsDraw()) { return 0; }
        if (board.IsInCheckmate()) { return 320000 * (board.IsWhiteToMove ? 1 : -1); }
        PieceList[] pieceList = board.GetAllPieceLists();
        int material = 0;
        int psqt_mg = 0;
        int psqt_eg = 0;
        int endgamePhase = 0;
        foreach (PieceList list in pieceList)
        {
            material += PieceValue[(int)list.TypeOfPieceInList] * list.Count * (list.IsWhitePieceList ? 1 : -1);
            endgamePhase += EndgameWeight[(int)list.TypeOfPieceInList] * list.Count;
        }
        for (int i = 0; i < 64; i++)
        {
            Square sq = new Square(i);
            Piece p = board.GetPiece(sq);
            if (!p.IsWhite) sq = new Square(i ^ 56);
            psqt_mg += (PSQT_mg[(int)p.PieceType](sq)) * (p.IsWhite ? 1 : -1);
            psqt_eg += (PSQT_eg[(int)p.PieceType](sq)) * (p.IsWhite ? 1 : -1);
        }
        return (int)((material +
                 (float)(psqt_eg * endgamePhase + psqt_mg * (24 - endgamePhase)) / 24
                 ) * 50 + board.GetLegalMoves().Length + CenterControl(board) * 3) * (board.IsWhiteToMove ? 1 : -1);
    }
    static int Max(int a, int b) { return a > b ? a : b; }
    static int Search(Board board, int depth, int alpha, int beta)
    {
        if (depth == 0)
        {
            if (board.IsDraw()) { return 0; }
            if (board.IsInCheckmate()) { return 320000 * (board.IsWhiteToMove ? 1 : -1); }
            return Eval(board);
        }
        Move[] moves = board.GetLegalMoves();
        List<Move> orderedMoves = OrderMoves(moves);
        foreach (Move move in orderedMoves)
        {
            board.MakeMove(move);
            int CurrEval = -Search(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);
            if (CurrEval >= beta)
            {
                return beta;
            }
            alpha = Max(alpha, CurrEval);
        }
        return alpha;
    }
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        int MaxEval = -320001;
        Move MaxMove = new Move();
        int CurrDepth = 1;
        Random rng = new();
        int time = timer.MillisecondsRemaining / 50;
        while (true)
        {
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                if (board.IsInCheckmate()) return move;
                int CurrEval = -Search(board, CurrDepth, -320000, 320000);
                if (MaxEval < CurrEval)
                {
                    MaxEval = CurrEval;
                    MaxMove = move;
                }
                board.UndoMove(move);
            }
            if (timer.MillisecondsElapsedThisTurn > time)
            {
                if (MaxMove != Move.NullMove)
                    return MaxMove;
                else return moves[rng.Next(moves.Length)];
            }
            CurrDepth++;
        }
    }
}