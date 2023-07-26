using System;
using System.Diagnostics;
using System.IO;
using ChessChallenge.API;

public class Stockfish : IChessBot {
    private Process stockfishProcess;
    private StreamWriter Ins() => stockfishProcess.StandardInput;
    private StreamReader Outs() => stockfishProcess.StandardOutput;

    private static int SLOW_MOVER = 0;
    private static int MOVE_OVERHEAD = 500;

    public Stockfish() {
        var stockFishBin = Environment.GetEnvironmentVariable("STOCKFISH_BIN");
        if (stockFishBin == null) {
            throw new Exception("Missing environment variable: 'STOCKFISH_BIN'");
        }
        var stockFishLevel = Environment.GetEnvironmentVariable("STOCKFISH_LEVEL");
        if (stockFishLevel == null) {
            throw new Exception("Missing environment variable: 'STOCKFISH_LEVEL'");
        }
        int skillLevel = Int32.Parse(stockFishLevel);
        stockfishProcess = new();
        stockfishProcess.StartInfo.RedirectStandardOutput = true;
        stockfishProcess.StartInfo.RedirectStandardInput = true;
        stockfishProcess.StartInfo.FileName = stockFishBin;
        stockfishProcess.Start();

        Ins().WriteLine("uci");
        string? line;
        var isOk = false;

        while ((line = Outs().ReadLine()) != null) {
            if (line == "uciok") {
                isOk = true;
                break;
            }
        }

        if (!isOk) {
            throw new Exception("Failed to communicate with stockfish");
        }

        Ins().WriteLine($"setoption name Skill Level value {skillLevel}");
        Ins().WriteLine($"setoption name Slow Mover value {SLOW_MOVER}");
        Ins().WriteLine($"setoption name Move Overhead value {MOVE_OVERHEAD}");
    }

    public Move Think(Board board, Timer timer) {
        Ins().WriteLine("ucinewgame");
        Ins().WriteLine($"position fen {board.GetFenString()}");
        var timeString = board.IsWhiteToMove ? "wtime" : "btime";
        Ins().WriteLine($"go {timeString} {timer.MillisecondsRemaining}");

        string? line;
        Move? move = null;

        while ((line = Outs().ReadLine()) != null) {
            if (line.StartsWith("bestmove")) {
                var moveStr = line.Split()[1];
                move = new Move(moveStr, board);

                break;
            }
        }

        if (move == null) {
            throw new Exception("Engine crashed");
        }

        return (Move)move;
    }
}
