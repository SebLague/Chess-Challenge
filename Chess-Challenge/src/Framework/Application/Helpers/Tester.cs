using ChessChallenge.API;
using ChessChallenge.Chess;
using System;

namespace ChessChallenge.Application
{
    public static class Tester
    {
        static MoveGenerator moveGen;
        static API.Board boardAPI;

        static bool anyFailed;

        public static void Run(bool runPerft)
        {
            anyFailed = false;

            MoveGenTest();
            PieceListTest();
            DrawTest();
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
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("c3")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("h4")), "Square attacked wrong");
            boardAPI.MakeMove(new API.Move("b5b7", boardAPI));
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("e7")), "Square attacked wrong");
            Assert(boardAPI.SquareIsAttackedByOpponent(new Square("b8")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("a5")), "Square attacked wrong");
            Assert(!boardAPI.SquareIsAttackedByOpponent(new Square("e8")), "Square attacked wrong");
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
                Assert(result == testResults[i], "TEST FAILED");
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
            }
        }


        static void WriteWithCol(string msg, ConsoleColor col = ConsoleColor.Red)
        {
            Console.ForegroundColor = col;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

    }
}
