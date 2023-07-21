namespace ChessChallenge.Chess
{
    using System;
    using static PrecomputedMoveData;

    public class MoveGenerator
    {
        public const int MaxMoves = 218;
        //public enum PromotionMode { All, QueenOnly, QueenAndKnight }
        public enum PromotionMode { All, QueenOnly, QueenAndKnight }

        public PromotionMode promotionsToGenerate = PromotionMode.All;

        // ---- Instance variables ----
        bool isWhiteToMove;
        int friendlyColour;
        int opponentColour;
        int friendlyKingSquare;
        int friendlyIndex;
        int enemyIndex;

        bool inCheck;
        bool inDoubleCheck;

        // If in check, this bitboard contains squares in line from checking piece up to king
        // If not in check, all bits are set to 1
        ulong checkRayBitmask;

        ulong pinRays;
        ulong notPinRays;
        ulong opponentAttackMapNoPawns;
        public ulong opponentAttackMap;
        public ulong opponentPawnAttackMap;
        ulong opponentSlidingAttackMap;

        bool generateQuietMoves;
        Board board;
        int currMoveIndex;

        ulong enemyPieces;
        ulong friendlyPieces;
        ulong allPieces;
        ulong emptySquares;
        ulong emptyOrEnemySquares;
        // If only captures should be generated, this will have 1s only in positions of enemy pieces.
        // Otherwise it will have 1s everywhere.
        ulong moveTypeMask;

        public System.Span<Move> GenerateMoves(Board board, bool includeQuietMoves = true)
        {
            System.Span<Move> moves = new Move[MaxMoves];
            return GenerateMoves(board, moves, includeQuietMoves);
        }

        // Generates list of legal moves in current position.
        // Quiet moves (non captures) can optionally be excluded. This is used in quiescence search.
        public System.Span<Move> GenerateMoves(Board board, System.Span<Move> moves, bool includeQuietMoves = true)
        {
            this.board = board;
            generateQuietMoves = includeQuietMoves;

            Init();

            GenerateKingMoves(moves);

            // Only king moves are valid in a double check position, so can return early.
            if (!inDoubleCheck)
            {
                GenerateSlidingMoves(moves);
                GenerateKnightMoves(moves);
                GeneratePawnMoves(moves);
            }

            return moves.Slice(0, currMoveIndex);
        }

        // Note, this will only return correct value after GenerateMoves() has been called in the current position
        public bool InCheck()
        {
            return inCheck;
        }

        void Init()
        {
            // Reset state
            currMoveIndex = 0;
            inCheck = false;
            inDoubleCheck = false;
            checkRayBitmask = 0;
            pinRays = 0;

            // Store some info for convenience
            isWhiteToMove = board.MoveColour == PieceHelper.White;
            friendlyColour = board.MoveColour;
            opponentColour = board.OpponentColour;
            friendlyKingSquare = board.KingSquare[board.MoveColourIndex];
            friendlyIndex = board.MoveColourIndex;
            enemyIndex = 1 - friendlyIndex;

            // Store some bitboards for convenience
            enemyPieces = board.colourBitboards[enemyIndex];
            friendlyPieces = board.colourBitboards[friendlyIndex];
            allPieces = board.allPiecesBitboard;
            emptySquares = ~allPieces;
            emptyOrEnemySquares = emptySquares | enemyPieces;
            moveTypeMask = generateQuietMoves ? ulong.MaxValue : enemyPieces;

            CalculateAttackData();
        }

        void GenerateKingMoves(System.Span<Move> moves)
        {
            ulong legalMask = ~(opponentAttackMap | friendlyPieces);
            ulong kingMoves = Bits.KingMoves[friendlyKingSquare] & legalMask & moveTypeMask;
            while (kingMoves != 0)
            {
                int targetSquare = BitBoardUtility.PopLSB(ref kingMoves);
                moves[currMoveIndex++] = new Move(friendlyKingSquare, targetSquare);
            }

            // Castling
            if (!inCheck && generateQuietMoves)
            {
                ulong castleBlockers = opponentAttackMap | board.allPiecesBitboard;
                if (board.currentGameState.HasKingsideCastleRight(board.IsWhiteToMove))
                {
                    ulong castleMask = board.IsWhiteToMove ? Bits.WhiteKingsideMask : Bits.BlackKingsideMask;
                    if ((castleMask & castleBlockers) == 0)
                    {
                        int targetSquare = board.IsWhiteToMove ? BoardHelper.g1 : BoardHelper.g8;
                        moves[currMoveIndex++] = new Move(friendlyKingSquare, targetSquare, Move.CastleFlag);
                    }
                }
                if (board.currentGameState.HasQueensideCastleRight(board.IsWhiteToMove))
                {
                    ulong castleMask = board.IsWhiteToMove ? Bits.WhiteQueensideMask2 : Bits.BlackQueensideMask2;
                    ulong castleBlockMask = board.IsWhiteToMove ? Bits.WhiteQueensideMask : Bits.BlackQueensideMask;
                    if ((castleMask & castleBlockers) == 0 && (castleBlockMask & board.allPiecesBitboard) == 0)
                    {
                        int targetSquare = board.IsWhiteToMove ? BoardHelper.c1 : BoardHelper.c8;
                        moves[currMoveIndex++] = new Move(friendlyKingSquare, targetSquare, Move.CastleFlag);
                    }
                }
            }
        }

        void GenerateSlidingMoves(System.Span<Move> moves)
        {
            // Limit movement to empty or enemy squares, and must block check if king is in check.
            ulong moveMask = emptyOrEnemySquares & checkRayBitmask & moveTypeMask;

            ulong othogonalSliders = board.FriendlyOrthogonalSliders;
            ulong diagonalSliders = board.FriendlyDiagonalSliders;

            // Pinned pieces cannot move if king is in check
            if (inCheck)
            {
                othogonalSliders &= ~pinRays;
                diagonalSliders &= ~pinRays;
            }

            // Ortho
            while (othogonalSliders != 0)
            {
                int startSquare = BitBoardUtility.PopLSB(ref othogonalSliders);
                ulong moveSquares = Magic.GetRookAttacks(startSquare, allPieces) & moveMask;

                // If piece is pinned, it can only move along the pin ray
                if (IsPinned(startSquare))
                {
                    moveSquares &= alignMask[startSquare, friendlyKingSquare];
                }

                while (moveSquares != 0)
                {
                    int targetSquare = BitBoardUtility.PopLSB(ref moveSquares);
                    moves[currMoveIndex++] = new Move(startSquare, targetSquare);
                }
            }

            // Diag
            while (diagonalSliders != 0)
            {
                int startSquare = BitBoardUtility.PopLSB(ref diagonalSliders);
                ulong moveSquares = Magic.GetBishopAttacks(startSquare, allPieces) & moveMask;

                // If piece is pinned, it can only move along the pin ray
                if (IsPinned(startSquare))
                {
                    moveSquares &= alignMask[startSquare, friendlyKingSquare];
                }

                while (moveSquares != 0)
                {
                    int targetSquare = BitBoardUtility.PopLSB(ref moveSquares);
                    moves[currMoveIndex++] = new Move(startSquare, targetSquare);
                }
            }
        }


        void GenerateKnightMoves(System.Span<Move> moves)
        {
            int friendlyKnightPiece = PieceHelper.MakePiece(PieceHelper.Knight, board.MoveColour);
            // bitboard of all non-pinned knights
            ulong knights = board.pieceBitboards[friendlyKnightPiece] & notPinRays;
            ulong moveMask = emptyOrEnemySquares & checkRayBitmask & moveTypeMask;

            while (knights != 0)
            {
                int knightSquare = BitBoardUtility.PopLSB(ref knights);
                ulong moveSquares = Bits.KnightAttacks[knightSquare] & moveMask;

                while (moveSquares != 0)
                {
                    int targetSquare = BitBoardUtility.PopLSB(ref moveSquares);
                    moves[currMoveIndex++] = new Move(knightSquare, targetSquare);
                }
            }
        }

        void GeneratePawnMoves(System.Span<Move> moves)
        {
            int pushDir = board.IsWhiteToMove ? 1 : -1;
            int pushOffset = pushDir * 8;

            int friendlyPawnPiece = PieceHelper.MakePiece(PieceHelper.Pawn, board.MoveColour);
            ulong pawns = board.pieceBitboards[friendlyPawnPiece];

            ulong promotionRankMask = board.IsWhiteToMove ? Bits.Rank8 : Bits.Rank1;

            ulong singlePush = (BitBoardUtility.Shift(pawns, pushOffset)) & emptySquares;

            ulong pushPromotions = singlePush & promotionRankMask & checkRayBitmask;


            ulong captureEdgeFileMask = board.IsWhiteToMove ? Bits.NotAFile : Bits.NotHFile;
            ulong captureEdgeFileMask2 = board.IsWhiteToMove ? Bits.NotHFile : Bits.NotAFile;
            ulong captureA = BitBoardUtility.Shift(pawns & captureEdgeFileMask, pushDir * 7) & enemyPieces;
            ulong captureB = BitBoardUtility.Shift(pawns & captureEdgeFileMask2, pushDir * 9) & enemyPieces;

            ulong singlePushNoPromotions = singlePush & ~promotionRankMask & checkRayBitmask;

            ulong capturePromotionsA = captureA & promotionRankMask & checkRayBitmask;
            ulong capturePromotionsB = captureB & promotionRankMask & checkRayBitmask;

            captureA &= checkRayBitmask & ~promotionRankMask;
            captureB &= checkRayBitmask & ~promotionRankMask;

            // Single / double push
            if (generateQuietMoves)
            {
                // Generate single pawn pushes
                while (singlePushNoPromotions != 0)
                {
                    int targetSquare = BitBoardUtility.PopLSB(ref singlePushNoPromotions);
                    int startSquare = targetSquare - pushOffset;
                    if (!IsPinned(startSquare) || alignMask[startSquare, friendlyKingSquare] == alignMask[targetSquare, friendlyKingSquare])
                    {
                        moves[currMoveIndex++] = new Move(startSquare, targetSquare);
                    }
                }

                // Generate double pawn pushes
                ulong doublePushTargetRankMask = board.IsWhiteToMove ? Bits.Rank4 : Bits.Rank5;
                ulong doublePush = BitBoardUtility.Shift(singlePush, pushOffset) & emptySquares & doublePushTargetRankMask & checkRayBitmask;

                while (doublePush != 0)
                {
                    int targetSquare = BitBoardUtility.PopLSB(ref doublePush);
                    int startSquare = targetSquare - pushOffset * 2;
                    if (!IsPinned(startSquare) || alignMask[startSquare, friendlyKingSquare] == alignMask[targetSquare, friendlyKingSquare])
                    {
                        moves[currMoveIndex++] = new Move(startSquare, targetSquare, Move.PawnTwoUpFlag);
                    }
                }
            }

            // Captures
            while (captureA != 0)
            {
                int targetSquare = BitBoardUtility.PopLSB(ref captureA);
                int startSquare = targetSquare - pushDir * 7;

                if (!IsPinned(startSquare) || alignMask[startSquare, friendlyKingSquare] == alignMask[targetSquare, friendlyKingSquare])
                {
                    moves[currMoveIndex++] = new Move(startSquare, targetSquare);
                }
            }

            while (captureB != 0)
            {
                int targetSquare = BitBoardUtility.PopLSB(ref captureB);
                int startSquare = targetSquare - pushDir * 9;

                if (!IsPinned(startSquare) || alignMask[startSquare, friendlyKingSquare] == alignMask[targetSquare, friendlyKingSquare])
                {
                    moves[currMoveIndex++] = new Move(startSquare, targetSquare);
                }
            }



            // Promotions
            while (pushPromotions != 0)
            {
                int targetSquare = BitBoardUtility.PopLSB(ref pushPromotions);
                int startSquare = targetSquare - pushOffset;
                if (!IsPinned(startSquare))
                {
                    GeneratePromotions(startSquare, targetSquare, moves);
                }
            }


            while (capturePromotionsA != 0)
            {
                int targetSquare = BitBoardUtility.PopLSB(ref capturePromotionsA);
                int startSquare = targetSquare - pushDir * 7;

                if (!IsPinned(startSquare) || alignMask[startSquare, friendlyKingSquare] == alignMask[targetSquare, friendlyKingSquare])
                {
                    GeneratePromotions(startSquare, targetSquare, moves);
                }
            }

            while (capturePromotionsB != 0)
            {
                int targetSquare = BitBoardUtility.PopLSB(ref capturePromotionsB);
                int startSquare = targetSquare - pushDir * 9;

                if (!IsPinned(startSquare) || alignMask[startSquare, friendlyKingSquare] == alignMask[targetSquare, friendlyKingSquare])
                {
                    GeneratePromotions(startSquare, targetSquare, moves);
                }
            }

            // En passant
            if (board.currentGameState.enPassantFile > 0)
            {
                int epFileIndex = board.currentGameState.enPassantFile - 1;
                int epRankIndex = board.IsWhiteToMove ? 5 : 2;
                int targetSquare = epRankIndex * 8 + epFileIndex;
                int capturedPawnSquare = targetSquare - pushOffset;

                if (BitBoardUtility.ContainsSquare(checkRayBitmask, capturedPawnSquare))
                {
                    ulong pawnsThatCanCaptureEp = pawns & BitBoardUtility.PawnAttacks(1ul << targetSquare, !board.IsWhiteToMove);

                    while (pawnsThatCanCaptureEp != 0)
                    {
                        int startSquare = BitBoardUtility.PopLSB(ref pawnsThatCanCaptureEp);
                        if (!IsPinned(startSquare) || alignMask[startSquare, friendlyKingSquare] == alignMask[targetSquare, friendlyKingSquare])
                        {
                            if (!InCheckAfterEnPassant(startSquare, targetSquare, capturedPawnSquare))
                            {
                                moves[currMoveIndex++] = new Move(startSquare, targetSquare, Move.EnPassantCaptureFlag);
                            }
                        }
                    }
                }
            }
        }

        void GeneratePromotions(int startSquare, int targetSquare, Span<Move> moves)
        {
            moves[currMoveIndex++] = new Move(startSquare, targetSquare, Move.PromoteToQueenFlag);
            // Don't generate non-queen promotions in q-search
            if (generateQuietMoves)
            {
                if (promotionsToGenerate == MoveGenerator.PromotionMode.All)
                {
                    moves[currMoveIndex++] = new Move(startSquare, targetSquare, Move.PromoteToKnightFlag);
                    moves[currMoveIndex++] = new Move(startSquare, targetSquare, Move.PromoteToRookFlag);
                    moves[currMoveIndex++] = new Move(startSquare, targetSquare, Move.PromoteToBishopFlag);
                }
                else if (promotionsToGenerate == MoveGenerator.PromotionMode.QueenAndKnight)
                {
                    moves[currMoveIndex++] = new Move(startSquare, targetSquare, Move.PromoteToKnightFlag);
                }
            }
        }

        bool IsPinned(int square)
        {
            return ((pinRays >> square) & 1) != 0;
        }

        void GenSlidingAttackMap()
        {
            opponentSlidingAttackMap = 0;

            UpdateSlideAttack(board.EnemyOrthogonalSliders, true);
            UpdateSlideAttack(board.EnemyDiagonalSliders, false);

            void UpdateSlideAttack(ulong pieceBoard, bool ortho)
            {
                ulong blockers = board.allPiecesBitboard & ~(1ul << friendlyKingSquare);

                while (pieceBoard != 0)
                {
                    int startSquare = BitBoardUtility.PopLSB(ref pieceBoard);
                    ulong moveBoard = Magic.GetSliderAttacks(startSquare, blockers, ortho);

                    opponentSlidingAttackMap |= moveBoard;
                }
            }
        }

        void CalculateAttackData()
        {
            GenSlidingAttackMap();
            // Search squares in all directions around friendly king for checks/pins by enemy sliding pieces (queen, rook, bishop)
            int startDirIndex = 0;
            int endDirIndex = 8;

            if (board.queens[enemyIndex].Count == 0)
            {
                startDirIndex = (board.rooks[enemyIndex].Count > 0) ? 0 : 4;
                endDirIndex = (board.bishops[enemyIndex].Count > 0) ? 8 : 4;
            }

            for (int dir = startDirIndex; dir < endDirIndex; dir++)
            {
                bool isDiagonal = dir > 3;
                ulong slider = isDiagonal ? board.EnemyDiagonalSliders : board.EnemyOrthogonalSliders;
                if ((dirRayMask[dir, friendlyKingSquare] & slider) == 0)
                {
                    continue;
                }

                int n = numSquaresToEdge[friendlyKingSquare][dir];
                int directionOffset = directionOffsets[dir];
                bool isFriendlyPieceAlongRay = false;
                ulong rayMask = 0;

                for (int i = 0; i < n; i++)
                {
                    int squareIndex = friendlyKingSquare + directionOffset * (i + 1);
                    rayMask |= 1ul << squareIndex;
                    int piece = board.Square[squareIndex];

                    // This square contains a piece
                    if (piece != PieceHelper.None)
                    {
                        if (PieceHelper.IsColour(piece, friendlyColour))
                        {
                            // First friendly piece we have come across in this direction, so it might be pinned
                            if (!isFriendlyPieceAlongRay)
                            {
                                isFriendlyPieceAlongRay = true;
                            }
                            // This is the second friendly piece we've found in this direction, therefore pin is not possible
                            else
                            {
                                break;
                            }
                        }
                        // This square contains an enemy piece
                        else
                        {
                            int pieceType = PieceHelper.PieceType(piece);

                            // Check if piece is in bitmask of pieces able to move in current direction
                            if (isDiagonal && PieceHelper.IsDiagonalSlider(pieceType) || !isDiagonal && PieceHelper.IsOrthogonalSlider(pieceType))
                            {
                                // Friendly piece blocks the check, so this is a pin
                                if (isFriendlyPieceAlongRay)
                                {
                                    pinRays |= rayMask;
                                }
                                // No friendly piece blocking the attack, so this is a check
                                else
                                {
                                    checkRayBitmask |= rayMask;
                                    inDoubleCheck = inCheck; // if already in check, then this is double check
                                    inCheck = true;
                                }
                                break;
                            }
                            else
                            {
                                // This enemy piece is not able to move in the current direction, and so is blocking any checks/pins
                                break;
                            }
                        }
                    }
                }
                // Stop searching for pins if in double check, as the king is the only piece able to move in that case anyway
                if (inDoubleCheck)
                {
                    break;
                }
            }

            notPinRays = ~pinRays;

            ulong opponentKnightAttacks = 0;
            ulong knights = board.pieceBitboards[PieceHelper.MakePiece(PieceHelper.Knight, board.OpponentColour)];
            ulong friendlyKingBoard = board.pieceBitboards[PieceHelper.MakePiece(PieceHelper.King, board.MoveColour)];

            while (knights != 0)
            {
                int knightSquare = BitBoardUtility.PopLSB(ref knights);
                ulong knightAttacks = Bits.KnightAttacks[knightSquare];
                opponentKnightAttacks |= knightAttacks;

                if ((knightAttacks & friendlyKingBoard) != 0)
                {
                    inDoubleCheck = inCheck;
                    inCheck = true;
                    checkRayBitmask |= 1ul << knightSquare;
                }
            }

            // Pawn attacks
            PieceList opponentPawns = board.pawns[enemyIndex];
            opponentPawnAttackMap = 0;

            ulong opponentPawnsBoard = board.pieceBitboards[PieceHelper.MakePiece(PieceHelper.Pawn, board.OpponentColour)];
            opponentPawnAttackMap = BitBoardUtility.PawnAttacks(opponentPawnsBoard, !isWhiteToMove);
            if (BitBoardUtility.ContainsSquare(opponentPawnAttackMap, friendlyKingSquare))
            {
                inDoubleCheck = inCheck; // if already in check, then this is double check
                inCheck = true;
                ulong possiblePawnAttackOrigins = board.IsWhiteToMove ? Bits.WhitePawnAttacks[friendlyKingSquare] : Bits.BlackPawnAttacks[friendlyKingSquare];
                ulong pawnCheckMap = opponentPawnsBoard & possiblePawnAttackOrigins;
                checkRayBitmask |= pawnCheckMap;
            }

            int enemyKingSquare = board.KingSquare[enemyIndex];

            opponentAttackMapNoPawns = opponentSlidingAttackMap | opponentKnightAttacks | Bits.KingMoves[enemyKingSquare];
            opponentAttackMap = opponentAttackMapNoPawns | opponentPawnAttackMap;

            if (!inCheck)
            {
                checkRayBitmask = ulong.MaxValue;
            }
        }

        // Test if capturing a pawn with en-passant reveals a sliding piece attack against the king
        // Note: this is only used for cases where pawn appears to not be pinned due to opponent pawn being on same rank
        // (therefore only need to check orthogonal sliders)
        bool InCheckAfterEnPassant(int startSquare, int targetSquare, int epCaptureSquare)
        {
            ulong enemyOrtho = board.EnemyOrthogonalSliders;

            if (enemyOrtho != 0)
            {
                ulong maskedBlockers = (allPieces ^ (1ul << epCaptureSquare | 1ul << startSquare | 1ul << targetSquare));
                ulong rookAttacks = Magic.GetRookAttacks(friendlyKingSquare, maskedBlockers);
                return (rookAttacks & enemyOrtho) != 0;
            }

            return false;
        }
    }

}