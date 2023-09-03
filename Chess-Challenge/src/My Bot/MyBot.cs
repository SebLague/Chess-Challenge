// TalkChess Bot

// contributors:
// Thomas Jahn - Leorik
// Mike Sherwin - RomiChess
// Dmitry Shechtman - LeanChess
// (Get your name added here)
// Deadline October 1st 2023

using ChessChallenge.API;

public class MyBot : IChessBot
{
    //Implements the https://en.wikipedia.org/wiki/Negamax algorithm

    // Piece values: null, pawn, knight, bishop, rook, queen
    int[] PieceValues = { 0, 100, 320, 330, 500, 900, 10000 };
    int CheckmateScore = 9999;
    int Depth;

    ulong[] pst = {
        0xE6F4B06438321400,0xEAF6B2643A341400,0xEAF2B2643A361400,0xEAF0B3653A361400,
        0xEAF0B3653A361400,0xEAF2B2643A361400,0xEAF6B2643A341400,0xE6F4B06438321400,
        0xEAF4B2633A341500,0xEAF4B4643D381600,0xF0F0B5643C3C1600,0xF0F0B4643C3D1000,
        0xF0F0B4643C3D1000,0xF0F0B4643C3C1600,0xEAF4B4643D381600,0xEAF4B2633A341500,
        0xEAEEB2633A361500,0xEEECB5643E3D1300,0xF4ECB5643E3E1200,0xF6ECB5643E3F1400,
        0xF6ECB5643E3F1400,0xF4ECB5643E3E1200,0xEEECB4643E3D1300,0xEAEEB2633A361500,
        0xEAECB4633A361400,0xEEEAB4643C3C1400,0xF6EAB5643E3F1400,0xF8E8B5643E401800,
        0xF8E8B5643E401800,0xF6EAB5643E3F1400,0xEEEAB4643C3C1400,0xEAECB3633A361400,
        0xEAEAB3633A361500,0xEEE8B4643D3D1500,0xF6E8B5643D3F1600,0xF8E6B5643E401900,
        0xF8E6B5643E401900,0xF6E8B5643D3F1600,0xEEE8B4643D3D1500,0xEAEAB3633A361500,
        0xEAEAB2633A361600,0xEEE8B4643C3C1600,0xF4E8B5643D3E1800,0xF6E6B5643E3F1A00,
        0xF6E6B5643E3F1A00,0xF4E8B5643D3E1800,0xEEE8B4643C3C1600,0xEAEAB2633A361600,
        0xEAEAB2653A341E00,0xECE8B4663C381E00,0xEEE8B4663C3C1E00,0xF0E6B4663C3C1E00,
        0xF0E6B4663C3C1E00,0xEEE8B4663C3C1E00,0xECE8B4663C381E00,0xEAEAB2653A341E00,
        0xE6EAB06438321400,0xE8E8B2643A341400,0xEAE8B2643A361400,0xECE6B3643A361400,
        0xECE6B3643A361400,0xEAE8B2643A361400,0xE8E8B2643A341400,0xE6EAB06438321400,
    };

