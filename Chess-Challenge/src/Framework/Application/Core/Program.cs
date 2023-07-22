using System;
using ChessChallenge.API;
using ChessChallenge.Chess;
using Move = ChessChallenge.API.Move;
using Board = ChessChallenge.API.Board;

namespace ChessChallenge.Application {
    static class Program {
        public static string moveToUci(Move move) {
            string moveString = BoardHelper.SquareNameFromIndex(move.StartSquare.Index);
            moveString += BoardHelper.SquareNameFromIndex(move.TargetSquare.Index);
            if(move.IsPromotion) {
                PieceType pt = move.PromotionPieceType;
                if(pt == PieceType.Knight)
                    moveString += 'n';
                else if(pt == PieceType.Bishop)
                    moveString += 'b';
                else if(pt == PieceType.Rook)
                    moveString += 'r';
                else if(pt == PieceType.Queen)
                    moveString += 'q';
            }
            return moveString;
        }
        public static void Main() {
            String initial_pos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            MyBot bot = new MyBot();
            Chess.Board fen_board = new Chess.Board();
            fen_board.LoadPosition(initial_pos);
            Board bot_board = new Board(fen_board);
            Timer timer = new Timer(1000);
            
            String input_line = Console.ReadLine();
            while (input_line != "") {
                if (input_line == "uci") {
                    Console.WriteLine("uciok");
                } 
                else if(input_line == "isready") {
                    Console.WriteLine("readyok");
                }
                else if (input_line == "quit") {
                    break;
                }

                String[] parts = input_line.Split(" ");
                if (parts[0] == "position") {
                    if (parts[1] == "startpos") {
                        // I'm not sure why he chose to have two board, but he does so you load fen
                        // into fen_board 
                        // and then feed that into bot_board
                        fen_board.LoadPosition(initial_pos);
                        bot_board = new Board(fen_board);
                        //Console.WriteLine(bot_board.GetFenString());
                    } else {
                        int start_idx = input_line.IndexOf("fen") + 3; // this is where the fen string starts
                        while(input_line[start_idx] == ' ')
                            start_idx++;
                        int end_idx = input_line.Length - 1;
                        if (input_line.Contains("moves")) {
                            end_idx = input_line.IndexOf("moves");
                        }
                        fen_board.LoadPosition(input_line.Substring(start_idx, end_idx - start_idx));
                        bot_board = new Board(fen_board);
                        //Console.WriteLine(bot_board.GetFenString());
                    }
                    if (input_line.Contains("moves")) {
                        int idx = input_line.IndexOf("moves") + 5;
                        while(input_line[idx] == ' ') idx++;
                        String movesString = input_line.Substring(idx);
                        String[] moves = movesString.Split(" ");
                        for(int i = 0; i < moves.Length; i++) {
                            Move move = new Move(moves[i], bot_board);
                            bot_board.MakeMove(move);
                        }
                        //Console.WriteLine(bot_board.GetFenString());
                    }
                }

                if (parts[0] == "go") {
                    int tm = 100000000;
                    if(parts.Length >= 2) {
                        if(parts[1] == "infinite") tm = 100000000;
                        else if(parts[1] == "wtime" && bot_board.IsWhiteToMove) tm = Convert.ToInt32(parts[2]);
                        else if(parts[3] == "btime" && !bot_board.IsWhiteToMove) tm = Convert.ToInt32(parts[4]);
                    }
                    timer = new Timer(tm);
                    Move bestMove = bot.Think(bot_board, timer);
                    Console.Write("bestmove ");
                    Console.WriteLine(moveToUci(bestMove));
                }

                input_line = Console.ReadLine();
                //Console.WriteLine(input_line);
            }
        }
    }
}