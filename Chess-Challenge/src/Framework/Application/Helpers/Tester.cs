using ChessChallenge.API;
using ChessChallenge.Application.APIHelpers;
using ChessChallenge.Chess;
using System;

namespace ChessChallenge.Application
{
    public static class Tester
    {
        const bool throwOnAssertFail = false;

        static MoveGenerator moveGen;
        static API.Board boardAPI;

        static bool anyFailed;

        public static void Run(bool runPerft)
        {
            anyFailed = false;
            
            new SearchTest().Run(false);
            new SearchTest().Run(true);
            new SearchTest2().Run();
            new SearchTest3().Run();
           
            RepetitionTest();
            DrawTest();
            MoveGenTest();
            PieceListTest();
            CheckTest();
            MiscTest();
            TestBitboards();
            TestMoveCreate();

            if (runPerft)
            {
                RunPerft(true);
                RunPerft(false);
            }

            if (anyFailed)
            {
                WriteWithCol("TEST FAILED");
            }
            else
            {
                WriteWithCol("ALL TESTS PASSED", ConsoleColor.Green);
            }
        }

        public static void RunPerft(bool useStackalloc = true)
        {
            Warmer.Warm();
            int[] depths = { 5, 5, 6, 5, 5, 4, 5, 4, 6, 6, 6, 7, 4, 5, 6, 5, 6, 6, 10, 7, 6, 5, 4, 5, 4, 6, 6, 9, 4, 5 };
            ulong[] expectedNodes = { 4865609, 5617302, 11030083, 15587335, 89941194, 3894594, 193690690, 497787, 1134888, 1440467, 661072, 15594314, 1274206, 58773923, 3821001, 1004658, 217342, 92683, 5966690, 567584, 3114998, 42761834, 3050662, 10574719, 6871272, 71179139, 28859283, 7618365, 28181, 6323457 };
            string[] fens = { "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", "2b1b3/1r1P4/3K3p/1p6/2p5/6k1/1P3p2/4B3 w - - 0 42", "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -", "r3k2r/pp3pp1/PN1pr1p1/4p1P1/4P3/3P4/P1P2PP1/R3K2R w KQkq - 4 4", "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", "r3k1nr/p2pp1pp/b1n1P1P1/1BK1Pp1q/8/8/2PP1PPP/6N1 w kq - 0 1", "3k4/3p4/8/K1P4r/8/8/8/8 b - - 0 1", "8/8/1k6/2b5/2pP4/8/5K2/8 b - d3 0 1", "5k2/8/8/8/8/8/8/4K2R w K - 0 1", "3k4/8/8/8/8/8/8/R3K3 w Q - 0 1", "r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", "r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1", "2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1", "8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1", "4k3/1P6/8/8/8/8/K7/8 w - - 0 1", "8/P1k5/K7/8/8/8/8/8 w - - 0 1", "K1k5/8/P7/8/8/8/8/8 w - - 0 1", "8/k1P5/8/1K6/8/8/8/8 w - - 0 1", "8/8/2k5/5q2/5n2/8/5K2/8 b - - 0 1", "r1bq2r1/1pppkppp/1b3n2/pP1PP3/2n5/2P5/P3QPPP/RNB1K2R w KQ a6 0 12", "r3k2r/pppqbppp/3p1n1B/1N2p3/1nB1P3/3P3b/PPPQNPPP/R3K2R w KQkq - 11 10", "4k2r/1pp1n2p/6N1/1K1P2r1/4P3/P5P1/1Pp4P/R7 w k - 0 6", "1Bb3BN/R2Pk2r/1Q5B/4q2R/2bN4/4Q1BK/1p6/1bq1R1rb w - - 0 1", "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - - 0 1", "8/PPPk4/8/8/8/8/4Kppp/8 b - - 0 1", "8/2k1p3/3pP3/3P2K1/8/8/8/8 w - - 0 1", "3r4/2p1p3/8/1P1P1P2/3K4/5k2/8/8 b - - 0 1", "8/1p4p1/8/q1PK1P1r/3p1k2/8/4P3/4Q3 b - - 0 1" };
            Console.WriteLine($"Running perft (useStackalloc={useStackalloc})");

            var board = new Chess.Board();
            long timeTotal = 0;

            for (int i = 0; i < fens.Length; i++)
            {
                board.LoadPosition(fens[i]);
                boardAPI = new(board);
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                ulong result;
                if (useStackalloc)
                {
                    result = SearchStackalloc(depths[i]);
                }
                else
                {
                    result = Search(depths[i]);
                }

                if (result != expectedNodes[i])
                {
                    Console.WriteLine("Error");
                    anyFailed = true;
                    break;
                }
                else
                {
                    sw.Stop();
                    timeTotal += sw.ElapsedMilliseconds;
                    Console.WriteLine(i + " successful: " + sw.ElapsedMilliseconds + " ms");
                }

            }
            Console.WriteLine("Time Total: " + timeTotal + " ms.");

        }

