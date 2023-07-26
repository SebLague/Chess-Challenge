using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    // https://seblague.github.io/chess-coding-challenge/documentation/
    // Documentation

    // Piece values: pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 100, 300, 320, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        Move moveToPlay = new Move();
        int bestEvaluation = -999999;

        foreach (Move move in allMoves)
        {
            board.MakeMove(move);
            int evaluation = -search(board, 3); //hardcode 3 (2 per side plus one from previous line). this function is now recursive
            if (evaluation > bestEvaluation)
            {
                bestEvaluation = evaluation;
                moveToPlay = move;
            }
            board.UndoMove(move);
        }

        return moveToPlay;
    }

    int search(Board board, int depth)
    {
        // reached the end of recursive loop, evaluate the position now
        if (depth == 0)
        {
            return evalute(board);
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

        int bestEvaluation = -9999999;

        // recursive loop from video
        foreach (Move move in allMoves)
        {
            board.MakeMove(move);
            int evaluation = -search(board, depth - 1); //negative because we're now evaluating for opponent and what's good for them is bad for us
            bestEvaluation = Math.Max(bestEvaluation, evaluation);
            board.UndoMove(move);
        }
        return bestEvaluation;
    }

    // Main method for evaluating the "score" of a position. Currently just material evaluation
    int evalute(Board board)
    {
        PieceList[] arrayOfPieceLists = board.GetAllPieceLists();
        int materialEvaluation = getTotalPieceValue(arrayOfPieceLists);
        int perspective = (board.IsWhiteToMove) ? 1 : -1;
        // positive good for current player moving/being evaluted

        return materialEvaluation * perspective;
    }


    // Uses pieceValue array and arrayOfPieceLists to calculate total piece value for a specific player. There's probably an efficient way to slim this down but that's for later
    int getTotalPieceValue(PieceList[] arrayOfPieceLists)
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
}