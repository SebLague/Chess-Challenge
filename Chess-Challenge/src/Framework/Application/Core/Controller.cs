using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using ChessChallenge.API;
using ChessChallenge.Chess;
using Board = ChessChallenge.Chess.Board;
using Move = ChessChallenge.Chess.Move;
using static ChessChallenge.Application.Settings;

namespace ChessChallenge.Application;

public class Controller
{
    private bool isPlaying;
    private float lastMoveMadeTime;
    private bool isWaitingToPlayMove;
    private Move moveToPlay;
    private float playMoveTime;
    private Board board;
    readonly Random rng;
    private readonly MoveGenerator moveGenerator;
    readonly string[] botMatchStartFens;
    private int botMatchGameIndex;
   public ChessPlayer PlayerWhite { get; private set; }
   public ChessPlayer PlayerBlack {get;private set;} 
    public Controller(IChessBot player1, IChessBot player2)
    {
        rng = new Random();
        moveGenerator = new MoveGenerator();
        PlayerWhite = CreatePlayer(player1);
        PlayerBlack = CreatePlayer(player2);
        botMatchStartFens = FileHelper.ReadResourceFile("Fens.txt").Split('\n').Where(fen => fen.Length > 0).ToArray();
    }

    public void StartNewGame()
    {
        board = new Board();
        isPlaying = true;
        board.LoadPosition(botMatchStartFens[botMatchGameIndex / 2]);
        NotifyTurnToMove();
    }

    ChessPlayer PlayerToMove => board.IsWhiteToMove ? PlayerWhite : PlayerBlack;
        ChessPlayer PlayerNotOnMove => board.IsWhiteToMove ? PlayerBlack : PlayerWhite;
    Move GetMove()
    {
        API.Board botBoard = new(board);
        try
        {
            API.Timer timer = new(PlayerToMove.TimeRemainingMs, PlayerNotOnMove.TimeRemainingMs,
                GameDurationMilliseconds, IncrementMilliseconds);
            API.Move move = PlayerToMove.Bot.Think(botBoard, timer);
            return new Move(move.RawValue);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        return Move.NullMove;
    }

    ChessPlayer CreatePlayer(IChessBot player)
    {
        return new ChessPlayer(player, GameDurationMilliseconds);
    }

    void NotifyTurnToMove()
    {
        Stopwatch timer = Stopwatch.StartNew();
        Move move = GetMove();
        timer.Stop();
        PlayerToMove.UpdateClock(timer.ElapsedMilliseconds);
        OnMoveChosen(move);
    }

    void OnMoveChosen(Move chosenMove)
    {
        if (IsLegal(chosenMove))
        {
            PlayerToMove.AddIncrement(IncrementMilliseconds);
            moveToPlay = chosenMove;
            isWaitingToPlayMove = true;
            playMoveTime = lastMoveMadeTime + MinMoveDelay;
            PlayMove(chosenMove);

        }
        else
        {
            Console.WriteLine("Illegal Move");
        } 
    }

    void PlayMove(Move move)
    {
        if (isPlaying)
        {
            lastMoveMadeTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            board.MakeMove(move, false);
            GameResult result = Arbiter.GetGameState(board);
            if (result == GameResult.InProgress)
            {
                NotifyTurnToMove();
            }
            else
            {
                EndGame(result);
            }
        }
    }

    void EndGame(GameResult result)
    {
        if (isPlaying)
        {
            isPlaying = false;
            isWaitingToPlayMove = false;
            Console.WriteLine(PGNCreator.CreatePGN(board, result, "White", "Black"));
        }
    }
    bool IsLegal(Move move)
    {
        var moves = moveGenerator.GenerateMoves(board);
        foreach (var legalMove in moves)
        {
            if (move.Value == legalMove.Value)
            {
                return true;
            }
        }
        return false;
    }
}