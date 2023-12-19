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

    private const string BotMatchStartFens = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    
    private readonly MyBot _bot = new MyBot();
    private Board _currentBoard;

    private const string LogFile = "./comm-log.txt";
    private const string UciLog = "./uci-log.txt";
    private const string ErrorFile = "./error.log";
    private const string DateFormat = "yyyyMMddTHHmmss";

    private void WriteLineToDisk(string line, string file)
    {
        using StreamWriter outputFile = new StreamWriter(file, true);
        
        outputFile.WriteLine(line);
        
    }
    public bool ReceiveCommand(string message)
    {
        var messageType = message.Split(' ')[0];
        WriteLineToDisk($"{DateTimeOffset.Now.ToString(DateFormat)} -- Received message {message}", LogFile);
        WriteLineToDisk($"{DateTimeOffset.Now.ToString(DateFormat)}{message}", UciLog);
        try
        {
            switch (messageType)
            {
                case UciInit:
                    Respond(UciOkay);
                    return true;
                case IsReady:
                    Respond(ReadyOk);
                    return true;
                case NewGame:
                    _currentBoard = new Board();
                    _currentBoard.LoadPosition(BotMatchStartFens);
                    return true;
                case Position:
                    ProcessPositionCommand(message);
                    return true;
                case Go:
                    ProcessGoCommand(message);
                    return true;
                case Stop:
                    return true;
                case Quit:
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            if (ex.StackTrace != null)
            {
                var errorMessage = $"{DateTimeOffset.Now.ToString(DateFormat)} -- {ex.Message}\n{ex.StackTrace}"; 
                WriteLineToDisk(errorMessage, ErrorFile);
                
            }
        }

        return false;

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
        _currentBoard = new Board();
        _currentBoard.LoadPosition(BotMatchStartFens);
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
        WriteLineToDisk($"Responding: {response}", LogFile);
        Console.WriteLine(response);
    }
}