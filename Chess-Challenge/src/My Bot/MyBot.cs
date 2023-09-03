using ChessChallenge.API;

public class MyBot : IChessBot
{
    int[] PieceValues = { 0, 100, 320, 330, 500, 900, 10000 };
    int CheckmateScore = 9999;
    int Depth;

    ulong[] pst = {
        0x04BE53FA54055052,0x30C0140E5C47C052,0x2D41E3FA46C97852,0x1DC0D42052090052,
        0x0943C438550C7052,0x1442D3CC50C78052,0x2642C3F85D0A1052,0x2BC2E41059473052,
        0x33BE93F054C840B4,0x24BDA3FA5F4940D8,0x1B3FC42E56CCC88F,0x21C02436580BA8B1,
        0x213F145A62CB4096,0x2343A4406A0C78D0,0x1241D3EE5FCAC074,0x16C374124F8A0047,
        0x20BF43B05749104C,0x313F03E0648C6859,0x264083EE660BB06C,0x1D409402654C9071,
        0x1B41E3DC640D2893,0x2843941467CE908A,0x30430434648CD06B,0x1A43A3DA5ACBE83E,
        0x1CBE638A5A4A4044,0x1B3E63A45C8B105F,0x1F3F13C8600B2058,0x17BF13EE67CC3067,
        0x164003EA648BB069,0x18C12400648CB05E,0x1E3FF3AA5D0B1863,0x134023925ACB383B,
        0x0CBF837259CA2037,0x24BE73865E8AA850,0x17BF83A25E8B084D,0x11BF73B861CAF05E,
        0x0E3FF3CC63CB6863,0x0F3FD3AC5E4B2058,0x14C043C65DCB305C,0x0BBFE38C5C4A4839,
        0x1E3F33605B49D038,0x1E4033885F0A404E,0x1A3F639A5F0AE84E,0x0E3FF3985F0AD848,
        0x0F3FC3C05ECB2055,0x164033BA620B1055,0x1DC0F3B05FCB5073,0x17C063785DCA0846,
        0x25BDE3625C49A02F,0x28BF939A5F08E051,0x2140C3925F4A283E,0x054033A85B4A703B,
        0x0FC093B85D0A8043,0x1D4103D0608B186A,0x29BFE3AE638A1878,0x2940232C5B89F03C,
        0x1DC0039453074052,0x373EF3A05A89E052,0x2B3F83BC57C8B852,0x0A40B3DC56098052,
        0x293F23DA580A0052,0x173E83C85849A852,0x313E23705189F052,0x2C3CF3865609D052,
        0x0039F41A46C6F85E,0x13BBE4144507985E,0x1C3BE4244788605E,0x1C3C341E4847E85E,
        0x1FBC34184887D05E,0x2CBBB4184807F05E,0x273B24104606D05E,0x1CBBC40A4445B05E,
        0x1F39741648480110,0x2DBBC41A4948890B,0x2C3C841A4C0800FC,0x2DBD14164748B8E4,
        0x2DBE23FA498880F1,0x383C1406470800E2,0x30BC641049480903,0x2ABA840646C72919,
        0x2A39440E4AC808BC,0x2DBAE40E484828C2,0x30BB140E4A4918B3,0x2CBD940A4A0910A1,
        0x2F3D740849C8C096,0x3BBCB3FA4BC88093,0x3B3BB3F64A4830B0,0x2BBB13FA4B4780B2,
        0x213AB4084988407E,0x303BE4064C88E076,0x313C041A4D49786B,0x32BD54024C897863,
        0x323E14044DC9785C,0x35BD04024CC92062,0x323E13FE4B09086F,0x26BCC4044AC8386F,
        0x1C39640648C8386B,0x233C440A4B089867,0x2FBBB4104D89485B,0x313D74084F099057,
        0x32BC73F64C094857,0x30BCA3F44CC95056,0x29BCF3F04988E861,0x1FBBF3EA4808385D,
        0x1BB983F847481062,0x23B8D4004988B065,0x2ABB73F64C48C058,0x2FBAE3FE4CC9405F,
        0x30BB13F24D89185E,0x2D3B93E84B08B059,0x28BB23F04888285D,0x20BAD3E046881856,
        0x17B923F446C7786B,0x1FB913F445C82866,0x2738A40048887866,0x2BB984044A08A068,
        0x2C3983EE4B48B86B,0x273913EE4808285E,0x22B843EA46881060,0x1CB883FA43876857,
        0x0AB873EE4487E05E,0x1438C4044807305E,0x1AB924064488105E,0x1FB7D3FE4908505E,
        0x173A33F64808185E,0x1E3883E64648385E,0x193944084907385E,0x0FB7F3D84606C85E
    };

    public Move Think(Board board, Timer timer)
    {
        Move bestMove = default;
        int alpha;
        int beta = CheckmateScore;
        int score;
        Move[] move = board.GetLegalMoves();
        for (Depth = 1; Depth < 99; Depth++)
        {
            alpha = -CheckmateScore;
            bestMove = default;
            for (int i = 0; i < move.Length; i++)
            {
                board.MakeMove(move[i]);
                if (board.IsInCheckmate())
                {
                    board.UndoMove(move[i]);
                    return move[i];
                }
                if (board.IsDraw())
                {
                    score = 0;
                }
                else score = -NegaMax(-beta, -alpha, Depth, board);
                board.UndoMove(move[i]);

                if (score > alpha)
                {
                    bestMove = move[i];
                    move[i] = move[0];
                    move[0] = bestMove;
                    alpha = score;
                }
            }
            if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 100) break;
        }
        return bestMove;
    }

    private int NegaQ(int alpha, int beta, Board board)
    {
        int score;
        int eval = Eval(board);
        if (eval >= beta) return beta;
        if (eval > alpha) alpha = eval;
        Move[] move = board.GetLegalMoves(true);
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
            score = -NegaQ(-beta, -alpha, board);
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

    private int NegaMax(int alpha, int beta, int depth, Board board)
    {
        int score;
        if (depth <= 0)
            return NegaQ(alpha, beta, board);
        int eval = Eval(board);
        int bestScore = -CheckmateScore;
        if (depth > 1 && eval - 10 >= beta && board.TrySkipTurn())
        {
            if (-NegaMax(-beta, -beta + 1, depth - 3 - depth / 4, board) >= beta)
            {
                board.UndoSkipTurn();
                return beta;
            }
            board.UndoSkipTurn();
        }
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
                score = 0;
            else
                if (beta - alpha > 1)
            {
                score = -NegaMax(-alpha - 1, -alpha, depth - 2, board);
                if (score > alpha)
                    score = -NegaMax(-beta, -alpha, depth - 1, board);
            }
            else
            {
                score = -NegaMax(-beta, -alpha, depth - 1, board);
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

    private int Eval(Board board)
    {
        int eval = 0, mg = 0, eg = 0;
        foreach (PieceList list in board.GetAllPieceLists())
        {
            foreach (Piece piece in list)
            {
                int square = piece.Square.Index ^ (piece.IsWhite ? 0x38 : 0),
                    type = (int)piece.PieceType - 1,
                    mul = piece.IsWhite ? 1 : -1;
                mg += mul * (int)(pst[square] >> type * 11 & 0b11111111111);
                eg += mul * (int)(pst[square + 64] >> type * 11 & 0b11111111111);
                eval += 0x00042110 >> type * 4 & 0x0F;
            }
        }
        eval = eval < 24 ? eval : 24;
        return (mg * eval + eg * (24 - eval)) / 24 * (board.IsWhiteToMove ? 1 : -1);
    }
}
