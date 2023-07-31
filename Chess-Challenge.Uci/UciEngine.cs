using ChessChallenge.API;
using ChessChallenge.Chess;
using Board = ChessChallenge.Chess.Board;
using Move = ChessChallenge.Chess.Move;
using Timer = ChessChallenge.API.Timer;

namespace ChessChallenge.Bot;

public class UciEngine
{
    private const string UciInit = "uci";
    private const string UciOkay = "uciok";
    private const string IsReady = "isready";
    private const string ReadyOk = "readyok";
    private const string NewGame = "ucinewgame";
    private const string Position = "position";
    private const string BestMove = "bestmove";
    private const string Go = "go";
    private const string Stop = "stop";
    private const string Quit = "quit";

    private const string botMatchStartFens = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";


    private MyBot _bot = new MyBot();
    private Board _currentBoard;

    private string _logFile = "./comm-log.txt";
    private string _uciLog = "uci-log.txt";
    private string _errorFile = "error.log";
    private const string _dateFormat = "yyyyMMddTHHmmss";


    private char GetPromotionCharacter(PieceType piece) =>
        piece switch
        {
            PieceType.None => 'q',
            PieceType.Pawn => 'q',
            PieceType.Knight => 'n',
            PieceType.Bishop => 'b',
            PieceType.Rook => 'r',
            PieceType.Queen => 'q',
            PieceType.King => 'q',
            _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
        };

    private void WriteLineToDisk(string line, string file)
    {
        using StreamWriter outputFile = new StreamWriter(file, true);
        
        outputFile.WriteLine(line);
        
    }
    public void ReceiveCommand(string message)
    {
        var messageType = message.Split(' ')[0];
        WriteLineToDisk($"{DateTimeOffset.Now.ToString(_dateFormat)} -- Received message {message}", _logFile);
        WriteLineToDisk($"{DateTimeOffset.Now.ToString(_dateFormat)}{message}", _uciLog);
        try
        {
            switch (messageType)
            {
                case UciInit:
                    Respond(UciOkay);
                    break;
                case IsReady:
                    Respond(ReadyOk);
                    break;
                case NewGame:
                    _uciLog = $"./{_uciLog}";
                    _currentBoard = new Board();
                    _currentBoard.LoadPosition(botMatchStartFens);
                    break;
                case Position:
                    ProcessPositionCommand(message);
                    break;
                case Go:
                    ProcessGoCommand(message);
                    break;
                case Stop:
                    // message = Quit;
                    // ProcessStopCommand();
                    break;
                case Quit:
                    break;
                default:
                    message = Quit;
                    break;
            }
        }
        catch (Exception ex)
        {
            if (ex.StackTrace != null)
            {
                var errorMessage = $"{DateTimeOffset.Now.ToString(_dateFormat)} -- {ex.Message}\n{ex.StackTrace}"; 
                WriteLineToDisk(errorMessage, _errorFile);
                
            }
        }
        
    }

    private void ProcessStopCommand()
    {
        throw new NotImplementedException();
    }

    private void ProcessGoCommand(string message)
    {
        var split = message.Split(' ');
        var millis = int.Parse(split[2]);
        var newMove = new Move(_bot.Think(new(_currentBoard), new (millis)).RawValue);
        var moveNameUci = MoveUtility.GetMoveNameUCI(newMove);
        Respond($"{BestMove} {moveNameUci}");
    }

    private void ProcessPositionCommand(string message)
    { 
        // if (message.Split(' ').Length < 3) return;
        // var moveStrings = message.Split(' ').Skip(3).ToArray();
        // var moves = new Move[moveStrings.Length];
        // for (var i = 0; i <  moveStrings.Length; i++)
        // {
        //     var str = moveStrings[i];
        //     moves[i] = MoveUtility.GetMoveFromUCIName(str, _currentBoard);
        //     _currentBoard.MakeMove(moves[i], false);
        // }
        _currentBoard = new Board();
        _currentBoard.LoadPosition(botMatchStartFens);
        var moveStrings = message.Split(' ');
        if (moveStrings[^1] == "startpos") return;
        for (var i = 3; i < moveStrings.Length; i++)
        {
            var newMove = MoveUtility.GetMoveFromUCIName(moveStrings[i], _currentBoard);
            _currentBoard.MakeMove(newMove, false);
        }
    }

    private void Respond(string response)
    {
        WriteLineToDisk($"Responding: {response}", _logFile);
        Console.WriteLine(response);
    }
}