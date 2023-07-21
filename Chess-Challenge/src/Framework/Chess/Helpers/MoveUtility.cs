namespace ChessChallenge.Chess
{
	// Helper class for converting between various move representations:
	// UCI: move represented by string, e.g. "e2e4"
	// SAN: move represented in standard notation e.g. "Nxe7+"
	// Move: internal move representation
	public static class MoveUtility
	{
		// Converts a moveName into internal move representation
		// Name is expected in UCI format: "e2e4"
		// Promotions can be written with or without equals sign, for example: "e7e8=q" or "e7e8q"
		public static Move GetMoveFromUCIName(string moveName, Board board)
		{

			int startSquare = BoardHelper.SquareIndexFromName(moveName.Substring(0, 2));
			int targetSquare = BoardHelper.SquareIndexFromName(moveName.Substring(2, 2));

			int movedPieceType = PieceHelper.PieceType(board.Square[startSquare]);
			Coord startCoord = new Coord(startSquare);
			Coord targetCoord = new Coord(targetSquare);

			// Figure out move flag
			int flag = Move.NoFlag;

			if (movedPieceType == PieceHelper.Pawn)
			{
				// Promotion
				if (moveName.Length > 4)
				{
					flag = moveName[^1] switch
					{
						'q' => Move.PromoteToQueenFlag,
						'r' => Move.PromoteToRookFlag,
						'n' => Move.PromoteToKnightFlag,
						'b' => Move.PromoteToBishopFlag,
						_ => Move.NoFlag
					};
				}
				// Double pawn push
				else if (System.Math.Abs(targetCoord.rankIndex - startCoord.rankIndex) == 2)
				{
					flag = Move.PawnTwoUpFlag;
				}
				// En-passant
				else if (startCoord.fileIndex != targetCoord.fileIndex && board.Square[targetSquare] == PieceHelper.None)
				{
					flag = Move.EnPassantCaptureFlag;
				}
			}
			else if (movedPieceType == PieceHelper.King)
			{
				if (System.Math.Abs(startCoord.fileIndex - targetCoord.fileIndex) > 1)
				{
					flag = Move.CastleFlag;
				}
			}

			return new Move(startSquare, targetSquare, flag);
		}

		// Get name of move in UCI format
		// Examples: "e2e4", "e7e8q"
		public static string GetMoveNameUCI(Move move)
		{
			if (move.IsNull)
			{
				return "Null";
			}
			string startSquareName = BoardHelper.SquareNameFromIndex(move.StartSquareIndex);
			string endSquareName = BoardHelper.SquareNameFromIndex(move.TargetSquareIndex);
			string moveName = startSquareName + endSquareName;
			if (move.IsPromotion)
			{
				switch (move.MoveFlag)
				{
					case Move.PromoteToRookFlag:
						moveName += "r";
						break;
					case Move.PromoteToKnightFlag:
						moveName += "n";
						break;
					case Move.PromoteToBishopFlag:
						moveName += "b";
						break;
					case Move.PromoteToQueenFlag:
						moveName += "q";
						break;
				}
			}
			return moveName;
		}

		// Get name of move in Standard Algebraic Notation (SAN)
		// Examples: "e4", "Bxf7+", "O-O", "Rh8#", "Nfd2"
		// Note, the move must not yet have been made on the board
		public static string GetMoveNameSAN(Move move, Board board)
		{
			if (move.IsNull)
			{
				return "Null";
			}
			int movePieceType = PieceHelper.PieceType(board.Square[move.StartSquareIndex]);
			int capturedPieceType = PieceHelper.PieceType(board.Square[move.TargetSquareIndex]);

			if (move.MoveFlag == Move.CastleFlag)
			{
				int delta = move.TargetSquareIndex - move.StartSquareIndex;
				if (delta == 2)
				{
					return "O-O";
				}
				else if (delta == -2)
				{
					return "O-O-O";
				}
			}

			MoveGenerator moveGen = new MoveGenerator();
			string moveNotation = GetSymbolFromPieceType(movePieceType);

			// check if any ambiguity exists in notation (e.g if e2 can be reached via Nfe2 and Nbe2)
			if (movePieceType != PieceHelper.Pawn && movePieceType != PieceHelper.King)
			{
				var allMoves = moveGen.GenerateMoves(board);

				foreach (Move altMove in allMoves)
				{

					if (altMove.StartSquareIndex != move.StartSquareIndex && altMove.TargetSquareIndex == move.TargetSquareIndex)
					{ // if moving to same square from different square
						if (PieceHelper.PieceType(board.Square[altMove.StartSquareIndex]) == movePieceType)
						{ // same piece type
							int fromFileIndex = BoardHelper.FileIndex(move.StartSquareIndex);
							int alternateFromFileIndex = BoardHelper.FileIndex(altMove.StartSquareIndex);
							int fromRankIndex = BoardHelper.RankIndex(move.StartSquareIndex);
							int alternateFromRankIndex = BoardHelper.RankIndex(altMove.StartSquareIndex);

							if (fromFileIndex != alternateFromFileIndex)
							{ // pieces on different files, thus ambiguity can be resolved by specifying file
								moveNotation += BoardHelper.fileNames[fromFileIndex];
								break; // ambiguity resolved
							}
							else if (fromRankIndex != alternateFromRankIndex)
							{
								moveNotation += BoardHelper.rankNames[fromRankIndex];
								break; // ambiguity resolved
							}
						}
					}

				}
			}

			if (capturedPieceType != 0)
			{ // add 'x' to indicate capture
				if (movePieceType == PieceHelper.Pawn)
				{
					moveNotation += BoardHelper.fileNames[BoardHelper.FileIndex(move.StartSquareIndex)];
				}
				moveNotation += "x";
			}
			else
			{ // check if capturing ep
				if (move.MoveFlag == Move.EnPassantCaptureFlag)
				{
					moveNotation += BoardHelper.fileNames[BoardHelper.FileIndex(move.StartSquareIndex)] + "x";
				}
			}

			moveNotation += BoardHelper.fileNames[BoardHelper.FileIndex(move.TargetSquareIndex)];
			moveNotation += BoardHelper.rankNames[BoardHelper.RankIndex(move.TargetSquareIndex)];

			// add promotion piece
			if (move.IsPromotion)
			{
				int promotionPieceType = move.PromotionPieceType;
				moveNotation += "=" + GetSymbolFromPieceType(promotionPieceType);
			}

			board.MakeMove(move, inSearch: true);
			var legalResponses = moveGen.GenerateMoves(board);
			// add check/mate symbol if applicable
			if (moveGen.InCheck())
			{
				if (legalResponses.Length == 0)
				{
					moveNotation += "#";
				}
				else
				{
					moveNotation += "+";
				}
			}
			board.UndoMove(move, inSearch: true);

			return moveNotation;

			string GetSymbolFromPieceType(int pieceType)
			{
				switch (pieceType)
				{
					case PieceHelper.Rook:
						return "R";
					case PieceHelper.Knight:
						return "N";
					case PieceHelper.Bishop:
						return "B";
					case PieceHelper.Queen:
						return "Q";
					case PieceHelper.King:
						return "K";
					default:
						return "";
				}
			}
		}

		public static Move GetMoveFromSAN(Board board, string algebraicMove)
		{
			MoveGenerator moveGenerator = new MoveGenerator();

			// Remove unrequired info from move string
			algebraicMove = algebraicMove.Replace("+", "").Replace("#", "").Replace("x", "").Replace("-", "");
			var allMoves = moveGenerator.GenerateMoves(board);

			Move move = new Move();

			foreach (Move moveToTest in allMoves)
			{
				move = moveToTest;

				int moveFromIndex = move.StartSquareIndex;
				int moveToIndex = move.TargetSquareIndex;
				int movePieceType = PieceHelper.PieceType(board.Square[moveFromIndex]);
				Coord fromCoord = BoardHelper.CoordFromIndex(moveFromIndex);
				Coord toCoord = BoardHelper.CoordFromIndex(moveToIndex);
				if (algebraicMove == "OO")
				{ // castle kingside
					if (movePieceType == PieceHelper.King && moveToIndex - moveFromIndex == 2)
					{
						return move;
					}
				}
				else if (algebraicMove == "OOO")
				{ // castle queenside
					if (movePieceType == PieceHelper.King && moveToIndex - moveFromIndex == -2)
					{
						return move;
					}
				}
				// Is pawn move if starts with any file indicator (e.g. 'e'4. Note that uppercase B is used for bishops) 
				else if (BoardHelper.fileNames.Contains(algebraicMove[0].ToString()))
				{
					if (movePieceType != PieceHelper.Pawn)
					{
						continue;
					}
					if (BoardHelper.fileNames.IndexOf(algebraicMove[0]) == fromCoord.fileIndex)
					{ // correct starting file
						if (algebraicMove.Contains("="))
						{ // is promotion
							if (toCoord.rankIndex == 0 || toCoord.rankIndex == 7)
							{

								if (algebraicMove.Length == 5) // pawn is capturing to promote
								{
									char targetFile = algebraicMove[1];
									if (BoardHelper.fileNames.IndexOf(targetFile) != toCoord.fileIndex)
									{
										// Skip if not moving to correct file
										continue;
									}
								}
								char promotionChar = algebraicMove[algebraicMove.Length - 1];

								if (move.PromotionPieceType != GetPieceTypeFromSymbol(promotionChar))
								{
									continue; // skip this move, incorrect promotion type
								}

								return move;
							}
						}
						else
						{

							char targetFile = algebraicMove[algebraicMove.Length - 2];
							char targetRank = algebraicMove[algebraicMove.Length - 1];

							if (BoardHelper.fileNames.IndexOf(targetFile) == toCoord.fileIndex)
							{ // correct ending file
								if (targetRank.ToString() == (toCoord.rankIndex + 1).ToString())
								{ // correct ending rank
									break;
								}
							}
						}
					}
				}
				else
				{ // regular piece move

					char movePieceChar = algebraicMove[0];
					if (GetPieceTypeFromSymbol(movePieceChar) != movePieceType)
					{
						continue; // skip this move, incorrect move piece type
					}

					char targetFile = algebraicMove[algebraicMove.Length - 2];
					char targetRank = algebraicMove[algebraicMove.Length - 1];
					if (BoardHelper.fileNames.IndexOf(targetFile) == toCoord.fileIndex)
					{ // correct ending file
						if (targetRank.ToString() == (toCoord.rankIndex + 1).ToString())
						{ // correct ending rank

							if (algebraicMove.Length == 4)
							{ // addition char present for disambiguation (e.g. Nbd7 or R7e2)
								char disambiguationChar = algebraicMove[1];

								if (BoardHelper.fileNames.Contains(disambiguationChar.ToString()))
								{ // is file disambiguation
									if (BoardHelper.fileNames.IndexOf(disambiguationChar) != fromCoord.fileIndex)
									{ // incorrect starting file
										continue;
									}
								}
								else
								{ // is rank disambiguation
									if (disambiguationChar.ToString() != (fromCoord.rankIndex + 1).ToString())
									{ // incorrect starting rank
										continue;
									}

								}
							}
							break;
						}
					}
				}
			}
			return move;

			int GetPieceTypeFromSymbol(char symbol)
			{
				switch (symbol)
				{
					case 'R':
						return PieceHelper.Rook;
					case 'N':
						return PieceHelper.Knight;
					case 'B':
						return PieceHelper.Bishop;
					case 'Q':
						return PieceHelper.Queen;
					case 'K':
						return PieceHelper.King;
					default:
						return PieceHelper.None;
				}
			}
		}

	}
}