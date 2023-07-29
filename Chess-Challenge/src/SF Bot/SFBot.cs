using System;
using System.Diagnostics;
using System.IO;
using ChessChallenge.API;

public class SFBot : IChessBot
{
    private readonly Process _stockfishProcess;
    private StreamWriter Ins() => _stockfishProcess.StandardInput;
    private StreamReader Outs() => _stockfishProcess.StandardOutput;

    private const int SkillLevel = 10;
    /* Select the skill level of stockfish 7 (min = 0, max = 20)
    These are Elo approximations of each level:
     00    1350
     01    1500
     02    1600
     03    1700
     04    1800
     05    1875
     06    1950
     07    2050
     08    2100
     09    2200
     10    2250
     11    2350
     12    2400
     13    2500
     14    2550
     15    2600
     16    2700
     17    2750
     18    2800
     19    2900
     20    3000
     */

    public SFBot()
    {
        // Path to the executable
        const string stockfishExe = "C:/stockfish/stockfish_7_x64_popcnt.exe";

        if (stockfishExe != null)
        {
            _stockfishProcess = new Process();
            _stockfishProcess.StartInfo.RedirectStandardOutput = true;
            _stockfishProcess.StartInfo.RedirectStandardInput = true;
            _stockfishProcess.StartInfo.FileName = stockfishExe;
            _stockfishProcess.Start();
            Ins().WriteLine("uci");
            var isOk = false;

            while (Outs().ReadLine() is { } line)
            {
                if (line != "uciok") continue;
                isOk = true;
                break;
            }
            if (!isOk)
            {
                throw new Exception("Failed to communicate with stockfish");
            }
            Ins().WriteLine($"setoption name Skill Level value {SkillLevel}");
        }
    }

    public Move Think(Board board, Timer timer)
    {
        Ins().WriteLine("ucinewgame");
        Ins().WriteLine($"position fen {board.GetFenString()}");
        var timeString = board.IsWhiteToMove ? "wtime" : "btime";
        Ins().WriteLine($"go {timeString} {timer.MillisecondsRemaining}");
        Move? move = null;

        while (Outs().ReadLine() is { } line)
        {
            if (!line.StartsWith("bestmove")) continue;
            var moveStr = line.Split()[1];
            move = new Move(moveStr, board);
            break;
        }
        if (move == null)
        {
            throw new Exception("Engine crashed");
        }
        return (Move)move;
    }
}