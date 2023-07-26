using ChessChallenge.API;
using System.Collections;
using System;



public class MyBot : IChessBot
{   
 
    //null, pawn, knight, bishop, rook, queen, king
    public Move Think(Board board, Timer timer){   
        int score = 0;
        int[] values = {0, 30, 50, 70, 60, 90, 100};
        bool color = board.IsWhiteToMove;
        
        static int GetScore(Board board, int score, int[] values, bool color){
            if(color == true){
                var opponant_pieces = board.BlackPiecesBitboard;
                Console.WriteLine("black pieces: "+ opponant_pieces);
            }else{
                var opponant_pieces = board.WhitePiecesBitboard;
                Console.WriteLine("White peices: "+ opponant_pieces);
            }
            //get bitboard of the whole board, when there is a piece there use getpeice to get infro about that pieve
            return 0;
        }
        GetScore(board, score, values, color);
         
        System.Random rand = new System.Random();
        Move[] moves = board.GetLegalMoves();
        
        int N = rand.Next(moves.Length);
        return moves[N];

    }
}
