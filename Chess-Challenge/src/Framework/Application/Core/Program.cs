using System;
using ChessChallenge.API;

namespace ChessChallenge.Application {
    static class Program {
        public static void Main() {
            String initial_pos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            MyBot bot = new MyBot();
            Chess.Board fen_board = new Chess.Board();
            fen_board.LoadPosition(initial_pos);
            Board bot_board = new Board(fen_board);
            
            Timer timer = new Timer(1000);
            bot.Think(bot_board, timer);
            
            String input_line = Console.ReadLine();
            while (input_line != "") {
                if (input_line == "uci") {
                    Console.WriteLine("uciok");
                } else if (input_line == "quit") {
                    break;
                }

                if (input_line == "position") {
                    String[] parts = input_line.Split(" ");
                    if (parts[1] == "startpos") {
                        // I'm not sure why he chose to have two board, but he does so you load fen
                        // into fen_board 
                        // and then feed that into bot_board
                        fen_board.LoadPosition(initial_pos);
                        bot_board = new Board(fen_board);
                    } else {
                        int start_idx = 0; // this is where the fen string starts, its wrong atm
                        int end_idx = input_line.Length - 1;
                        if (input_line.Contains("moves")) {
                            end_idx = input_line.IndexOf("moves");
                        }
                        fen_board.LoadPosition(input_line.);
                    }
                }

                if (input_line == "go") {
                    bot.Think(bot_board, timer);
                }

                input_line = Console.ReadLine();
            }
        }
    }
}