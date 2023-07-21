using ChessChallenge.Chess;
using System;
using ChessChallenge.API;

namespace ChessChallenge.Application.APIHelpers
{

    public class MoveHelper
    {
        public static (Chess.Move move, PieceType pieceType, PieceType captureType) CreateMoveFromName(string moveNameUCI, API.Board board)
        {
            int indexStart = BoardHelper.SquareIndexFromName(moveNameUCI[0] + "" + moveNameUCI[1]);
            int indexTarget = BoardHelper.SquareIndexFromName(moveNameUCI[2] + "" + moveNameUCI[3]);
            char promoteChar = moveNameUCI.Length > 3 ? moveNameUCI[^1] : ' ';

            PieceType promotePieceType = promoteChar switch
            {
                'q' => PieceType.Queen,
                'r' => PieceType.Rook,
                'n' => PieceType.Knight,
                'b' => PieceType.Bishop,
                _ => PieceType.None
            };

            Square startSquare = new Square(indexStart);
            Square targetSquare = new Square(indexTarget);


            PieceType movedPieceType = board.GetPiece(startSquare).PieceType;
            PieceType capturedPieceType = board.GetPiece(targetSquare).PieceType;

            // Figure out move flag
            int flag = Chess.Move.NoFlag;

            if (movedPieceType == PieceType.Pawn)
            {
                if (targetSquare.Rank is 7 or 0)
                {
                    flag = promotePieceType switch
                    {
                        PieceType.Queen => Chess.Move.PromoteToQueenFlag,
                        PieceType.Rook => Chess.Move.PromoteToRookFlag,
                        PieceType.Knight => Chess.Move.PromoteToKnightFlag,
                        PieceType.Bishop => Chess.Move.PromoteToBishopFlag,
                        _ => 0
                    };
                }
                else
                {
                    if (Math.Abs(targetSquare.Rank - startSquare.Rank) == 2)
                    {
                        flag = Chess.Move.PawnTwoUpFlag;
                    }
                    // En-passant
                    else if (startSquare.File != targetSquare.File && board.GetPiece(targetSquare).IsNull)
                    {
                        flag = Chess.Move.EnPassantCaptureFlag;
                    }
                }
            }
            else if (movedPieceType == PieceType.King)
            {
                if (Math.Abs(startSquare.File - targetSquare.File) > 1)
                {
                    flag = Chess.Move.CastleFlag;
                }
            }

            Chess.Move coreMove = new Chess.Move(startSquare.Index, targetSquare.Index, flag);
            return (coreMove, movedPieceType, capturedPieceType);
        }


    }
}
