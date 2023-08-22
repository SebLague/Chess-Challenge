using ChessChallenge.API;
using System.Collections;
using System;
using System.Linq;


public class MyBot : IChessBot
{   
 
    //null, pawn, knight, bishop, rook, queen, king
<<<<<<< Updated upstream
    public Move Think(Board board, Timer timer){   
        
        bool color = board.IsWhiteToMove;
=======
    public Move Think(Board board, Timer timer){
        bool me_white;
        bool they_white;
        //gets if it is white's turn to move
        if(board.IsWhiteToMove){
            me_white = true;
            they_white = false;
        }else{
            they_white = true;
            me_white = false;
        }
>>>>>>> Stashed changes
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
         

<<<<<<< Updated upstream
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
=======

            int before = countup(values, they_white);
            int after;
            bool ischeck;
            bool ismate;
        
            board.MakeMove(move);
            after = countup(values, they_white);
            ischeck = board.IsInCheck();
            ismate = board.IsInCheckmate();
            board.UndoMove(move);
        
            

            //the weight modifier from captures
            int capture_weight = (before-after);
            //getting the weight modifier from checks
            int check_weight = ((Convert.ToInt32(ischeck)+Convert.ToInt32(ismate))*100);
            return(check_weight+capture_weight);
            
        }

        int GetHighestScore(Board board){
            Move[] moves = board.GetLegalMoves();
            int[] scores = new int[moves.Length];
            for(int i=0; i<moves.Length;i++){
                scores[i] = move_weight(board, values, moves, they_white, moves[i], true);
                Console.WriteLine("Score for move "+ i+  ": "+scores[i]);

            }

            int index_of_highest = Array.IndexOf(scores, scores.Max());
            return index_of_highest;
        }

        Move search_for_moves(Board board, bool they_white, int depth){
            
            Move[] moves = board.GetLegalMoves();
            int name = 1;
            int [] weights = new int[moves.Length];
            Console.WriteLine(moves.Length);
            for(int i=0; i< moves.Length; i++){
                Console.WriteLine("Lookinin move"+ i +" of"+ weights.Length);
                Move[] done_moves =new Move[depth];
                //for(int j=0; j<depth; j++){
                    
                //}

                weights[i] = move_weight(board, values, done_moves, they_white, done_moves[0], false);
                Console.WriteLine("move weight = " + weights[i]);
            }
            int highest_score = weights.Max();
            
            Console.WriteLine("highest score from this moveset: "+highest_score);
            int highest_index = Array.IndexOf(weights, highest_score);

            if(weights[highest_index] < 1){
                Console.WriteLine("using random");
                Random random = new Random();
                int random_move = random.Next(0, moves.Length);
                return moves[random_move];
            }
            else{
                Console.WriteLine("Using regularol");
                return moves[highest_index];
>>>>>>> Stashed changes
            }
            int highest_score = scores.Max();
            int highest_index = Array.IndexOf(scores, highest_score);
            return(highest_index);
        }
<<<<<<< Updated upstream
       
        int index = GetHighestScore(board, moves, scores);
        
        if(scores[index] == 0){
            Random random = new Random();
            int random_move = random.Next(0, moves.Length);

            return moves[random_move];
        }else{
         
            return moves[index];
=======
        return(search_for_moves(board, they_white, 3));
>>>>>>> Stashed changes
        }

    }
}
