using ChessChallenge.API;
using System;


public class MyBot : IChessBot
{
    int[] pieceValues = {0, 100, 300, 300, 500, 900, 9999};
    bool white = false;
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        int[] values = new int[moves.Length];
        white = board.IsWhiteToMove;

        for(int i = 0; i < moves.GetLength(0); i++)
        {
            Move m = moves[i];
            int value = 0;
            //Don't move king
            if((int)m.MovePieceType == 6)
            {
                value -= 3;
                if(board.HasKingsideCastleRight(white) || board.HasQueensideCastleRight(white)) value -= 7;
            }
            board.MakeMove(m);//    THIS IS WHERE THE MOVE HAPPENS!!!!!!!!!!!!!!!!!!!!!!!
            //Move forward
            value += (white ? 1 : -1) * (m.TargetSquare.Rank - m.StartSquare.Rank);
            //Take checkmates
            if(board.IsInCheckmate()) return m;
            //Take checks
            if(board.IsInCheck()) value += 90;
            //Promote pawns
            if(m.IsPromotion) value += pieceValues[(int)m.PromotionPieceType] - 100;
            //Castle
            if(m.IsCastles) value += 5;
            //Avoid Draw when winning
            if(board.IsDraw()) value -= (white ? 1 : -1) * evalBoard(board)/3;
            
            //minimize reactions
            Move[] movesBack = board.GetLegalMoves(false);
            value -= movesBack.Length;
            if(movesBack.Length > 0)
            {
                //Don't trade unfavorably
                int highestCaptured = 0;
                int movesBackBack = 0;
                for(int j = 0; j < movesBack.Length; j++)
                {
                    Move b = movesBack[j];
                    board.MakeMove(b);
                    movesBackBack += board.GetLegalMoves().Length;
                    //Avoid Draw when winning
                    if(board.IsDraw()) value -= (white ? 1 : -1) * evalBoard(board)/4;
                    //don't lose
                    if(board.IsInCheckmate()) value -= 10000;
                    bool check = false;
                    if(board.IsInCheck())
                    {
                        check = true;
                        value -= 200;
                    }
                    board.UndoMove(b);
                    if(board.SquareIsAttackedByOpponent(b.TargetSquare) && check)
                    {
                        value += 190;
                    }
                    highestCaptured = (int)Math.Max(highestCaptured, pieceValues[(int)b.CapturePieceType] * (board.SquareIsAttackedByOpponent(b.TargetSquare) ? 0.5 : 1));
                }
                //maximise options
                movesBackBack/=movesBack.Length;
                value += (int)(Math.Pow(movesBackBack, 2)/50);
                value += pieceValues[(int)m.CapturePieceType] - highestCaptured;
            }

            board.UndoMove(m);
            values[i] = value;
        }

        Move best = moves[0];
        int bestvalue = -999999;
        for(int i = 0; i < moves.Length; i++) if(values[i] > bestvalue)
        {
            bestvalue = values[i];
            best = moves[i];
        }
        if(false)
        {
            foreach(Move m in moves) Console.Write(m.StartSquare.Name + m.TargetSquare.Name + "\t");
            Console.WriteLine();
            foreach(int i in values) Console.Write(i + "\t");
            Console.WriteLine();
        }
        return best;
    }

    int evalBoard(Board board)
    {
        int score = 0;
        PieceList[] allPieces = board.GetAllPieceLists();
        foreach(PieceList pl in allPieces)
        {
            foreach(Piece p in pl) score += (p.IsWhite ? 1 : -1) * pieceValues[(int)p.PieceType];
        }
        return score;
    }
        
}

