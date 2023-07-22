using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class MyBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            List<Move> allMoves = board.GetLegalMoves().ToList();

            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Count)];

            //Priortize Checkmates
            Move[] checkmates = allMoves.Where(m => MoveIsCheckmate(board, m)).ToArray();
            if (checkmates.Length > 0)
            {
                return checkmates[0];
            }

            //Upgrade pieces to best piece only
            Move[] promotions = allMoves.Where(m => m.IsPromotion).OrderByDescending(m => pieceValues[(int)m.PromotionPieceType]).ToArray();
            if (promotions.Length > 0)
            {
                return promotions[0];
            }

            //Prioritize Checks
            Move[] checks = allMoves.Where(m => MoveIsCheck(board, m) && !MoveCreatesTarget(board, m)).ToArray();
            if (checks.Length > 0)
            {
                return checks[rng.Next(checks.Length)];
            }

            IOrderedEnumerable<Move> rankedMoves = allMoves
                        .Where(m => !checkmates.Contains(m) && !promotions.Contains(m) && !checks.Contains(m) && CalculateMoveValue(board, m) > 0)
                        .OrderByDescending(m => CalculateMoveValue(board, m));

            if (rankedMoves.Count() > 0)
            {
                moveToPlay = rankedMoves.First();
            }
            else
            {
                //move pawn up you ass
                Move[] pawnMoves = allMoves.Where(m => m.MovePieceType == PieceType.Pawn && !MoveCreatesTarget(board, m)).ToArray();
                if(pawnMoves.Length > 0)
                    moveToPlay= pawnMoves[rng.Next(pawnMoves.Length)];
            }
            
            return  moveToPlay;
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }

        bool MoveIsCheck(Board board,Move move)
        {
            board.MakeMove(move);
            bool isCheck = board.IsInCheck();
            board.UndoMove(move);
            return isCheck;
        }

        bool MoveCreatesTarget(Board board, Move move)
        {
            board.MakeMove(move);
            Move[] enemyMoves = board.GetLegalMoves();
            bool isTarget = enemyMoves.Any(m => m.TargetSquare == move.TargetSquare);
            board.UndoMove(move);

            return isTarget;
        }


        int CalculateMoveValue(Board board, Move move)
        {
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

            int cost = !MoveCreatesTarget(board, move) ? 0 : pieceValues[(int)move.MovePieceType];

            return capturedPieceValue - cost;
        }
    }
}