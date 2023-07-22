using ChessChallenge.API;
using ChessChallenge.Chess;
using System;

namespace ChessChallenge.Application
{
    public static class Tester
    {
        static MoveGenerator moveGen;
        static API.Board boardAPI;

        public static void Run()
        {
            MoveGenTest();
            PieceListTest();
            DrawTest();
            CheckTest();
            MiscTest();
            TestBitboards();
            TestMoveCreate();
            Console.WriteLine("Tests Finished");
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
            var board = new Chess.Board();
            board.LoadPosition("1q3rk1/P5p1/4p2p/2ppP1N1/5Qb1/1PP5/7P/2R2RK1 w - - 0 28");
            boardAPI = new(board);
            Assert(boardAPI.IsWhiteToMove, "Colour to move wrong");

            //var moves = boardAPI.GetLegalMoves();
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
        }

        static void MoveGenTest()
        {
            Console.WriteLine("Running move gen tests");
            ChessChallenge.Chess.Board board = new();
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

        static void Assert(bool condition, string msg)
        {
            if (!condition)
            {
                LogError(msg);
            }
        }


        static void LogError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

    }
}
