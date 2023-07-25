using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChessChallenge.Chess
{
    // Helper class for dealing with FEN strings
    public static class FenUtility
    {
        public const string StartPositionFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        // Load position from fen string
        public static PositionInfo PositionFromFen(string fen)
        {

            PositionInfo loadedPositionInfo = new(fen);
            return loadedPositionInfo;
        }

        /// <summary>
        /// Get the fen string of the current position
        /// When alwaysIncludeEPSquare is true the en passant square will be included
        /// in the fen string even if no enemy pawn is in a position to capture it.
        /// </summary>
        public static string CurrentFen(Board board, bool alwaysIncludeEPSquare = true)
        {
            string fen = "";
            for (int rank = 7; rank >= 0; rank--)
            {
                int numEmptyFiles = 0;
                for (int file = 0; file < 8; file++)
                {
                    int i = rank * 8 + file;
                    int piece = board.Square[i];
                    if (piece != 0)
                    {
                        if (numEmptyFiles != 0)
                        {
                            fen += numEmptyFiles;
                            numEmptyFiles = 0;
                        }
                        bool isBlack = PieceHelper.IsColour(piece, PieceHelper.Black);
                        int pieceType = PieceHelper.PieceType(piece);
                        char pieceChar = ' ';
                        switch (pieceType)
                        {
                            case PieceHelper.Rook:
                                pieceChar = 'R';
                                break;
                            case PieceHelper.Knight:
                                pieceChar = 'N';
                                break;
                            case PieceHelper.Bishop:
                                pieceChar = 'B';
                                break;
                            case PieceHelper.Queen:
                                pieceChar = 'Q';
                                break;
                            case PieceHelper.King:
                                pieceChar = 'K';
                                break;
                            case PieceHelper.Pawn:
                                pieceChar = 'P';
                                break;
                        }
                        fen += (isBlack) ? pieceChar.ToString().ToLower() : pieceChar.ToString();
                    }
                    else
                    {
                        numEmptyFiles++;
                    }

                }
                if (numEmptyFiles != 0)
                {
                    fen += numEmptyFiles;
                }
                if (rank != 0)
                {
                    fen += '/';
                }
            }

            // Side to move
            fen += ' ';
            fen += (board.IsWhiteToMove) ? 'w' : 'b';

            // Castling
            bool whiteKingside = (board.currentGameState.castlingRights & 1) == 1;
            bool whiteQueenside = (board.currentGameState.castlingRights >> 1 & 1) == 1;
            bool blackKingside = (board.currentGameState.castlingRights >> 2 & 1) == 1;
            bool blackQueenside = (board.currentGameState.castlingRights >> 3 & 1) == 1;
            fen += ' ';
            fen += (whiteKingside) ? "K" : "";
            fen += (whiteQueenside) ? "Q" : "";
            fen += (blackKingside) ? "k" : "";
            fen += (blackQueenside) ? "q" : "";
            fen += ((board.currentGameState.castlingRights) == 0) ? "-" : "";

            // En-passant
            fen += ' ';
            int epFileIndex = board.currentGameState.enPassantFile - 1;
            int epRankIndex = (board.IsWhiteToMove) ? 5 : 2;

            bool isEnPassant = epFileIndex != -1;
            bool includeEP = alwaysIncludeEPSquare || EnPassantCanBeCaptured(epFileIndex, epRankIndex, board);
            if (isEnPassant && includeEP)
            {
                fen += BoardHelper.SquareNameFromCoordinate(epFileIndex, epRankIndex);
            }
            else
            {
                fen += '-';
            }

            // 50 move counter
            fen += ' ';
            fen += board.currentGameState.fiftyMoveCounter;

            // Full-move count (should be one at start, and increase after each move by black)
            fen += ' ';
            fen += (board.plyCount / 2) + 1;

            return fen;
        }

        static bool EnPassantCanBeCaptured(int epFileIndex, int epRankIndex, Board board)
        {
            Coord captureFromA = new Coord(epFileIndex - 1, epRankIndex + (board.IsWhiteToMove ? -1 : 1));
            Coord captureFromB = new Coord(epFileIndex + 1, epRankIndex + (board.IsWhiteToMove ? -1 : 1));
            int epCaptureSquare = new Coord(epFileIndex, epRankIndex).SquareIndex;
            int friendlyPawn = PieceHelper.MakePiece(PieceHelper.Pawn, board.MoveColour);



            return CanCapture(captureFromA) || CanCapture(captureFromB);


            bool CanCapture(Coord from)
            {
                bool isPawnOnSquare = board.Square[from.SquareIndex] == friendlyPawn;
                if (from.IsValidSquare() && isPawnOnSquare)
                {
                    Move move = new Move(from.SquareIndex, epCaptureSquare, Move.EnPassantCaptureFlag);
                    board.MakeMove(move);
                    board.MakeNullMove();
                    bool wasLegalMove = !board.CalculateInCheckState();

                    board.UnmakeNullMove();
                    board.UndoMove(move);
                    return wasLegalMove;
                }

                return false;
            }
        }

        public static string FlipFen(string fen)
        {
            string flippedFen = "";
            string[] sections = fen.Split(' ');

            List<char> invertedFenChars = new();
            string[] fenRanks = sections[0].Split('/');

            for (int i = fenRanks.Length - 1; i >= 0; i--)
            {
                string rank = fenRanks[i];
                foreach (char c in rank)
                {
                    flippedFen += InvertCase(c);
                }
                if (i != 0)
                {
                    flippedFen += '/';
                }
            }

            flippedFen += " " + (sections[1][0] == 'w' ? 'b' : 'w');
            string castlingRights = sections[2];
            string flippedRights = "";
            foreach (char c in "kqKQ")
            {
                if (castlingRights.Contains(c))
                {
                    flippedRights += InvertCase(c);
                }
            }
            flippedFen += " " + (flippedRights.Length == 0 ? "-" : flippedRights);

            string ep = sections[3];
            string flippedEp = ep[0] + "";
            if (ep.Length > 1)
            {
                flippedEp += ep[1] == '6' ? '3' : '6';
            }
            flippedFen += " " + flippedEp;
            flippedFen += " " + sections[4] + " " + sections[5];


            return flippedFen;

            char InvertCase(char c)
            {
                if (char.IsLower(c))
                {
                    return char.ToUpper(c);
                }
                return char.ToLower(c);
            }
        }

        public readonly struct PositionInfo
        {
            public readonly string fen;
            public readonly ReadOnlyCollection<int> squares;

            // Castling rights
            public readonly bool whiteCastleKingside;
            public readonly bool whiteCastleQueenside;
            public readonly bool blackCastleKingside;
            public readonly bool blackCastleQueenside;
            // En passant file (1 is a-file, 8 is h-file, 0 means none)
            public readonly int epFile;
            public readonly bool whiteToMove;
            // Number of half-moves since last capture or pawn advance
            // (starts at 0 and increments after each player's move)
            public readonly int fiftyMovePlyCount;
            // Total number of moves played in the game
            // (starts at 1 and increments after black's move)
            public readonly int moveCount;

            public PositionInfo(string fen)
            {
                this.fen = fen;
                int[] squarePieces = new int[64];

                string[] sections = fen.Split(' ');

                int file = 0;
                int rank = 7;

                foreach (char symbol in sections[0])
                {
                    if (symbol == '/')
                    {
                        file = 0;
                        rank--;
                    }
                    else
                    {
                        if (char.IsDigit(symbol))
                        {
                            file += (int)char.GetNumericValue(symbol);
                        }
                        else
                        {
                            int pieceColour = (char.IsUpper(symbol)) ? PieceHelper.White : PieceHelper.Black;
                            int pieceType = char.ToLower(symbol) switch
                            {
                                'k' => PieceHelper.King,
                                'p' => PieceHelper.Pawn,
                                'n' => PieceHelper.Knight,
                                'b' => PieceHelper.Bishop,
                                'r' => PieceHelper.Rook,
                                'q' => PieceHelper.Queen,
                                _ => PieceHelper.None
                            };

                            squarePieces[rank * 8 + file] = pieceType | pieceColour;
                            file++;
                        }
                    }
                }

                squares = new(squarePieces);

                whiteToMove = (sections[1] == "w");

                string castlingRights = sections[2];
                whiteCastleKingside = castlingRights.Contains('K');
                whiteCastleQueenside = castlingRights.Contains('Q');
                blackCastleKingside = castlingRights.Contains('k');
                blackCastleQueenside = castlingRights.Contains('q');

                // Default values
                epFile = 0;
                fiftyMovePlyCount = 0;
                moveCount = 0;

                if (sections.Length > 3)
                {
                    string enPassantFileName = sections[3][0].ToString();
                    if (BoardHelper.fileNames.Contains(enPassantFileName))
                    {
                        epFile = BoardHelper.fileNames.IndexOf(enPassantFileName) + 1;
                    }
                }

                // Half-move clock
                if (sections.Length > 4)
                {
                    int.TryParse(sections[4], out fiftyMovePlyCount);
                }
                // Full move number
                if (sections.Length > 5)
                {
                    int.TryParse(sections[5], out moveCount);
                }
            }
        }
    }
}