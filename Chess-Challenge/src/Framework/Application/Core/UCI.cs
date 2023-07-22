namespace ChessChallenge.Application
{
    using System;
    using System.Collections.Generic;
    using ChessChallenge.API;

    public class UCI
    {
        private const double TIME_USED = 0.02;
        private const double INCREMENT_USED = 0.5;

        private Board board;
        private MyBot myBot;

        public static void Main()
        {
            UCI uci = new UCI();

            while (true)
            {
                string? line = Console.ReadLine();

                if (line == null)
                {
                    continue;
                }

                uci.ProcessCommand(line);
            }
        }

        public void ProcessCommand(string command)
        {
            var commands = command.Split();
            var mainCommand = commands[0];
            switch (mainCommand)
            {
                case "uci":
                    SendIdAndOptions();
                    break;
                case "isready":
                    Console.WriteLine("readyok");
                    break;
                case "position":
                    SetPosition(commands);
                    break;
                case "go":
                    Go(commands);
                    break;
                case "ucinewgame":
                    UciNewGame();
                    break;
                case "exit":
                    System.Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Unknown command!");
                    break;
            }
        }

        private void SendIdAndOptions()
        {
            Console.WriteLine("id name MyChessEngine");
            Console.WriteLine("id author MyName");
            // Set up any other initialization UCI options here.
            Console.WriteLine("uciok");
        }

        private void SetPosition(string[] commands)
        {
            if (commands[1] == "startpos")
            {
                board = Board.CreateBoardFromFEN(Board.DefaultFEN);
            }
            else if (commands[1] == "fen")
            {
                board = Board.CreateBoardFromFEN(commands[2]);
            }
            if (commands.Length > 3 && commands[3] == "moves")
            {
                for (int i = 4; i < commands.Length; i++)
                {
                    var move = new Move(commands[i], board);
                    board.MakeMove(move);
                }
            }
        }

        private void Go(string[] args)
        {
            int wTime = 0;
            int bTime = 0;
            int wInc = 0;
            int bInc = 0;

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "wtime":
                        i++;
                        wTime = int.Parse(args[i]);
                        break;
                    case "btime":
                        i++;
                        bTime = int.Parse(args[i]);
                        break;
                    case "winc":
                        i++;
                        wInc = int.Parse(args[i]);
                        break;
                    case "binc":
                        i++;
                        bInc = int.Parse(args[i]);
                        break;
                    default:
                        continue;
                }
            }

            int time;
            int inc;

            if (board.IsWhiteToMove)
            {
                time = wTime;
                inc = wInc;
            }
            else
            {
                time = bTime;
                inc = bInc;
            }

            int totalTime = (int)(time * TIME_USED + inc * INCREMENT_USED);

            myBot = new MyBot();
            Move bestmove = myBot.Think(board, new Timer(totalTime));
            Console.WriteLine($"bestmove {Chess.MoveUtility.GetMoveNameUCI(bestmove.move)}");
        }

        private void UciNewGame()
        {
            string[] args = { "", "startpos" };
            SetPosition(args);
        }
    }
}