        static void TestMoveCreate()
        {
            Console.WriteLine("Testing move create");
            var board = new Chess.Board();
            board.LoadPosition("2rqk2r/1p3p1p/p2p1n2/2PPn3/8/3B1QP1/PR1K1P1p/2B1R3 b k - 1 27");
            boardAPI = new(board);

            var move = new API.Move("b7b5", boardAPI);
            boardAPI.MakeMove(move);
            move = new API.Move("c5b6", boardAPI);
            Assert(move.IsEnPassant, "En passant wrong");
            move = new API.Move("h2h1q", boardAPI);
            Assert(move.IsPromotion && move.PromotionPieceType == PieceType.Queen, "Promotion wrong");
            move = new API.Move("e8g8", boardAPI);
            Assert(move.IsCastles && move.MovePieceType == PieceType.King, "Castles wrong");
        }

        static void TestBitboards()
        {

            Console.WriteLine("Testing Bitboards");
            var board = new Chess.Board();
            board.LoadPosition("r2q2k1/pp2rppp/3p1n2/1R1Pn3/8/2PB1Q1P/P4PP1/2B2RK1 w - - 7 16");
            boardAPI = new(board);

            ulong rookTest = BitboardHelper.GetSliderAttacks(PieceType.Rook, new Square("b5"), boardAPI);
            Assert(BitboardHelper.GetNumberOfSetBits(rookTest) == 9, "Bitboard error");
            ulong queenTest = BitboardHelper.GetSliderAttacks(PieceType.Queen, new Square("f3"), boardAPI);
            Assert(BitboardHelper.GetNumberOfSetBits(queenTest) == 15, "Bitboard error");
            ulong bishopTest = BitboardHelper.GetSliderAttacks(PieceType.Bishop, new Square("d3"), boardAPI);
            Assert(BitboardHelper.GetNumberOfSetBits(bishopTest) == 10, "Bitboard error");
            ulong pawnTest = BitboardHelper.GetPawnAttacks(new Square("c3"), true);
            Assert(BitboardHelper.SquareIsSet(pawnTest, new Square("b4")), "Pawn bitboard error");
            Assert(BitboardHelper.SquareIsSet(pawnTest, new Square("d4")), "Pawn bitboard error");
            ulong knightTest = BitboardHelper.GetKnightAttacks(new Square("a1"));
            Assert(BitboardHelper.GetNumberOfSetBits(knightTest) == 2, "Knight bb wrong");
            ulong king = BitboardHelper.GetKingAttacks(new Square("a1"));
            Assert(BitboardHelper.GetNumberOfSetBits(king) == 3, "King bb wrong");

            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("a6")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("f3")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("c5")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("c3")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("h4")), "Square attacked wrong");
            var m1 = new API.Move("b5b7", boardAPI);
            boardAPI.MakeMove(m1);
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("e7")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("b8")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("d4")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("h6")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("a5")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("b7")), "Square attacked wrong");
            var m2 = new API.Move("f6e4", boardAPI);
            boardAPI.MakeMove(m2);
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("f2")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("c3")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("h6")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("h4")), "Square attacked wrong");

            boardAPI.ForceSkipTurn();

            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("f7")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("d5")), "Square attacked wrong");

            boardAPI.UndoSkipTurn();

            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("c5")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("c3")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("h5")), "Square attacked wrong");

            boardAPI.UndoMove(m2);
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("b1")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("a5")), "Square attacked wrong");

            boardAPI.UndoMove(m1);
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("a5")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("f8")), "Square attacked wrong");

        }

        static void CheckTest()
        {
            Console.WriteLine("Testing Checks");
            var board = new Chess.Board();
            board.LoadPosition("r2q1rk1/pp3ppp/3p1n2/3Pn3/8/2PB1Q1P/P4PP1/R1B2RK1 w - - 3 14");
            boardAPI = new(board);

            Assert(!boardAPI.IsInCheck(), "Check wrong");
            API.Move move = new API.Move("d3h7", boardAPI);
            boardAPI.MakeMove(move);
            Assert(boardAPI.IsInCheck(), "Check wrong");
            boardAPI.UndoMove(move);
            Assert(!boardAPI.IsInCheck(), "Check wrong");

            boardAPI.MakeMove(new API.Move("f3h5", boardAPI));
            boardAPI.MakeMove(new API.Move("f6d7", boardAPI));
            Assert(!boardAPI.IsInCheckmate(), "Checkmate wrong");
            boardAPI.MakeMove(new API.Move("h5h7", boardAPI));
            Assert(boardAPI.IsInCheckmate(), "Checkmate wrong");

        }
        static void PieceListTest()
        {
            Console.WriteLine("Piece Lists Tests");
            var board = new Chess.Board();
            board.LoadPosition("1q3rk1/P5p1/4p2p/2ppP1N1/5Qb1/1PP5/7P/2R2RK1 w - - 0 28");
            boardAPI = new(board);

            API.PieceList[] pieceLists = boardAPI.GetAllPieceLists();
            int[] counts = { 5, 1, 0, 2, 1, 1, 5, 0, 1, 1, 1, 1 };
            for (int i = 0; i < pieceLists.Length; i++)
            {
                string msg = $"Wrong piece count: {pieceLists[i].Count} Type: {pieceLists[i].TypeOfPieceInList}";
                Assert(pieceLists[i].Count == counts[i], msg);
            }

            Assert(boardAPI.GetKingSquare(true) == boardAPI.GetPieceList(PieceType.King, true)[0].Square, "King square wrong");
            Assert(boardAPI.GetKingSquare(true) == new Square(6, 0), "King square wrong");
            Assert(boardAPI.GetKingSquare(false) == boardAPI.GetPieceList(PieceType.King, false)[0].Square, "King square wrong");
            Assert(boardAPI.GetKingSquare(false) == new Square(6, 7), "King square wrong");
            Assert(boardAPI.GetPiece(new Square(4, 5)).IsPawn, "Wrong piece");
            Assert(!boardAPI.GetPiece(new Square(4, 5)).IsWhite, "Wrong colour");

            API.Move testMove = new("g5e6", boardAPI);
            boardAPI.MakeMove(testMove);
            Assert(boardAPI.GetPiece(new Square("g5")).IsNull, "Wrong piece");
            Assert(boardAPI.GetPiece(new Square("e6")).IsKnight, "Wrong piece");
            Assert(boardAPI.GetPiece(new Square("e6")).IsWhite, "Wrong piece col");
            boardAPI.UndoMove(testMove);
            Assert(boardAPI.GetPiece(new Square("e6")).IsPawn, "Wrong piece");
            Assert(!boardAPI.GetPiece(new Square("e6")).IsWhite, "Wrong piece col");
            Assert(boardAPI.GetPiece(new Square("g5")).IsKnight, "Wrong piece");

        }

        static void RepetitionTest()
        {
            Console.WriteLine("Repetition test");
            string fen = "3k4/8/3K4/8/8/8/8/4Q3 w - - 0 1";
            var board = new Chess.Board();
            board.LoadPosition(fen);
            boardAPI = new(board);

            // -- Simple repeated position in search --
            string[] moveStrings = { "d6c6", "d8c8", "c6d6", "c8d8" };
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            Make(moveStrings[0]); // Kc6
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            Make(moveStrings[1]); // ... Kc8
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            Make(moveStrings[2]); // Kd6
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            var move = Make(moveStrings[3]); // ...Kd8 (repeated position)
            Assert(boardAPI.IsRepeatedPosition(), "should be repetition");
            boardAPI.UndoMove(move); // Undo ...Kd8 (no longer repeated position)
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");

            // -- Repetition of position in actual game occuring in search --
            
            board.LoadPosition(fen);
            board.MakeMove(MoveUtility.GetMoveFromUCIName("e1e2", board), inSearch: false); // Qe2
            board.MakeMove(MoveUtility.GetMoveFromUCIName("d8c8", board), inSearch: false); // ...Kc8
            board.MakeMove(MoveUtility.GetMoveFromUCIName("d6c6", board), inSearch: false); // Kc6
            board.MakeMove(MoveUtility.GetMoveFromUCIName("c8d8", board), inSearch: false); // ...Kd8
            boardAPI = new(board);

            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            var moveKd6 = Make("c6d6"); // Kd6 (repetition of position in game)
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            boardAPI.UndoMove(moveKd6); // Undo Kd6
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            boardAPI.MakeMove(moveKd6); // Redo Kd6
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            var moveKc8 = Make("d8c8"); // ...Kc8
            boardAPI.UndoMove(moveKc8);
            boardAPI.UndoMove(moveKd6);
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            boardAPI.MakeMove(moveKd6);
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            
            // -- Same test but purely in search --
            board.LoadPosition(fen);
            
            boardAPI = new(board);
            Make("e1e2"); // Qe2
            Make("d8c8"); // ...Kc8
            Make("d6c6"); // Kc6
            Make("c8d8"); // ...Kd8

            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            moveKd6 = Make("c6d6"); // Kd6 (repetition of position in game)
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            boardAPI.UndoMove(moveKd6); // Undo Kd6
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            boardAPI.MakeMove(moveKd6); // Redo Kd6
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            moveKc8 = Make("d8c8"); // ...Kc8
            boardAPI.UndoMove(moveKc8);
            boardAPI.UndoMove(moveKd6);
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            boardAPI.MakeMove(moveKd6);
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");

            // Another test
            board.LoadPosition("k7/1p6/2pp4/1QQ5/1b3N2/8/1qq1PPPP/3q2BK w - - 0 1");
            boardAPI = new(board);
            Make("b5a5");
            Make("b4a5");
            Make("c5a5");
            Make("a8b8");
            Make("a5d8");
            Make("b8a7");
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            Make("d8a5");
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            Make("a7b8");
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            Make("a5d8");
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            Make("b8a7");
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            var pawnMove = Make("h2h4");
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            boardAPI.UndoMove(pawnMove);
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            boardAPI.MakeMove(pawnMove);
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            Make("d1c1");
            Make("d8a5");
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            Make("a7b8");
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            Make("a5d8");
            Assert(!boardAPI.IsRepeatedPosition(), "Should not be repetition");
            Make("b8a7");
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");
            Make("d8a5");
            Assert(boardAPI.IsRepeatedPosition(), "Should be repetition");

            API.Move Make(string name)
            {
                var move = new API.Move(name, boardAPI);
                boardAPI.MakeMove(move);
                return move;
            }
        }

        static void DrawTest()
        {
            Console.WriteLine("Draw test");

            // Repetition test
            var board = new Chess.Board();
            board.LoadPosition("r1r3k1/p1q5/3p2pQ/1p1Pp1N1/2B5/1PP2P2/K1b3P1/7R b - - 2 24");
            boardAPI = new(board);

            Assert(!boardAPI.IsDraw(), "Draw wrong");
            boardAPI.MakeMove(new API.Move("c7g7", boardAPI));
            Assert(!boardAPI.IsDraw(), "Draw wrong");
            boardAPI.MakeMove(new API.Move("h6h4", boardAPI));
            Assert(!boardAPI.IsDraw(), "Draw wrong");
            boardAPI.MakeMove(new API.Move("g7c7", boardAPI));
            Assert(!boardAPI.IsDraw(), "Draw wrong");
            boardAPI.MakeMove(new API.Move("h4h6", boardAPI));
            Assert(boardAPI.IsDraw(), "Draw wrong");
            boardAPI.UndoMove(new API.Move("h4h6", boardAPI));
            Assert(!boardAPI.IsDraw(), "Draw wrong");

            // Stalemate test
            board = new Chess.Board();
            board.LoadPosition("7K/8/6k1/5q2/8/8/8/8 b - - 0 1");
            boardAPI = new(board);
            Assert(!boardAPI.IsDraw(), "Draw wrong");
            boardAPI.MakeMove(new API.Move("f5f7", boardAPI));
            Assert(boardAPI.IsDraw(), "Draw wrong");

            // Insufficient material
            board = new Chess.Board();
            board.LoadPosition("7K/3N4/6k1/2n5/8/8/8/8 b - - 0 1");
            boardAPI = new(board);
            Assert(!boardAPI.IsDraw(), "Draw wrong");
            boardAPI.MakeMove(new API.Move("c5d7", boardAPI));
            Assert(boardAPI.IsDraw(), "Draw wrong");

            string[] notInsufficient =
            {
                "3k4/4b3/8/8/8/3B4/1K6/8 w - - 0 1",
                "3k4/3b4/8/8/8/4B3/1K6/8 w - - 0 1",
                "3k4/3b4/8/8/8/2N5/1K6/8 w - - 0 1",
                "3k4/3n4/8/8/8/2N5/1K6/8 w - - 0 1",
                "8/4k3/8/8/8/2NN4/1K6/8 w - - 0 1",
                "8/4k3/8/8/8/8/PK6/8 w - - 0 1",
                "8/4k3/8/8/8/8/1K1R4/8 w - - 0 1",
                "8/4k3/8/8/8/8/1KQ5/8 w - - 0 1"
            };

            string[] insufficient =
            {
                "3k4/8/8/8/8/8/1K6/8 w - - 0 1",
                "3k4/8/8/8/8/2B5/1K6/8 w - - 0 1",
                "3k4/8/8/8/8/8/1KN5/8 w - - 0 1",
                "3k4/2b5/8/8/8/2B5/1K6/8 w - - 0 1",
                "3k4/3b4/8/8/8/3B4/1K6/8 w - - 0 1"
            };

            foreach (string drawPos in insufficient)
            {
                boardAPI = API.Board.CreateBoardFromFEN(drawPos);
                Assert(boardAPI.IsDraw(), "Draw wrong, position is insufficient mat");
                boardAPI = API.Board.CreateBoardFromFEN(FenUtility.FlipFen(drawPos));
                Assert(boardAPI.IsDraw(), "Draw wrong, position is insufficient mat");
            }

            foreach (string winnablePos in notInsufficient)
            {
                boardAPI = API.Board.CreateBoardFromFEN(winnablePos);
                Assert(!boardAPI.IsDraw(), "Draw wrong, position is winnable");
                boardAPI = API.Board.CreateBoardFromFEN(FenUtility.FlipFen(winnablePos));
                Assert(!boardAPI.IsDraw(), "Draw wrong, position is winnable");
            }
        }

        static void MiscTest()
        {
            Console.WriteLine("Running Misc Tests");

            // Captures
            var board = new Chess.Board();
            board.LoadPosition("1q3rk1/P5p1/4p2p/2ppP1N1/5Qb1/1PP5/7P/2R2RK1 w - - 0 28");
            boardAPI = new(board);
            Assert(boardAPI.IsWhiteToMove, "Colour to move wrong");

            var captures = boardAPI.GetLegalMoves(true);
            Assert(captures.Length == 4, "Captures wrong");
            int numTested = 0;
            foreach (var c in captures)
            {
                if (c.TargetSquare.Index == 57)
                {
                    Assert(c.StartSquare == new Square(0, 6), "Start square wrong");
                    Assert(c.CapturePieceType == PieceType.Queen, "Capture type wrong");
                    Assert(c.MovePieceType == PieceType.Pawn, "Move piece type wrong");
                    Assert(c.PromotionPieceType == PieceType.Queen, "Promote type wrong");
                    numTested++;
                }
                if (c.TargetSquare.Index == 44)
                {
                    Assert(c.StartSquare == new Square(6, 4), "Start square wrong");
                    Assert(c.CapturePieceType == PieceType.Pawn, "Capture type wrong");
                    Assert(c.MovePieceType == PieceType.Knight, "Move piece type wrong");
                    numTested++;
                }
                if (c.TargetSquare.Index == 61)
                {
                    Assert(c.CapturePieceType == PieceType.Rook, "Capture type wrong");
                    Assert(c.MovePieceType == PieceType.Queen, "Move piece type wrong");
                    numTested++;
                }
                if (c.TargetSquare.Index == 30)
                {
                    Assert(c.CapturePieceType == PieceType.Bishop, "Capture type wrong");
                    Assert(c.MovePieceType == PieceType.Queen, "Move piece type wrong");
                    numTested++;
                }
            }
            Assert(numTested == 4, "Target square wrong");

            // Game moves
            string startPos = "r1bqkbnr/pppppppp/2n5/8/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 1 2";
            board.LoadPosition(startPos);
            board.MakeMove(MoveUtility.GetMoveFromUCIName("e4e5", board), false);
            board.MakeMove(MoveUtility.GetMoveFromUCIName("c6e5", board), false);
            var b = new Chess.Board(board);
            boardAPI = new(b);
            Assert(boardAPI.GameMoveHistory[0].MovePieceType is PieceType.Pawn, "Wrong game move history");
            Assert(boardAPI.GameMoveHistory[0].CapturePieceType is PieceType.None, "Wrong game move history");
            Assert(boardAPI.GameMoveHistory[1].MovePieceType is PieceType.Knight, "Wrong game move history");
            Assert(boardAPI.GameMoveHistory[1].CapturePieceType is PieceType.Pawn, "Wrong game move history");
            Assert(boardAPI.GameStartFenString == startPos, "Wrong game start fen");
            Assert(boardAPI.GetFenString() == "r1bqkbnr/pppppppp/8/4n3/8/8/PPPP1PPP/RNBQKBNR w KQkq - 0 3", "Wrong game fen");

            // Test for bug: Invalid target piece in capture when en passant is available
            string[] invalidCaptureFens =
            {
                "r1b1r1k1/ppp2pp1/7p/2Pp4/4q3/PQ6/4PPPP/3RKB1R w K d6 0 16", // c5d6
                "8/8/4k3/8/5PpP/6P1/7K/8 b - f3 0 73" // g4f3
            };
            foreach (var fen in invalidCaptureFens)
            {
                board.LoadPosition(fen);
                boardAPI = new(board);
                captures = boardAPI.GetLegalMoves(true);
                foreach (var c in captures)
                {
                    Assert(c.MovePieceType != PieceType.None, $"Move piece type wrong for move {c}");
                    Assert(c.CapturePieceType != PieceType.None, $"Capture piece type wrong for move {c}");
                }
            }

            board.LoadPosition(invalidCaptureFens[0]);
            board.MakeMove(MoveUtility.GetMoveFromUCIName("c5d6", board), false);
            boardAPI = new(board);
            Assert(boardAPI.GameMoveHistory[0].CapturePieceType == PieceType.Pawn, "Wrong capture type: game history");
            Assert(boardAPI.GameMoveHistory[0].IsEnPassant, "Wrong move flag: move history");

        }

        static void MoveGenTest()
        {
            Console.WriteLine("Running move gen tests");
            Chess.Board board = new();
            moveGen = new();

            string[] testFens =
            {
                "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -",
                "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -",
                "r1bq2r1/1pppkppp/1b3n2/pP1PP3/2n5/2P5/P3QPPP/RNB1K2R w KQ a6 0 12",
                "2b1b3/1r1P4/3K3p/1p6/2p5/6k1/1P3p2/4B3 w - - 0 42"
            };

            int[] testDepths = { 3, 4, 4, 5 };
            ulong[] testResults = { 2812, 4085603, 1280017, 5617302 };


            for (int i = 0; i < testFens.Length; i++)
            {
                board.LoadPosition(testFens[i]);
                boardAPI = new API.Board(board);
                ulong result = Search(testDepths[i]);
                Assert(result == testResults[i], "Movegen test failed");
            }

            board.LoadPosition("r2q2k1/pp2rppp/3p1n2/1R1Pn3/8/2PB1Q1P/P4PP1/2B2RK1 w - - 7 16");
            boardAPI = new(board);

            API.Move m1 = new("f3f6", boardAPI);
            Assert(RecreateOpponentAttackMap() == 18446743649919696896ul, "Wrong attack map");
            Assert(boardAPI.GetLegalMoves().Length == 43, "Wrong move count");
            Assert(boardAPI.GetLegalMoves(true).Length == 3, "Wrong capture count");

            boardAPI.MakeMove(m1);
            Assert(RecreateOpponentAttackMap() == 68361585683595006ul, "Wrong attack map");
            Assert(boardAPI.GetLegalMoves().Length == 31, "Wrong move count");
            Assert(boardAPI.GetLegalMoves(true).Length == 2, "Wrong capture count");
            boardAPI.ForceSkipTurn();
            Assert(RecreateOpponentAttackMap() == 18446743065535709184ul, "Wrong attack map");
            Assert(boardAPI.GetLegalMoves().Length == 48, "Wrong move count");
            Assert(boardAPI.GetLegalMoves(true).Length == 7, "Wrong capture count");
            boardAPI.ForceSkipTurn();
            Assert(RecreateOpponentAttackMap() == 68361585683595006ul, "Wrong attack map");
            Assert(boardAPI.GetLegalMoves().Length == 31, "Wrong move count");
            Assert(boardAPI.GetLegalMoves(true).Length == 2, "Wrong capture count");
            boardAPI.UndoSkipTurn();
            Assert(RecreateOpponentAttackMap() == 18446743065535709184ul, "Wrong attack map");
            Assert(boardAPI.GetLegalMoves().Length == 48, "Wrong move count");
            Assert(boardAPI.GetLegalMoves(true).Length == 7, "Wrong capture count");
            boardAPI.UndoSkipTurn();
            Assert(RecreateOpponentAttackMap() == 68361585683595006ul, "Wrong attack map");
            Assert(boardAPI.GetLegalMoves().Length == 31, "Wrong move count");
            Assert(boardAPI.GetLegalMoves(true).Length == 2, "Wrong capture count");
            boardAPI.UndoMove(m1);
            Assert(RecreateOpponentAttackMap() == 18446743649919696896ul, "Wrong attack map");
            Assert(boardAPI.GetLegalMoves().Length == 43, "Wrong move count");
            Assert(boardAPI.GetLegalMoves(true).Length == 3, "Wrong capture count");

            Span<API.Move> moveList = stackalloc API.Move[218];
            boardAPI.GetLegalMovesNonAlloc(ref moveList);
            Span<API.Move> moveListDupe = stackalloc API.Move[218];
            boardAPI.GetLegalMovesNonAlloc(ref moveListDupe);
            Assert(moveList.Length == 43 && moveListDupe.Length == 43, "Move gen wrong");
            Span<API.Move> moveListAtk = stackalloc API.Move[218];
            boardAPI.GetLegalMovesNonAlloc(ref moveListAtk, true);
            Assert(moveListAtk.Length == 3, "Move gen wrong");
            Assert(RecreateOpponentAttackMap() == 18446743649919696896ul, "Wrong attack map");

            ulong RecreateOpponentAttackMap()
            {
                ulong bb = 0;
                for (int i = 0; i < 64; i++)
                {
                    if (boardAPI.SquareIsAttackedByOpponent(new Square(i)))
                    {
                        BitboardHelper.SetSquare(ref bb, new Square(i));
                    }
                }
                return bb;
            }

        }


        static ulong Search(int depth)
        {
            var moves = boardAPI.GetLegalMoves();

            if (depth == 1)
            {
                return (ulong)moves.Length;
            }
            ulong numLocalNodes = 0;
            for (int i = 0; i < moves.Length; i++)
            {

                boardAPI.MakeMove(moves[i]);

                ulong numNodesFromThisPosition = Search(depth - 1);
                numLocalNodes += numNodesFromThisPosition;

                boardAPI.UndoMove(moves[i]);

            }
            return numLocalNodes;
        }

        static ulong SearchStackalloc(int depth)
        {
            Span<API.Move> moves = stackalloc API.Move[128];
            boardAPI.GetLegalMovesNonAlloc(ref moves);

            if (depth == 1)
            {
                return (ulong)moves.Length;
            }
            ulong numLocalNodes = 0;
            for (int i = 0; i < moves.Length; i++)
            {

                boardAPI.MakeMove(moves[i]);

                ulong numNodesFromThisPosition = SearchStackalloc(depth - 1);
                numLocalNodes += numNodesFromThisPosition;

                boardAPI.UndoMove(moves[i]);

            }
            return numLocalNodes;
        }

        static void Assert(bool condition, string msg)
        {
            if (!condition)
            {
                WriteWithCol(msg);
                anyFailed = true;
                if (throwOnAssertFail)
                {
                    throw new Exception();
                }
            }
        }


        static void WriteWithCol(string msg, ConsoleColor col = ConsoleColor.Red)
        {
            Console.ForegroundColor = col;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public class SearchTest3
        {


            API.Board board;
            int numCaptures;
            int numChecks;
            int numMates;
            int nodes;

            public void Run()
            {
                Console.WriteLine("Running misc search test");
                Chess.Board b = new();
                b.LoadPosition("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ");
                board = new API.Board(b);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                Search(4, API.Move.NullMove);
                sw.Stop();
                Console.WriteLine("Test3 time: " + sw.ElapsedMilliseconds + " ms");
                bool passed = nodes == 4085603 && numCaptures == 757163 && numChecks == 25523 && numMates == 43;
                Assert(passed, "Test3 failed");
                

            }

            void Search(int depth, API.Move prevMove)
            {
              
                Span<API.Move> moveSpan = stackalloc API.Move[256];
                board.GetLegalMovesNonAlloc(ref moveSpan);

                if (depth == 0)
                {
                    if (prevMove.IsCapture)
                    {
                        numCaptures++;
                    }
                    if (board.IsInCheck())
                    {
                        numChecks++;
                    }
                    if (board.IsInCheckmate())
                    {
                        numMates++;
                    }

                    nodes += 1;
                    return;
                }

               
                foreach (var move in moveSpan)
                {
                    board.MakeMove(move);
                    Search(depth - 1, move);
                    board.UndoMove(move);
                }

            }
        }


        public class SearchTest2
        {
            API.Board board;
            int numSkips;
            int numCalls;
            int numMates;
            int numDraws;
            int numExtend;

            public void Run()
            {
                Console.WriteLine("Running misc search test");
                Chess.Board b = new();
                b.LoadPosition("8/2b5/2kp4/2p2K2/7P/1p3RP1/2n3N1/8 w - - 0 1");
                board = new API.Board(b);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                Search(7, -10000, 10000);
                sw.Stop();
                Console.WriteLine("Time: " + sw.ElapsedMilliseconds + " ms");

                long testVal = numCalls + numSkips + numMates + numDraws;
                //17092086 skip: 3740 mate: 31 draw: 3803  extend: 172125
                //Console.WriteLine(numCalls + " skip: " + numSkips + " mate: " + numMates + " draw: " + numDraws + "  extend: " + numExtend);
                Console.WriteLine(testVal);
                bool passed = testVal == 17092086;
                //Assert(passed, "Test failed");

                anyFailed &= passed;

            }

            int Search(int plyRemaining, int alpha, int beta, bool isQ = false)
            {
                numCalls++;


                if (!isQ)
                {
                    if (plyRemaining == 0)
                    {
                        return Search(-1, alpha, beta, true);
                    }

                    if (board.IsInCheckmate())
                    {
                        numMates++;
                        return -10000;
                    }
                    if (board.IsDraw())
                    {
                        numDraws++;
                        return 0;
                    }

                    if ((numCalls % 4 == 0 || numCalls % 9 == 0) && plyRemaining > 2)
                    {
                        if (board.TrySkipTurn())
                        {
                            numSkips++;
                            Search(plyRemaining - 2, -beta, -alpha);
                            board.UndoSkipTurn();
                        }
                    }
                }


                API.Move[] moves;
                if (numCalls % 3 == 0 || numCalls % 7 == 0)
                {
                    Span<API.Move> moveSpan = stackalloc API.Move[256];
                    board.GetLegalMovesNonAlloc(ref moveSpan, isQ);
                    // (don't actually care about allocations here, just testing the func)
                    moves = moveSpan.ToArray();
                }
                else
                {
                    moves = board.GetLegalMoves(isQ);
                }

                if (isQ && moves.Length == 0)
                {
                    int numWhite = BitboardHelper.GetNumberOfSetBits(board.WhitePiecesBitboard);
                    int numBlack = BitboardHelper.GetNumberOfSetBits(board.BlackPiecesBitboard);
                    int e = numWhite - numBlack;
                    return e * (board.IsWhiteToMove ? 1 : -1);
                }

                int best = int.MinValue;
                foreach (var move in moves)
                {
                    board.MakeMove(move);
                    int extend = !isQ && board.IsInCheck() ? 1 : 0;
                    numExtend += extend;
                    int eval = -Search(plyRemaining - 1 + extend, -beta, -alpha, isQ);
                    best = Math.Max(best, eval);
                    board.UndoMove(move);
                    if (eval >= beta)
                    {
                        return eval;
                    }
                    if (eval > alpha)
                    {
                        alpha = eval;

                    }
                }
                return best;

            }
        }

        public class SearchTest
        {
            API.Board board;
            bool useStackalloc;
            int numLeafNodes;
            int numCalls;
            long miscSumTest;

            public void Run(bool useStackalloc)
            {
                this.useStackalloc = useStackalloc;
                Console.WriteLine("Running misc search test | stackalloc = " + useStackalloc);
                Chess.Board b = new();
                b.LoadPosition("2rqk2r/5p1p/p2p1n2/1pPPn3/8/3B1QP1/PR1K1P1p/2B1R3 w k b6 0 28");
                board = new API.Board(b);
                Search(4);

                Assert(miscSumTest == 101146355, "Misc search test failed");
            }

            void Search(int plyRemaining)
            {


                numCalls++;
                var square = new Square(numCalls % 64);
                miscSumTest += (int)board.GetPiece(square).PieceType;
                miscSumTest += board.GetAllPieceLists()[numCalls % 12].Count;
                miscSumTest += (long)(board.ZobristKey % 100);
                miscSumTest += board.IsInCheckmate() ? 1 : 0;

                if (numCalls % 6 == 0)
                {
                    miscSumTest += board.IsInCheck() ? 1 : 0;
                }

                if (numCalls % 18 == 0)
                {
                    miscSumTest += board.SquareIsAttackedByOpponent(square) ? 1 : 0;
                }

                if (plyRemaining == 0)
                {
                    numLeafNodes++;
                    return;
                }

                if (numCalls % 3 == 0 && plyRemaining > 2)
                {
                    if (board.TrySkipTurn())
                    {
                        Search(plyRemaining - 2);
                        board.UndoSkipTurn();
                    }
                }


                API.Move[] moves;
                if (useStackalloc)
                {
                    Span<API.Move> moveSpan = stackalloc API.Move[256];
                    board.GetLegalMovesNonAlloc(ref moveSpan);
                    moves = moveSpan.ToArray(); // (don't actually care about allocations here, just testing the func)
                }
                else
                {
                    moves = board.GetLegalMoves();
                }

                foreach (var move in moves)
                {
                    board.MakeMove(move);
                    Search(plyRemaining - 1);
                    board.UndoMove(move);
                }


            }
        }

    }
}
