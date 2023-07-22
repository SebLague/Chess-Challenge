using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    Move bestmoveRoot = Move.NullMove;
    ulong[] hashStack = new ulong[1000];

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
    public int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply) {
        hashStack[board.PlyCount] = board.ZobristKey;
        bool qsearch = (depth <= 0);
        int best = -30000;

        if(ply > 0) {
            for(int i = board.PlyCount - 2; i >= 0; i -= 2) {
                if(hashStack[i] == hashStack[board.PlyCount])
                    return 0;
            }
        }

        if(qsearch) {
            best = Evaluate(board);
            if(best >= beta) return best;
            if(best > alpha) alpha = best;
        }

        Move[] moves = board.GetLegalMoves(qsearch);
        int[] scores = new int[moves.Length];
        for(int i = 0; i < moves.Length; i++)
            scores[i] = 100 * moves[i].TargetSquare.Index - moves[i].StartSquare.Index;
        for(int i = 0; i < moves.Length; i++) {
            if(depth > 2 && timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 0;

            int ind = i;
            for(int j = i + 1; j < moves.Length; j++) {
                if(scores[j] > scores[ind]) ind = j;
            }
            (scores[i], scores[ind]) = (scores[ind], scores[i]);
            (moves[i], moves[ind]) = (moves[ind], moves[i]);

            Move move = moves[i];
            board.MakeMove(move);
            int score = -Search(board, timer, -beta, -alpha, depth - 1, ply + 1);
            board.UndoMove(move);
            if(score > best) {
                best = score;
                if(ply == 0)
                    bestmoveRoot = move;
                if(score > alpha) {
                    alpha = score;
                    if(alpha >= beta) break;
                }
            }
        }
        if(!qsearch && moves.Length == 0) {
            if(board.IsInCheck()) return -30000 + ply;
            else return 0;
        }
        return best;
    }
    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;
        for(int depth = 1; depth <= 50; depth++) {
            int score = Search(board, timer, -30000, 30000, depth, 0);
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
                break;
            Console.WriteLine($"info depth {depth} score cp {score} time {timer.MillisecondsElapsedThisTurn} pv {bestmoveRoot}");
            bestMove = bestmoveRoot;
        }
        return bestMove;
    }
}