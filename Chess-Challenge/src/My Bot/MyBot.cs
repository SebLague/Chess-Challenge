//#define DEBUG
//#define UCI

#pragma warning disable RCS1001, S125 // Add braces (when expression spans over multiple lines) - Tokens are tokens

using ChessChallenge.API;
using System;
using System.Linq;

using static ChessChallenge.API.BitboardHelper;
using static System.Math;
using static System.BitConverter;

public class MyBot : IChessBot
{
    public /*internal*/ Board _position;
    Timer _timer;
    int _timePerMove, _targetDepth
#if DEBUG || UCI
    , _nodes
#endif
        ;

    readonly int[] _indexes = new int[129];
    readonly Move[] _pVTable = new Move[8_256];   // 128 * (128 + 1) / 2
    //readonly int[,] _previousKillerMoves = new int[2, 128];
    //readonly int[,] _killerMoves = new int[2, 128];
    //readonly int[,] _historyMoves = new int[12, 64];

    bool _isFollowingPV, _isScoringPV;

    /// <summary>
    /// <see cref="_indexes"/> initialization
    /// </summary>
    public MyBot()
    {
        int previousPVIndex = _indexes[0] = 0;

        for (int i = -1; ++i < _indexes.Length - 1;)
            previousPVIndex = _indexes[i + 1] = previousPVIndex + 128 - i;
    }

    public Move Think(Board board, Timer timer)
    {
        _position = board;
        _timer = timer;
        _targetDepth = 1;
        _isScoringPV = false;
        Array.Clear(_pVTable);
        //Array.Clear(_killerMoves);
        //Array.Clear(_historyMoves);

        int movesToGo = 100 - board.PlyCount >> 1,
            alpha = short.MinValue,
            beta = short.MaxValue
#if DEBUG
            , bestEvaluation = 0;
#else
            ;
#endif

        #region Time management

        movesToGo = Clamp(movesToGo, 40, 100);
        _timePerMove = Clamp(timer.MillisecondsRemaining / movesToGo, 1, 2000);

#if DEBUG || UCI
        _nodes = 0;
#endif
#if DEBUG
        Console.WriteLine($"\n[{this.GetType().Name}] Searching {_position.GetFenString()} ({_timePerMove}ms to move)");
#endif

        #endregion

        Move bestMove = new();
        try
        {
            bool isMateDetected;
            int msSpentPerDepth = 0;
            do
            {
                //AspirationWindows_SearchAgain:
                _isFollowingPV = true;
#if DEBUG
                bestEvaluation = NegaMax(0, alpha, beta, false);
#else
                int bestEvaluation = NegaMax(0, alpha, beta, false);
#endif
                isMateDetected = Abs(bestEvaluation) > 27_000;

                //if (!isMateDetected && ((bestEvaluation <= alpha) || (bestEvaluation >= beta)))
                //{
                alpha = short.MinValue;   // We fell outside the window, so try again with a
                beta = short.MaxValue;    // full-width window (and the same depth).

                //    goto AspirationWindows_SearchAgain;
                //}

                bestMove = _pVTable[0];
#if DEBUG || UCI
                Console.WriteLine($"info depth {_targetDepth} score {(isMateDetected ? "mate 99" : $"cp {bestEvaluation}")} nodes {_nodes} nps {Convert.ToInt64(Clamp(_nodes / ((0.001 * _timer.MillisecondsElapsedThisTurn) + 1), 0, long.MaxValue))} time {_timer.MillisecondsElapsedThisTurn} pv {string.Join(' ', _pVTable.TakeWhile(m => !m.IsNull).Select(m => m.ToString()[7..^1]))}");
#endif
                //alpha = bestEvaluation - 50;
                //beta = bestEvaluation + 50;

                //Array.Copy(_killerMoves, _previousKillerMoves, _killerMoves.Length);

                msSpentPerDepth = timer.MillisecondsElapsedThisTurn - msSpentPerDepth;
                ++_targetDepth;
            }
            while (!isMateDetected && msSpentPerDepth < _timePerMove * 0.5);
        }
        catch (Exception
#if DEBUG
        e
#endif
        )
        {
#if DEBUG
            ;
            //Console.WriteLine($"Exception: {e.Message}\n{e.StackTrace}");
#endif
        }

#if DEBUG
        Console.WriteLine($"bestmove {bestMove.ToString()[7..^1]} score cp {bestEvaluation} depth {_targetDepth - 1} time {timer.MillisecondsElapsedThisTurn} nodes {_nodes} pv {string.Join(' ', _pVTable.TakeWhile(m => !m.IsNull).Select(m => m.ToString()[7..^1]))}");
#endif
        return bestMove.IsNull ? board.GetLegalMoves()[0] : bestMove;
    }

