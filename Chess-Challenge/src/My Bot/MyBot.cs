using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    // https://seblague.github.io/chess-coding-challenge/documentation/
    // Documentation

    // Piece values: pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 100, 300, 320, 500, 900, 10000 };
    int[,] pawnBlackPositionValues = 
        { 
            { 100, 100, 103, 105, 105, 103, 100, 100 },
            { 20,20,23,25,25,23,20,20 },
            { 13,13,16,18,18,16,13,13 },
            { 12,12,15,17,17,15,12,12 },
            { 11,11,14,16,16,14,11,11 },
            { 10,10,13,15,15,13,10,10 },
            { 0,0,0,0,0,0,0,0 },
            { 0,0,0,0,0,0,0,0 },
        };
    public MyBot() {
        //Random random = new Random();
    }
    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        Move moveToPlay = new Move();
        int bestEvaluation = -999999;

        foreach (Move move in allMoves)
        {
            board.MakeMove(move);
            int evaluation = -Search(board, 3, -99999999, 99999999); //hardcode 3 (2 per side plus one from previous line). this function is now recursive
            if (evaluation > bestEvaluation)
            {
                bestEvaluation = evaluation;
                moveToPlay = move;
            }
            board.UndoMove(move);
        }

        return moveToPlay;
    }

    int Search(Board board, int depth, int alpha, int beta)
    {
        // Alpha is the best choice for you we have found so far at any point along the path
        // Beta is the best choice for the opponent we have found so far at any point along the path
        if (depth == 0)
        {
            // reached the end of recursive loop, evaluate the position now
            return Evalute(board);
        }

        Move[] allMoves = board.GetLegalMoves();

        if (board.IsInCheckmate())
        {
            return -99999; // Current Player is in checkmate. That's bad
        }

        if (board.IsDraw())
        {
            return 0; // game over, draw
        }

        // int bestEvaluation = -9999999;

        // recursive loop from video
        foreach (Move move in allMoves)
        {
            board.MakeMove(move);
            int evaluation = -Search(board, depth - 1, -beta, -alpha); //negative because we're now evaluating for opponent and what's good for them is bad for us, swapping beta/alpha 
            board.UndoMove(move);
            //bestEvaluation = Math.Max(bestEvaluation, evaluation);
            if (evaluation >= beta)
            {
                // Move was too good, opponent will avoid this position
                return beta; // snip this tree
            }
            alpha = Math.Max(alpha, evaluation);
        }
        return alpha;
    }

    // Main method for evaluating the "score" of a position. Currently just material evaluation
    int Evalute(Board board)
    {
        PieceList[] arrayOfPieceLists = board.GetAllPieceLists();
        int materialEvaluation = GetAllPiecesMaterialValue(arrayOfPieceLists);
        int positionEvaluation = GetAllPiecesPositionValue(arrayOfPieceLists);
        int perspective = (board.IsWhiteToMove) ? 1 : -1;
        // positive good for current player moving/being evaluted

        return (materialEvaluation + positionEvaluation) * perspective;
    }


    // Uses pieceValue array and arrayOfPieceLists to calculate total piece value for a specific player. There's probably an efficient way to slim this down but that's for later
    int GetAllPiecesMaterialValue(PieceList[] arrayOfPieceLists)
    {
        int index = 0;
        int total = 0;
        int flip = 1;
        // Loop to get total piece value count
        // Pawns(white), Knights (white), Bishops (white), Rooks (white), Queens (white), King (white), Pawns (black), Knights (black), Bishops (black), Rooks (black), Queens (black), King (black).
        foreach (PieceList listOfPiece in arrayOfPieceLists)
        {
            total += listOfPiece.Count * pieceValues[index % 6] * flip;
            if (index == 5)
            {
                flip = -1; // After pieces 0-5, we're using black PieceList
            }
            index++;
        }
        return total;
    }

    int GetAllPiecesPositionValue(PieceList[] pieceLists)
    {
        int total = 0;
        foreach (Piece piece in pieceLists[0]) // Add points for my good positions
        {
            total += pawnBlackPositionValues[7 - piece.Square.File, piece.Square.Rank]; // Row, Column (currently assumes position values are symmetrical because it simplifies our calculation)
        }
        foreach (Piece piece in pieceLists[6]) // Subtract points for opponent's good positions
        {
            total += pawnBlackPositionValues[piece.Square.File, piece.Square.Rank] * -1; // Row, Column (currently assumes position values are symmetrical because it simplifies our calculation)
        }
        return total;
    }


}