// https://github.com/Alex2262/Chess-Challenge

using ChessChallenge.API;
using System;
// ReSharper disable All

public class Alex2262Bot : IChessBot
{

    struct TT_Entry
    {
        public ulong key;
        public Move move;

        public TT_Entry(ulong new_key, Move new_move)
        {
            key = new_key;
            move = new_move;
        }
    };

    const ulong ttLength = 1048576;
    TT_Entry[] transpositionTable = new TT_Entry[ttLength];

    int[] weights = { 0, 106, 415, 454, 586, 1237, 0, 0, 181, 411, 454, 792, 1443, 0, 0, 0, 1, 1, 2, 4, 0 };
    ulong[,] PST = new ulong[12, 8]{
        { 8029759185026510703, 8038223216812650336, 8325317849177550947, 7962782498863477860, 7891864041956277096, 8252742557567647078, 8829280427051901900, 8029759185026510703},
        { 6588893181434163248, 9185234911231634022, 8541507192988400242, 9121351164112632696, 10416440643547203971, 11507202547020306027, 9331089673170348661, 3922698774389736198},
        { 8317146214268172417, 9334411155558793849, 9621240762637386364, 9764227933488185977, 8972182642405901173, 9555974981077011577, 8037413005912148078, 9329307076113366367},
        { 7596002947414059624, 5149425726703167829, 7527882678577030746, 8393428149176262751, 8969354616575784047, 11076794158634993010, 11716040375228205196, 14380179108247871646},
        { 7528170798204677498, 8106905374864472940, 8756539704015748976, 9043926754648290930, 9043375890749550708, 10644739870045468531, 10560001556780180599, 11215868214847107437},
        { 10633095980922740873, 10559080032120111749, 8106024558137340281, 6012132214994936678, 5297219855253537663, 8125548866044671649, 3345791531082359275, 10134889354634396551},
        { 6655295901103053916, 7525644159122373245, 7669470132538537844, 7957975343530080122, 8969041160819015559, 12369012770025748407, 17141190283574434529, 6655295901103053916},
        { 7597424654822303096, 7383779735990860396, 8897851240973894247, 9407067009115129984, 9408754776593240444, 7745216670298440056, 7817253369184879472, 4783810653029102690},
        { 7961372912055449447, 7890434638135063414, 8537281782704603254, 8395143503818818940, 9695833895048285055, 9620683310275658114, 8826627024941907838, 8321102373405883002},
        { 8250449929407987836, 9183532862937134969, 8248194826764123257, 8465496833224247427, 9187766034344936836, 8607640579721233801, 9043359458591804295, 8900662627614099078},
        { 6003951818938213470, 7372194001108425572, 8822689634963249265, 9911460254577952625, 12729606516182517867, 11854758082246835058, 10203349891310849397, 9620385372757334139},
        { 5938977330447736909, 7744373379138222195, 8900117364588184694, 9407624474344520821, 10130172929943966850, 10202791201696026248, 10708595124671712869, 8466928396807854412}
    };

    ulong[] repetitionTable = new ulong[1024];

    Move bestMoveRoot = Move.NullMove;
    int nodes = 0/*, gamePly = 0*/;

    bool stopped = false;

    int Evaluate(Board board)
    {
        int[] scoresMid = { 0, 0 }, scoresEnd = { 0, 0 };
        int gamePhase = 0;

        for (int color = 0; color < 2; color++)
        {
            for (int pieceType = 1; pieceType <= 6; pieceType++)
            {
                ulong pieceBB = board.GetPieceBitboard((PieceType)pieceType, color == 0);

                while (pieceBB != 0)
                {
                    int square = BitboardHelper.ClearAndGetIndexOfLSB(ref pieceBB) ^ (56 * color);
                    scoresMid[color] += weights[pieceType] + (int)(2 * (((PST[pieceType - 1, square / 8] >> (square % 8 * 8)) & 255) - 128));
                    scoresEnd[color] += weights[7 + pieceType] + (int)(2 * (((PST[5 + pieceType, square / 8] >> (square % 8 * 8)) & 255) - 128));

                    gamePhase += weights[14 + pieceType];
                }

            }
        }

        return ((scoresMid[0] - scoresMid[1]) * gamePhase + (scoresEnd[0] - scoresEnd[1]) * (24 - gamePhase)) / 24 * (board.IsWhiteToMove ? 1 : -1);
    }