    public /*internal */int NegaMax(int ply, int alpha, int beta, bool isQuiescence)
    {
        if (_position.IsDraw()) //  IsFiftyMoveDraw() || IsInsufficientMaterial() || IsRepeatedPosition(), no need to check for stalemate
            return 0;

        if (!isQuiescence && _timer.MillisecondsElapsedThisTurn > _timePerMove)
            throw new();

        //if (!isQuiescence && _position.IsInCheck())    // TODO investigate, this makes the bot suggest null moves either other move
        //    ++_targetDepth;

        // TODO: GetLegalMovesNonAlloc
        //Span<Move> spanLegalMoves = stackalloc Move[256];
        //_position.GetLegalMovesNonAlloc(ref spanLegalMoves);
        //spanLegalMoves.Sort((a, b) => Score(a, ply > Score(b, ply) ? 1 : 0));

        if (!isQuiescence && ply > _targetDepth)
            return _position.GetLegalMoves().Any()//.Length > 0
                 ? NegaMax(ply, alpha, beta, true) // Quiescence
                 : EvaluateFinalPosition(ply);

        int pvIndex = _indexes[ply],
            nextPvIndex = _indexes[ply + 1],
            staticEvaluation = 0,
            kingSquare;
        Move bestMove = _pVTable[pvIndex] = new();

        #region Move sorting

        if (!isQuiescence && _isFollowingPV)
            _isFollowingPV = _position.GetLegalMoves().Any(m => m == _pVTable[ply])
                && (_isScoringPV = true);

        #endregion

        if (isQuiescence)
        {
            #region Static evaluation

            ulong bitboard;
            for (int i = 0; ++i < 6;)
            {
                void Eval(bool localIsWhiteToMove)
                {
                    bitboard = _position.GetPieceBitboard((PieceType)i, localIsWhiteToMove);

                    while (bitboard != default)
                    {
                        var square = ClearAndGetIndexOfLSB(ref bitboard);

                        if (!localIsWhiteToMove)
                            square ^= 56;

                        staticEvaluation += (localIsWhiteToMove ? 1 : -1) * (
                            MaterialScore[i]
                            + Magic[square + 64 * (i - 1)]);
                    }
                }

                Eval(true);
                Eval(false);
            }

            bitboard = _position.GetPieceBitboard(PieceType.King, true);
            kingSquare = ClearAndGetIndexOfLSB(ref bitboard);

            staticEvaluation += _position.GetPieceBitboard(PieceType.Queen, false) > 0      // White king, no black queens
                ? Magic[kingSquare + 320]    // Regular king positional values -  64 * ((int)PieceType(King), after regular tables
                : Magic[kingSquare + 384];   // Endgame king position values - 64 * ((int)PieceType(King) - 1), last regular table

            bitboard = _position.GetPieceBitboard(PieceType.King, false);
            kingSquare = ClearAndGetIndexOfLSB(ref bitboard) ^ 56;

            staticEvaluation -= _position.GetPieceBitboard(PieceType.Queen, true) > 0       // Black king, no white queens
                ? Magic[kingSquare + 320]    // Regular king positional values -  64 * ((int)PieceType(King), after regular tables
                : Magic[kingSquare + 384];   // Endgame king position values - 64 * ((int)PieceType(King) - 1), last regular table

            if (!_position.IsWhiteToMove)
                staticEvaluation = -staticEvaluation;

            #endregion

            // Fail-hard beta-cutoff (updating alpha after this check)
            if (staticEvaluation >= beta)
                return staticEvaluation;

            // Better move
            if (staticEvaluation > alpha)
                alpha = staticEvaluation;
        }

#if UCI || DEBUG
        ++_nodes;
#endif

        var moves = _position.GetLegalMoves(isQuiescence);

        if (isQuiescence && moves.Length == 0)
            return staticEvaluation;

        foreach (var move in moves.OrderByDescending(move => Score(move, ply/*, _killerMoves*/)))
        {
            _position.MakeMove(move);
            var evaluation = -NegaMax(ply + 1, -beta, -alpha, isQuiescence); // Invokes itself, either Negamax or Quiescence
            _position.UndoMove(move);

            // Fail-hard beta-cutoff - refutation found, no need to keep searching this line
            if (evaluation >= beta)
                return beta;
            //if (isNotQuiescence && !move.IsCapture)
            //{
            //    _killerMoves[1, ply] = _killerMoves[0, ply];
            //    _killerMoves[0, ply] = move.RawValue;
            //}

            if (evaluation > alpha)
            {
                alpha = evaluation;
                bestMove = _pVTable[pvIndex] = move;
                CopyPVTableMoves(pvIndex + 1, nextPvIndex, ply);

                // 🔍 History moves
                //if (!move.IsCapture) // No isNotQuiescence check needed, in quiecence there will never be non capure moves
                //{
                //    _historyMoves[(int)move.MovePieceType, move.TargetSquare.Index] += ply << 2;
                //}
            }
        }

        if (bestMove.IsNull && _position.GetLegalMoves().Length == 0)
            return EvaluateFinalPosition(ply);

        // Node fails low
        return alpha;
    }

