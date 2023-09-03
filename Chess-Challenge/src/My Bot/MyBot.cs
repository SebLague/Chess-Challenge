using ChessChallenge.API;

public class MyBot : IChessBot
{
    //Implements the https://en.wikipedia.org/wiki/Negamax algorithm

    // Piece values: null, pawn, knight, bishop, rook, queen
    int[] PieceValues = { 0, 100, 300, 300, 500, 900 };
    int CheckmateScore = 9999;
    int Depth = 6;

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
        Move bestMove = default;
        int bestScore = -CheckmateScore;
        int alpha = -CheckmateScore;
        int beta = CheckmateScore;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -NegaMax(-beta, -alpha, 1, board);
            board.UndoMove(move);

            if (score > bestScore)
            {
                bestMove = move;
                bestScore = alpha = score;
            }
        }
        return bestMove;
    }

    private int NegaQ(int alpha, int beta, Board board)
    {
        int eval = Eval(board);
        if (eval >= beta) return beta;
        if (eval > alpha) alpha = eval;
        foreach (Move move in board.GetLegalMoves())
        {
            if (move.IsCapture || move.IsPromotion || move.IsEnPassant)
            {
                board.MakeMove(move);
                int score = -NegaQ(-beta, -alpha, board);
                board.UndoMove(move);

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
        if (depth == Depth)
            return NegaQ(alpha, beta, board);

        if (board.IsInCheckmate())
            return -CheckmateScore;

        int bestScore = -CheckmateScore;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -NegaMax(-beta, -alpha, depth + 1, board);
            board.UndoMove(move);

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
