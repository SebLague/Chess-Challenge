using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    Move bestmoveRoot = Move.NullMove;

    public int Evaluate(Board board) {
        int eval = 0;
        eval += 100 * (board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count);

        eval += 310 * (board.GetPieceList(PieceType.Knight, true).Count - board.GetPieceList(PieceType.Knight, false).Count);
        
        eval += 330 * (board.GetPieceList(PieceType.Bishop, true).Count - board.GetPieceList(PieceType.Bishop, false).Count);

        eval += 500 * (board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count);

        eval += 1000 * (board.GetPieceList(PieceType.Queen, true).Count - board.GetPieceList(PieceType.Queen, false).Count);

        if(!board.IsWhiteToMove)
            eval *= -1;
        return eval;
    }
    public int Search(Board board, Timer timer, int depth, int ply) {
        if(depth == 0)
            return Evaluate(board);
        
        if(board.IsInCheckmate() == true)
            return -30000 + ply;

        Move[] moves = board.GetLegalMoves();
        int best = -30000;
        for(int i = 0; i < moves.Length; i++) {
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 0;

            Move move = moves[i];
            board.MakeMove(move);
            int score = -Search(board, timer, depth - 1, ply + 1);
            board.UndoMove(move);
            if(score > best) {
                best = score;
                if(ply == 0)
                    bestmoveRoot = move;
            }
        }
        return best;
    }
    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;
        for(int depth = 1; depth <= 50; depth++) {
            int score = Search(board, timer, depth, 0);
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
                break;
            bestMove = bestmoveRoot;
        }
        return bestMove;
    }
}