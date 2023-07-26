using ChessChallenge.API;
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace ChessChallenge.Example;

public class DiamoundzBot : IChessBot
{
    //GLOBAL variables
    ushort searchDepth = 3;
    ushort turnIndex = 0;

    // a dictionary where the key value pairs are  <FEN,move(Eq."a2a4")>
    string[] openings = new string[] {
        //BLACK OPENINGS
        "d7d5",
        //WHITE OPENINGS
        "e2e4"
    };

    string[] centerSquares = new string[] {
        "c6","d6","e6","f6",
        "c5","d5","e5","f5",
        "c4","d4","e4","f4",
        "c3","d3","e3","f3"
    };
    int[] preferedWhitePawnRows = new int[] {
        5,6,7
    };
    int[] preferedBlackPawnRows = new int[] {
        0,1,2
    };


    // Piece values: null, pawn, knight, bishop, rook, queen, king
    ushort[] pieceValues = { 100, 300, 300, 450, 950 };

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        Move chosenMove = GetMove(board, turnIndex);
        turnIndex++;

        return chosenMove;
    }

    private short GetBoardValue(Board board)
    {
        // Add up all white pieces value
        // Substract all black pieces value
        // return value if we are white, else return the opposite number

        short value = 0;

        PieceList[] pieceLists = board.GetAllPieceLists();

        for (ushort i = 0; i < pieceLists.Length - 2; i++)
        {
            if (i < 5)
            {
                value += (short)(pieceValues[i % 5] * pieceLists[i].Count);
            }
            else
            {
                value -= (short)(pieceValues[i % 5] * pieceLists[i + 1].Count);

            }
        }

        if (board.IsWhiteToMove)
            return value;
        else
        {
            return (short)(-value);
        }

    }
    private short GetDirectMoveValue(Board board, Move move)
    {
        short value = 0;

        board.MakeMove(move);
        //  if the move is a check mate we choose this one
        if (board.IsInCheckmate())
        {
            value += 25000;
        }
        //check yields a higher value then pawn moves
        if (board.IsInCheck())
        {
            value += 28;
        }
        if (!board.IsInCheck() && board.GetLegalMoves().Length == 0)
        {
            value -= 25000;
        }

        board.UndoMove(move);

        Square targetSquare = move.TargetSquare;

        switch (board.GetPiece(move.StartSquare).PieceType)
        {
            case PieceType.Pawn:
                value += 11;
                if (board.IsWhiteToMove)
                {
                    if (preferedWhitePawnRows.Contains(targetSquare.Rank))
                    {
                        value += 16;
                    }
                }
                else
                {
                    if (preferedBlackPawnRows.Contains(targetSquare.Rank))
                    {
                        value += 16;
                    }
                }
                break;
            case PieceType.Knight:
                if (centerSquares.Contains(targetSquare.Name))
                {
                    value += 9;
                }
                break;
            case PieceType.Bishop:
                if (centerSquares.Contains(targetSquare.Name))
                {
                    value += 4;
                }
                break;
            case PieceType.Rook:
                if (centerSquares.Contains(targetSquare.Name))
                {
                    value += 2;
                }
                break;
            case PieceType.Queen:
                if (centerSquares.Contains(targetSquare.Name))
                {
                    value += 2;
                }
                break;
            case PieceType.King:
                value -= 22;
                if (move.IsCastles)
                {
                    value += 88;
                }
                break;
            default:
                break;
        }
        // we like capture trades
        if (move.IsCapture)
        {
            if (GetBoardValue(board) >= 0)
            {
                value += 31;
            }
            else
            {
                value -= 20;
            }
        }

        return value;
    }

    private Move GetMove(Board board, int turn)
    {
        List<Move> bestMoves = new List<Move>();
        //We get the best moves through minmax algorithm
        GetMoveEval(board, searchDepth, -99999, 99999, true, bestMoves);

        foreach (string opening in openings)
        {
            if (bestMoves.Contains(new Move(opening, board)))
            {
                return new Move(opening, board);
            }
        }

        Move bestMove = bestMoves[0];
        short bestMoveValue = GetDirectMoveValue(board, bestMove);
        foreach (Move move in bestMoves)
        {
            short value = GetDirectMoveValue(board, move);
            if (value > bestMoveValue)
            {
                bestMove = move;
                bestMoveValue = value;
            }
        }


        return bestMove;
    }

    private short GetMoveEval(Board board, int depth, int alpha, int beta, bool isMaximizingPlayer, List<Move> bestMoves)
    {
        //NOTE : needs working implementation of the pruning algorithm
        //Chess version of the MINMAX algorithm

        Move[] moves = board.GetLegalMoves();

        if (isMaximizingPlayer)
        {
            short maxEval = -30000;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                short eval = 0;
                eval = GetMoveEval(board, depth - 1, alpha, beta, false, bestMoves);

                if (eval > maxEval)
                {
                    maxEval = eval;
                    if (depth == searchDepth)
                    {
                        bestMoves.Clear();
                        bestMoves.Add(move);
                    }
                }
                else if (depth == searchDepth && eval == maxEval)
                {
                    bestMoves.Add(move);
                }

                board.UndoMove(move);
                // Attempt to alpha beta pruning (NOT WORKING YET)
                /*
                if (eval > alpha)
                {
                    alpha = eval;
                }
                if (beta <= alpha)
                {
                    break;
                }
                */
            }
            return maxEval;
        }
        else
        {
            short minEval = 30000;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                short eval = 0;
                if (depth > 0)
                {
                    eval = GetMoveEval(board, depth - 1, alpha, beta, true, bestMoves);
                }
                else
                {
                    eval = GetBoardValue(board);

                }
                if (eval < minEval)
                {
                    minEval = eval;
                }
                board.UndoMove(move);
                /*
                if (eval < beta)
                {
                    beta = eval;
                }
                if (beta <= alpha)
                {
                    break;
                }
                */
            }
            return minEval;
        }
    }


}