    public /*internal*/ int Score(Move move, int depth/*, int[,]? killerMoves = null,  int[,]? historyMoves = null*/)
    {
        if (_isScoringPV && move == _pVTable[depth])
        {
            _isScoringPV = false;

            return 20_000;
        }

        if (move.IsCapture)
        {
            int targetPiece = (int)PieceType.Pawn;    // Important to initialize to P or p, due to en-passant captures
            for (int pieceIndex = 0; ++pieceIndex < 7;)
            {
                if (SquareIsSet(
                    _position.GetPieceBitboard((PieceType)pieceIndex, !_position.IsWhiteToMove),
                    move.TargetSquare))
                {
                    targetPiece = pieceIndex;
                    break;
                }
            }

            return 100_000 +
                Magic[441 + targetPiece + 6 * (int)move.MovePieceType];      // MVVLVATest.cs, current expression as a simplification of
                                                                             // 448 + targetPiece - 1 + 6 * ((int)move.MovePieceType - 1)
        }
        //else
        //{
        //    // 1st killer move
        //    if (killerMoves?[0, depth] == move.RawValue)
        //    {
        //        return 9_000;
        //    }

        //    // 2nd killer move
        //    else if (killerMoves?[1, depth] == move.RawValue)
        //    {
        //        return 8_000;
        //    }

        //    // History move
        //    //else if (historyMoves is not null)
        //    //{
        //    //    return historyMoves[(int)move.MovePieceType + offset, move.TargetSquare.Index];
        //    //}

        //    return 0;
        //}

        return 0;
    }

    private void CopyPVTableMoves(int target, int source, int ply)
    {
        if (_pVTable[source].IsNull)
            Array.Clear(_pVTable, target, _pVTable.Length - target);
        else
            Array.Copy(_pVTable, source, _pVTable, target, 128 - ply - 1);
    }

    private int EvaluateFinalPosition(int ply) => _position.IsInCheckmate()
        ? -30_000 + 10 * ply
        : 0;

    /*static*/
    readonly int[] MaterialScore = new[]
{
        0,      // PieceType.Pawn starts at index 1
        100,
        300,
        350,
        500,
        1_000
    };

    #region PQST and MVVLVA

    /// <summary>
    /// P PSQT | N PSQT | B PSQT | R PSQT | Q PSQT | K PSQT | K endgame PSQT | MVVLVA
    /// </summary>
    public /*internal static*/ readonly int[] Magic =
        new[/*41*/]
        {
                24868030789173962581061818970m, 27962881072575429945784425040m, 34164717750142730454335511130m,
                34176901826770794923914716270m, 55925761771157817937832870530m, 18654104760093094720549729460m,
                31081933020509579254836122955m, 26427640099376182114853547620m, 34176783119447850105141355605m,
                23314561296157175206972976750m, 24843805044787745151253497374m, 26427545096476845190073566800m,
                37259544051696060033187863125m, 27968973295717185390385259640m, 27962928018458255659375680085m,
                27950744220335595857669020250m, 34164670342372161863418863455m, 27962928481431090069478466670m,
                34164670342010472015904135770m, 27962928481431090069478466670m, 43497814715160557429687623840m,
                24855917919143983528981335180m, 31063799318045908771826261840m, 29516398159635894078093090660m,
                31069843948224851390503346015m, 24874099122699871243475050340m, 24892304121107408860622049360m,
                35736131866136166722759570020m, 12427947061061072563693168680m, 12427947061061072563693168680m,
                12427947061061072563693168680m, 12427947061061072563693168680m, 34170762287008501567167160154m,
                31075959875794436853749541230m, 43473493878160450689378578025m, 31075959875794436853917972620m,
                31688813875635234031847171167m, 32611465967480339888542278758m, 31990068614427286261316681075m,
                31368671261374232633132022641m, 79228162514264337591623186799m
        }
        .SelectMany(decimal.GetBits)
        .Where((_, i) => i % 4 != 3)        // Removes non-integer part of the Decimal
        .SelectMany(GetBytes)
        //.Where(b => b < byte.MaxValue)    // Removes extra-padding, given the array length could not be multiple of 12
        .Select(b => b - 90)
        .ToArray();

    #endregion
}

#pragma warning restore RCS1001 // Add braces (when expression spans over multiple lines).