    public Move Think(Board board, Timer timer)
    {
        int score;
        Move bestMove = default;
        Move[] moves = board.GetLegalMoves();
        for (Depth = 1; Depth < 99; Depth++)
        {
            int alpha = -CheckmateScore;
            bestMove = default;
            Move bestNew = default;
            for (int i = 0; i < moves.Length; i++)
            {
                Move move = moves[i];
                board.MakeMove(move);
                bool checkmate = board.IsInCheckmate();
                if (board.IsRepeatedPosition())
                    score = 0;
                else
                    score = board.IsDraw()
                        ? 0
                        : -NegaMax(-CheckmateScore, -alpha, 1, board);
                board.UndoMove(move);
                if (checkmate)
                    return move;
                if (score > alpha)
                {
                    moves[i] = moves[0];
                    bestMove = moves[0] = move;
                    bestNew = bestMove;
                    alpha = score;
                }
            }
            bestMove = bestNew.IsNull ? bestMove : bestNew;
            if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 100)
                break;
        }
        return bestMove;
    }

    private int NegaQ(int alpha, int beta, Board board)
    {
        int eval = Eval(board);
        if (eval >= beta) return beta;
        if (eval > alpha) alpha = eval;
        Move[] move = board.GetLegalMoves();
        for (int i = 0; i < move.Length; i++)
        {
            if (move[i].IsCapture || move[i].IsPromotion || move[i].IsEnPassant)
            {
                Move tempMove;
                Piece capturedPiece1 = board.GetPiece(move[i].TargetSquare);
                int capturedPieceValue1 = PieceValues[(int)capturedPiece1.PieceType];
                for (int j = i + 1; j < move.Length; j++)
                {
                    Piece capturedPiece2 = board.GetPiece(move[j].TargetSquare);
                    int capturedPieceValue2 = PieceValues[(int)capturedPiece2.PieceType];
                    if (capturedPieceValue2 > capturedPieceValue1)
                    {
                        tempMove = move[i];
                        move[i] = move[j];
                        move[j] = tempMove;
                    }
                }

                board.MakeMove(move[i]);
                if (board.IsInCheckmate())
                {
                    board.UndoMove(move[i]);
                    return CheckmateScore - board.PlyCount;
                }
                if (board.IsDraw())
                {
                    board.UndoMove(move[i]);
                    return 0;
                }
                int score = -NegaQ(-beta, -alpha, board);
                board.UndoMove(move[i]);

                if (score > alpha)
                {
                    alpha = score;
                    if (score >= beta)
                        return beta;
                }
            }
        }
        return alpha;
    }

    private int NegaMax(int alpha, int beta, int depth, Board board)
    {
        int score;
        if (depth >= Depth)
            return NegaQ(alpha, beta, board);

        int bestScore = -CheckmateScore;
        Move[] move = board.GetLegalMoves();
        for (int i = 0; i < move.Length; i++)
        {
            Move tempMove;
            Piece capturedPiece1 = board.GetPiece(move[i].TargetSquare);
            int capturedPieceValue1 = PieceValues[(int)capturedPiece1.PieceType];
            for (int j = i + 1; j < move.Length; j++)
            {
                Piece capturedPiece2 = board.GetPiece(move[j].TargetSquare);
                int capturedPieceValue2 = PieceValues[(int)capturedPiece2.PieceType];
                if (capturedPieceValue2 > capturedPieceValue1)
                {
                    tempMove = move[i];
                    move[i] = move[j];
                    move[j] = tempMove;
                }
            }

            board.MakeMove(move[i]);
            if (board.IsInCheckmate())
            {
                board.UndoMove(move[i]);
                return CheckmateScore - board.PlyCount;
            }
            if (board.IsDraw())
            {
                board.UndoMove(move[i]);
                return 0;
            }
            if (beta - alpha > 1)
            {
                score = -NegaMax(-alpha - 1, -alpha, depth + 2, board);
                if (score > alpha)
                    score = -NegaMax(-beta, -alpha, depth + 1, board);
            }
            else
            {
                score = -NegaMax(-beta, -alpha, depth + 1, board);
            }
            board.UndoMove(move[i]);

            if (score > alpha)
            {
                alpha = score;
                if (score >= beta)
                    return beta;
            }
        }
        return alpha;
    }

    int Eval(Board board)
    {
        int eval = 0, value;
        foreach (PieceList list in board.GetAllPieceLists())
            foreach (Piece piece in list)
            {
                value = (byte)(pst[piece.Square.Index ^ (piece.IsWhite ? 0 : 0x38)] >>
                    ((int)piece.PieceType << 3));
                eval -= (board.IsWhiteToMove ^ piece.IsWhite) ? value : -value;
            }
        return eval;
    }
}
