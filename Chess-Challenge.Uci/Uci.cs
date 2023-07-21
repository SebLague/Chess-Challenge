using System.Text;
using ChessChallenge.API;
using ChessChallenge.Chess;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;
using Timer = ChessChallenge.API.Timer;

namespace Chess_Challenge.Cli
{
    internal class Uci
    {
        private const string StartposFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private IChessBot _bot;
        private Board _board;

        public Uci()
        {
            Reset();
        }

        private void Reset()
        {
            _bot = new MyBot();
            _board = Board.CreateBoardFromFEN(StartposFen);
        }

        private void HandleUci()
        {
            Console.WriteLine("id name Chess Challenge");
            Console.WriteLine("id author Gediminas Masaitis, Sebastian Lague");
            Console.WriteLine();
            Console.WriteLine("uciok");
        }

        private void HandlePosition(IReadOnlyList<string> words)
        {
            var writingFen = false;
            var writingMoves = false;
            var fenBuilder = new StringBuilder();
            
            for (var wordIndex = 0; wordIndex < words.Count; wordIndex++)
            {
                var word = words[wordIndex];

                if (word == "startpos")
                {
                    _board = Board.CreateBoardFromFEN(StartposFen);
                }

                if (word == "fen")
                {
                    writingFen = true;
                    continue;
                }

                if (word == "moves")
                {
                    if (writingFen)
                    {
                        fenBuilder.Length--;
                        var fen = fenBuilder.ToString();
                        _board = Board.CreateBoardFromFEN(fen);
                    }
                    writingFen = false;
                    writingMoves = true;
                    continue;
                }

                if (writingFen)
                {
                    fenBuilder.Append(word);
                    fenBuilder.Append(' ');
                }

                if (writingMoves)
                {
                    var move = new Move(word, _board);
                    _board.MakeMove(move);
                }
            }

            if (writingFen)
            {
                fenBuilder.Length--;
                var fen = fenBuilder.ToString();
                _board = Board.CreateBoardFromFEN(fen);
            }
        }

        private static string GetMoveName(Move move)
        {
            if (move.IsNull)
            {
                return "Null";
            }

            string startSquareName = BoardHelper.SquareNameFromIndex(move.StartSquare.Index);
            string endSquareName = BoardHelper.SquareNameFromIndex(move.TargetSquare.Index);
            string moveName = startSquareName + endSquareName;
            if (move.IsPromotion)
            {
                switch (move.PromotionPieceType)
                {
                    case PieceType.Rook:
                        moveName += "r";
                        break;
                    case PieceType.Knight:
                        moveName += "n";
                        break;
                    case PieceType.Bishop:
                        moveName += "b";
                        break;
                    case PieceType.Queen:
                        moveName += "q";
                        break;
                }
            }
            return moveName;
        }

        private void HandleGo(IReadOnlyList<string> words)
        {
            var ms = 60000;

            for (var wordIndex = 0; wordIndex < words.Count; wordIndex++)
            {
                var word = words[wordIndex];
                if (words.Count > wordIndex + 1)
                {
                    var nextWord = words[wordIndex + 1];
                    if (word == "wtime" && _board.IsWhiteToMove)
                    {
                        if (int.TryParse(nextWord, out var wtime))
                        {
                            ms = wtime;
                        }
                    }
                    if (word == "btime" && !_board.IsWhiteToMove)
                    {
                        if (int.TryParse(nextWord, out var btime))
                        {
                            ms = btime;
                        }
                    }
                }

                if (word == "infinite")
                {
                    ms = int.MaxValue;
                }
            }

            var timer = new Timer(ms);
            var move = _bot.Think(_board, timer);
            var moveStr = GetMoveName(move);
            Console.WriteLine($"bestmove {moveStr}");
        }

        private void HandleLine(string line)
        {
            var words = line.Split(' ');
            if (words.Length == 0)
            {
                return;
            }

            var firstWord = words[0];
            switch (firstWord)
            {
                case "uci":
                    HandleUci();
                    return;
                case "ucinewgame":
                    Reset();
                    return;
                case "position":
                    HandlePosition(words);
                    return;
                case "isready":
                    Console.WriteLine("readyok");
                    return;
                case "go":
                    HandleGo(words);
                    return;
            }
        }

        public void Run()
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "quit" || line == "exit")
                {
                    return;
                }

                HandleLine(line);
            }
        }
    }
}
