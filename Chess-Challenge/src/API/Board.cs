namespace ChessChallenge.API
{
	using ChessChallenge.Application.APIHelpers;
	using ChessChallenge.Chess;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public sealed class Board
	{
		readonly Chess.Board board;
		readonly APIMoveGen moveGen;

		readonly HashSet<ulong> repetitionHistory;
		readonly PieceList[] allPieceLists;
		readonly PieceList[] validPieceLists;

		Move[] cachedLegalMoves;
		bool hasCachedMoves;
		Move[] cachedLegalCaptureMoves;
		bool hasCachedCaptureMoves;
        readonly Move[] movesDest;

        /// <summary>
        /// Create a new board. Note: this should not be used in the challenge,
        /// use the board provided in the Think method instead.
        /// </summary>
        public Board(Chess.Board boardSource)
		{
			// Clone board and create game move history
			board = new Chess.Board();
			board.LoadPosition(boardSource.StartPositionInfo);
			GameMoveHistory = new Move[boardSource.AllGameMoves.Count];

			for (int i = 0; i < boardSource.AllGameMoves.Count; i ++)
			{
				Chess.Move move = boardSource.AllGameMoves[i];
				int movePieceType = PieceHelper.PieceType(board.Square[move.StartSquareIndex]);
				int capturePieceType = move.IsEnPassant ? PieceHelper.Pawn : PieceHelper.PieceType(board.Square[move.TargetSquareIndex]);
				GameMoveHistory[i] = new Move(move, movePieceType, capturePieceType);
				board.MakeMove(move, false);
			}

			// Init move gen
			moveGen = new APIMoveGen();
			cachedLegalMoves = Array.Empty<Move>();
			cachedLegalCaptureMoves = Array.Empty<Move>();
			movesDest = new Move[APIMoveGen.MaxMoves];

			// Init piece lists
			List<PieceList> validPieceLists = new();
			allPieceLists = new PieceList[board.pieceLists.Length];
			for (int i = 0; i < board.pieceLists.Length; i++)
			{
				if (board.pieceLists[i] != null)
				{
					allPieceLists[i] = new PieceList(board.pieceLists[i], this, i);
					validPieceLists.Add(allPieceLists[i]);
				}
			}
			this.validPieceLists = validPieceLists.ToArray();

			// Init rep history
			repetitionHistory = new HashSet<ulong>(board.RepetitionPositionHistory);
			GameRepetitionHistory = repetitionHistory.ToArray();
			repetitionHistory.Remove(board.ZobristKey);
        }

		/// <summary>
		/// Updates the board state with the given move.
		/// The move is assumed to be legal, and may result in errors if it is not.
		/// Can be undone with the UndoMove method.
		/// </summary>
		public void MakeMove(Move move)
		{
			if (!move.IsNull)
			{
				repetitionHistory.Add(board.ZobristKey);
				OnPositionChanged();
				board.MakeMove(new Chess.Move(move.RawValue), inSearch: true);
			}
		}

		/// <summary>
		/// Undo a move that was made with the MakeMove method
		/// </summary>
		public void UndoMove(Move move)
		{
			if (!move.IsNull)
			{
				board.UndoMove(new Chess.Move(move.RawValue), inSearch: true);
                OnPositionChanged();
                repetitionHistory.Remove(board.ZobristKey);
			}
		}

		/// <summary>
		/// Try skip the current turn.
		/// This will fail and return false if in check.
		/// Note: skipping a turn is not allowed in the game, but it can be used as a search technique.
		/// Skipped turns can be undone with UndoSkipTurn()
		/// </summary>
		public bool TrySkipTurn()
		{
			if (IsInCheck())
			{
				return false;
			}
			board.MakeNullMove();
            OnPositionChanged();
            return true;
		}

        /// <summary>
        /// Forcibly skips the current turn.
		/// Unlike TrySkipTurn(), this will work even when in check, which has some dangerous side-effects if done:
		/// 1) Generating 'legal' moves will now include the illegal capture of the king.
		/// 2) If the skipped turn is undone, the board will now incorrectly report that the position is not check.
        /// Note: skipping a turn is not allowed in the game, but it can be used as a search technique.
		/// Skipped turns can be undone with UndoSkipTurn()
        /// </summary>
        public void ForceSkipTurn()
        {
            board.MakeNullMove();
            OnPositionChanged();
        }

        /// <summary>
        /// Undo a turn that was succesfully skipped with TrySkipTurn() or ForceSkipTurn()
        /// </summary>
        public void UndoSkipTurn()
		{
			board.UnmakeNullMove();
            OnPositionChanged();
        }

		/// <summary>
		/// Gets an array of the legal moves in the current position.
		/// Can choose to get only capture moves with the optional 'capturesOnly' parameter.
		/// </summary>
		public Move[] GetLegalMoves(bool capturesOnly = false)
		{
			if (capturesOnly)
			{
				return GetLegalCaptureMoves();
			}

			if (!hasCachedMoves)
			{
                Span<Move> moveSpan = movesDest.AsSpan();
                moveGen.GenerateMoves(ref moveSpan, board, includeQuietMoves: true);
                cachedLegalMoves = moveSpan.ToArray();
                hasCachedMoves = true;
			}

			return cachedLegalMoves;
		}

        /// <summary>
        /// Fills the given move span with legal moves, and slices it to the correct length.
        /// Can choose to get only capture moves with the optional 'capturesOnly' parameter.
		/// This gives the same result as the GetLegalMoves function, but allows you to be more
		/// efficient with memory by allocating moves on the stack rather than the heap.
        /// </summary>
        public void GetLegalMovesNonAlloc(ref Span<Move> moveList, bool capturesOnly = false)
		{
			bool includeQuietMoves = !capturesOnly;
			moveGen.GenerateMoves(ref moveList, board, includeQuietMoves);
		}


		Move[] GetLegalCaptureMoves()
		{
			if (!hasCachedCaptureMoves)
			{
                Span<Move> moveSpan = movesDest.AsSpan();
                moveGen.GenerateMoves(ref moveSpan, board, includeQuietMoves: false);
                cachedLegalCaptureMoves = moveSpan.ToArray();
                hasCachedCaptureMoves = true;
			}
			return cachedLegalCaptureMoves;
		}

		/// <summary>
		/// Test if the player to move is in check in the current position.
		/// </summary>
		public bool IsInCheck() => board.IsInCheck();

		/// <summary>
		/// Test if the current position is checkmate
		/// </summary>
		public bool IsInCheckmate() => IsInCheck() && GetLegalMoves().Length == 0;

        /// <summary>
        /// Test if the current position is a draw due stalemate, repetition, insufficient material, or 50-move rule.
        /// Note: this function will return true if the same position has occurred twice on the board (rather than 3 times,
        /// which is when the game is actually drawn). This quirk is to help bots avoid repeating positions unnecessarily.
        /// </summary>
        public bool IsDraw()
		{
			return IsFiftyMoveDraw() || IsInsufficientMaterial() || IsInStalemate() || IsRepeatedPosition();

			bool IsInStalemate() => !IsInCheck() && GetLegalMoves().Length == 0;
			bool IsFiftyMoveDraw() => board.currentGameState.fiftyMoveCounter >= 100;
		}

		/// <summary>
		/// Test if the current position has occurred at least once before on the board.
		/// This includes both positions in the actual game, and positions reached by
		/// making moves while the bot is thinking.
		/// </summary>
		public bool IsRepeatedPosition() => repetitionHistory.Contains(board.ZobristKey);

		/// <summary>
		/// Test if there are sufficient pieces remaining on the board to potentially deliver checkmate.
		/// If not, the game is automatically a draw.
		/// </summary>
		public bool IsInsufficientMaterial() => Arbiter.InsufficentMaterial(board);

        /// <summary>
        /// Does the given player still have the right to castle kingside?
        /// Note that having the right to castle doesn't necessarily mean castling is legal right now
        /// (for example, a piece might be in the way, or player might be in check, etc).
        /// </summary>
        public bool HasKingsideCastleRight(bool white) => board.currentGameState.HasKingsideCastleRight(white);

		/// <summary>
		/// Does the given player still have the right to castle queenside?
		/// Note that having the right to castle doesn't necessarily mean castling is legal right now
		/// (for example, a piece might be in the way, or player might be in check, etc).
		/// </summary>
		public bool HasQueensideCastleRight(bool white) => board.currentGameState.HasQueensideCastleRight(white);

		/// <summary>
		/// Gets the square that the king (of the given colour) is currently on.
		/// </summary>
		public Square GetKingSquare(bool white)
		{
			int colIndex = white ? Chess.Board.WhiteIndex : Chess.Board.BlackIndex;
			return new Square(board.KingSquare[colIndex]);
		}

        /// <summary>
        /// Gets the piece on the given square. If the square is empty, the piece will have a PieceType of None.
        /// </summary>
        public Piece GetPiece(Square square)
        {
            int p = board.Square[square.Index];
            bool white = PieceHelper.IsWhite(p);
            return new Piece((PieceType)PieceHelper.PieceType(p), white, square);
        }

        /// <summary>
        /// Gets a list of pieces of the given type and colour
        /// </summary>
        public PieceList GetPieceList(PieceType pieceType, bool white)
		{
			return allPieceLists[PieceHelper.MakePiece((int)pieceType, white)];
		}
		/// <summary>
		/// Gets an array of all the piece lists. In order these are:
		/// Pawns(white), Knights (white), Bishops (white), Rooks (white), Queens (white), King (white),
		/// Pawns (black), Knights (black), Bishops (black), Rooks (black), Queens (black), King (black)
		/// </summary>
		public PieceList[] GetAllPieceLists()
		{
			return validPieceLists;
		}
		
		/// <summary>
		/// Is the given square attacked by the opponent?
		/// (opponent being whichever player doesn't currently have the right to move)
		/// </summary>
		public bool SquareIsAttackedByOpponent(Square square)
		{
			return BitboardHelper.SquareIsSet(moveGen.GetOpponentAttackMap(board), square);
		}


		/// <summary>
		/// FEN representation of the current position
		/// </summary>
		public string GetFenString() => FenUtility.CurrentFen(board);

        /// <summary>
        /// 64-bit number where each bit that is set to 1 represents a
        /// square that contains a piece of the given type and colour.
        /// </summary>
        public ulong GetPieceBitboard(PieceType pieceType, bool white)
		{
			return board.pieceBitboards[PieceHelper.MakePiece((int)pieceType, white)];
		}
		/// <summary>
		/// 64-bit number where each bit that is set to 1 represents a square that contains any type of white piece.
		/// </summary>
		public ulong WhitePiecesBitboard => board.colourBitboards[Chess.Board.WhiteIndex];
		/// <summary>
		/// 64-bit number where each bit that is set to 1 represents a square that contains any type of black piece.
		/// </summary>
		public ulong BlackPiecesBitboard => board.colourBitboards[Chess.Board.BlackIndex];

		/// <summary>
		/// 64-bit number where each bit that is set to 1 represents a
		/// square that contains a piece of any type or colour.
		/// </summary>
		public ulong AllPiecesBitboard => board.allPiecesBitboard;


		public bool IsWhiteToMove => board.IsWhiteToMove;

		/// <summary>
		/// Number of ply (a single move by either white or black) played so far
		/// </summary>
		public int PlyCount => board.plyCount;

        /// <summary>
        ///  Number of ply (a single move by either white or black) since the last pawn move or capture.
		///  If this value reaches a hundred (meaning 50 full moves without a pawn move or capture), the game is drawn.
        /// </summary>
        public int FiftyMoveCounter => board.currentGameState.fiftyMoveCounter;

		/// <summary>
		/// 64-bit hash of the current position
		/// </summary>
		public ulong ZobristKey => board.ZobristKey;

		/// <summary>
		/// Zobrist keys for all the positions played in the game so far. This is reset whenever a
		/// pawn move or capture is made, as previous positions are now impossible to reach again.
		/// Note that this is not updated when your bot makes moves on the board while thinking,
		/// but rather only when moves are actually played in the game.
		/// </summary>
		public ulong[] GameRepetitionHistory { get; private set; }

        /// <summary>
        /// FEN representation of the game's starting position.
        /// </summary>
        public string GameStartFenString => board.GameStartFen;

		/// <summary>
		/// All the moves played in the game so far.
		/// This only includes moves played in the actual game, not moves made on the board while the bot is thinking.
		/// </summary>
		public Move[] GameMoveHistory { get; private set; }

        /// <summary>
        /// Creates a board from the given fen string. Please note that this is quite slow, and so it is advised
        /// to use the board given in the Think function, and update it using MakeMove and UndoMove instead.
        /// </summary>
        public static Board CreateBoardFromFEN(string fen)
        {
            Chess.Board boardCore = new Chess.Board();
            boardCore.LoadPosition(fen);
            return new Board(boardCore);
        }

        void OnPositionChanged()
        {
            moveGen.NotifyPositionChanged();
            hasCachedMoves = false;
            hasCachedCaptureMoves = false;
        }

    }
}