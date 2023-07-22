using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    Move bestmoveRoot = Move.NullMove;
    ulong[] hashStack = new ulong[1000];
    int[] pieceVal = {0, 100, 310, 330, 500, 1000};

    struct TTEntry {
        ulong key;
        public Move move;

        public TTEntry(ulong _key, Move _move) {key = _key; move = _move; }
    }

    const int entries = (1 << 18);
    ulong nodes = 0;
    TTEntry[] tt = new TTEntry[entries];

    public int Evaluate(Board board) {
        int eval = 0;
        for(var p = PieceType.Pawn; p <= PieceType.Queen; p++)
            eval += pieceVal[(int)p] * (board.GetPieceList(p, true).Count - board.GetPieceList(p, false).Count);

        if(!board.IsWhiteToMove)
            eval *= -1;
        return eval;
    }
    public int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply) {
        nodes++;
        ulong key = board.ZobristKey;
        hashStack[board.PlyCount] = key;
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

        TTEntry entry = tt[key % entries];
        Move[] moves = board.GetLegalMoves(qsearch);
        int[] scores = new int[moves.Length];
        for(int i = 0; i < moves.Length; i++) {
            if(moves[i] == entry.move) scores[i] = 1000000;
            else scores[i] = 100 * moves[i].TargetSquare.Index - moves[i].StartSquare.Index;
        }
        Move bestMove = Move.NullMove;
        for(int i = 0; i < moves.Length; i++) {
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 0;

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
                bestMove = move;
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
        tt[key % entries] = new TTEntry(key, bestMove);
        return best;
    }
    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;
        for(int depth = 1; depth <= 50; depth++) {
            int score = Search(board, timer, -30000, 30000, depth, 0);
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
                break;
            Console.WriteLine($"info depth {depth} score cp {score} time {timer.MillisecondsElapsedThisTurn} pv {bestmoveRoot} nodes {nodes} nps {nodes * 1000 / (ulong)timer.MillisecondsElapsedThisTurn}");
            bestMove = bestmoveRoot;
        }
        return bestMove;
    }
}