    public Move Think(Board board, Timer timer)
    {
        //gamePly = 0;
        nodes = 0;
        stopped = false;

        for (int depth = 1; depth <= 64; depth++)
        {
            int returnEval = Negamax(board, timer, -1000000, 1000000, depth, 0);

            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 38) break;

            //Console.WriteLine("info depth " + depth.ToString() + " score cp " + returnEval.ToString() + " nodes " + nodes.ToString() + " nps " + (nodes / Math.Max(timer.MillisecondsElapsedThisTurn, 1) * 1000).ToString());
        }

        return bestMoveRoot.IsNull ? board.GetLegalMoves()[0] : bestMoveRoot;
    }

    public int Negamax(Board board, Timer timer, int alpha, int beta, int depth, int ply)
    {

        if ((nodes & 2047) == 0 && timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 12) stopped = true;

        TT_Entry ttEntry = transpositionTable[board.ZobristKey % ttLength];

        bool qsearch = depth <= 0, inCheck = board.IsInCheck(), pvNode = alpha != beta - 1, useTT = ttEntry.key == board.ZobristKey;

        Move[] moves = board.GetLegalMoves(qsearch);

        if (qsearch)
        {
            int staticEval = Evaluate(board);
            if (staticEval >= beta) return staticEval;
            if (staticEval > alpha) alpha = staticEval;
        }
        else if (moves.Length == 0)
        {
            return inCheck ? -900000 + ply : 0;
        }

        int[] scores = new int[moves.Length];
        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            int score = 0;
            if (useTT && move == ttEntry.move) score += 500000;
            if (move.IsCapture)
                score += 50000 + 5 * weights[(int)move.CapturePieceType] -
                    weights[(int)move.MovePieceType];

            scores[i] = score;
        }

        int bestScore = -1000000;
        Move bestMove = Move.NullMove;
        for (int currentCount = 0; currentCount < moves.Length; currentCount++)
        {

            int bestIndex = currentCount;

            for (int nextCount = currentCount + 1; nextCount < moves.Length; nextCount++)
                if (scores[nextCount] > scores[bestIndex]) bestIndex = nextCount;

            int moveScore = scores[bestIndex];
            scores[bestIndex] = scores[currentCount];
            scores[currentCount] = moveScore;

            Move move = moves[bestIndex];
            moves[bestIndex] = moves[currentCount];
            moves[currentCount] = move;

            board.MakeMove(move);

            nodes++;

            // PVS
            int newAlpha = -beta, newDepth = depth - 1, returnScore;
            if (currentCount == 0 || qsearch) goto Search;

            newAlpha = -alpha - 1;

            // Late Move Reductions
            if (currentCount >= 3 && depth >= 3 && !move.IsCapture)
            {
                newDepth -= 1 + depth / 10 + currentCount / 12;
                if (pvNode) newDepth++;
                if (inCheck) newDepth++;
            }
            //

            newDepth = Math.Min(newDepth, depth - 1);

            Search:
            returnScore = -Negamax(board, timer, newAlpha, -alpha, newDepth, ply + 1);

            // Researches
            if (newDepth != depth - 1 && returnScore > alpha)
            {
                newDepth = depth - 1;
                goto Search;
            }

            if (newAlpha == -alpha - 1 && pvNode && returnScore > alpha && returnScore < beta)
            {
                newAlpha = -beta;
                goto Search;
            }
            //

            board.UndoMove(move);

            if (stopped) return 0;

            // Update Info
            if (returnScore > bestScore)
            {
                bestScore = returnScore;
                bestMove = move;

                if (returnScore > alpha)
                {
                    alpha = returnScore;
                    if (ply == 0) bestMoveRoot = move;

                    if (returnScore >= beta) break;
                }
            }
        }

        transpositionTable[board.ZobristKey % ttLength] = new TT_Entry(board.ZobristKey, bestMove);

        return alpha;
    }

}