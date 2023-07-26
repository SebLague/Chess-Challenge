using ChessChallenge.API;
using System.Collections;
using System;
using System.Linq;


public class MyBot : IChessBot
{   
 
    //null, pawn, knight, bishop, rook, queen, king
    public Move Think(Board board, Timer timer){   
        
        bool color = board.IsWhiteToMove;
        Move[] moves = board.GetLegalMoves();
        int[] scores = new int[moves.Length];
        int[] values = {0, 30, 50, 70, 60, 90, 100};

        int countup(Board board, int[] values){
            int count = 0;
            PieceList[] pieces = board.GetAllPieceLists();
            for(int i=0; i<5; i++){
                count +=(values[i]*pieces[i].Count);
            }
            return(count);
            
        }
        static int pieceweight(Piece piece){
         

            if(piece.IsPawn){
                return(30);
            }else if(piece.IsKnight){
                return(50);
            }else if(piece.IsRook){
                return(60);
            }else if(piece.IsBishop){
                return(70);
            }else if(piece.IsQueen){
                return(90);
            }else if(piece.IsKing){
                return(100);
            }else{
                return(0);
            }
        }
        static int ischeck_or_mate(Board board, Move move){
            board.MakeMove(move);
            bool ischeck = board.IsInCheck();
            bool ismate = board.IsInCheckmate();
            board.UndoMove(move);
            if(ischeck){
                return(1);
            }else if(ismate){
                return(2);
            }else{
                return(0);
            }

        }
        Console.WriteLine(countup(board, values));
        
        int GetHighestScore(Board board, Move[] moves, int[] scores){
           
            for(int i = 0; i++ < moves.Length-1;){
         
                bool capture = moves[i].IsCapture;
                if(capture == true){
                    
                    var capture_type = moves[i].TargetSquare;
                    Piece target = board.GetPiece(capture_type);
                    scores[i] += pieceweight(target);
                }
                int check_mate = ischeck_or_mate(board, moves[i]);
                if(check_mate == 1){
                    scores[i] += 250;
                }else if(check_mate == 2){
                    scores[i] += 1000000000;
                }
            }
            int highest_score = scores.Max();
            int highest_index = Array.IndexOf(scores, highest_score);
            return(highest_index);
        }
       
        int index = GetHighestScore(board, moves, scores);
        
        if(scores[index] == 0){
            Random random = new Random();
            int random_move = random.Next(0, moves.Length);

            return moves[random_move];
        }else{
         
            return moves[index];
        }

    }
}
