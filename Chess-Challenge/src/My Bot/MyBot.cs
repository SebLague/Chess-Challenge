using ChessChallenge.API;

public class SimpleNegaMax : IChessBot
{
    //Implements the https://en.wikipedia.org/wiki/Negamax algorithm

    // Piece values: null, pawn, knight, bishop, rook, queen
    int[] PieceValues = { 0, 100, 300, 300, 500, 900 };
    int CheckmateScore = 9999;
    int Depth = 4;

    public Move Think(Board board, Timer timer)
    {
        Move bestMove = default;
        int bestScore = -CheckmateScore;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -NegaMax(1, board);
            board.UndoMove(move);

            if (score > bestScore)
            {
                bestMove = move;
                bestScore = score;
            }
        }
        return bestMove;
    }

    private int NegaMax(int depth, Board board)
    {
        if (depth == Depth)
            return Eval(board);

        if (board.IsInCheckmate())
            return -CheckmateScore;

        int bestScore = -CheckmateScore;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -NegaMax(depth + 1, board);
            board.UndoMove(move);

            if (score > bestScore)
                bestScore = score;
        }
        return bestScore;
    }

    private int Eval(Board board)
    {
        int eval = 0;
        for (int pieceType = 1; pieceType < 5; pieceType++)
        {
            var white = board.GetPieceList((PieceType)pieceType, true);
            var black = board.GetPieceList((PieceType)pieceType, false);
            eval += (white.Count - black.Count) * PieceValues[pieceType];
        }
        return board.IsWhiteToMove ? eval : -eval;
    }
}
