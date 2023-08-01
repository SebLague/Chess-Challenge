using System;
using System.Diagnostics;
using System.IO;
using ChessChallenge.API;

public class SebLague: IChessBot {
    private Process sebLagueProcess;
    private StreamWriter Ins() => sebLagueProcess.StandardInput;
    private StreamReader Outs() => sebLagueProcess.StandardOutput;

    public SebLague() {
        var sebLagueBin = Environment.GetEnvironmentVariable("SEBLAGUE_BIN");
        if (sebLagueBin == null) {
            throw new Exception("Missing environment variable: 'SEBLAGUE_BIN'");
        }
        sebLagueProcess = new();
        sebLagueProcess.StartInfo.RedirectStandardOutput = true;
        sebLagueProcess.StartInfo.RedirectStandardInput = true;
        sebLagueProcess.StartInfo.FileName = sebLagueBin;
        sebLagueProcess.Start();

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
