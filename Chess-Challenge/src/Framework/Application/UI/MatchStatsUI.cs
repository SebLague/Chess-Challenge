using Raylib_cs;
using System.Numerics;
using System;
using System.Security.Cryptography;

namespace ChessChallenge.Application
{
    public static class MatchStatsUI
    {
        public static void DrawMatchStats(ChallengeController controller)
        {
            if (controller.PlayerWhite.IsBot && controller.PlayerBlack.IsBot)
            {
                int nameFontSize = UIHelper.ScaleInt(40);
                int regularFontSize = UIHelper.ScaleInt(35);
                int headerFontSize = UIHelper.ScaleInt(45);
                Color col = new(180, 180, 180, 255);
                Vector2 startPos = UIHelper.Scale(new Vector2(1500, 180));
                float spacingY = UIHelper.Scale(35);

                DrawNextText($"Game {controller.CurrGameNumber} of {controller.TotalGameCount}", headerFontSize, Color.WHITE);
                startPos.Y += spacingY * 2;

                DrawStats(controller.BotStatsA);
                startPos.Y += spacingY * 2;
                DrawStats(controller.BotStatsB);

                void DrawStats(ChallengeController.BotMatchStats stats)
                {
                    double Erf(double x)
                    {
                        const double a1 = 0.254829592;
                        const double a2 = -0.284496736;
                        const double a3 = 1.421413741;
                        const double a4 = -1.453152027;
                        const double a5 = 1.061405429;
                        const double p = 0.3275911;
                        double t = 1.0 / (1.0 + p * Math.Abs(x));
                        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
                        return (x < 0.0 ? -y : y);
                    }

                    int numgames = stats.NumWins + stats.NumDraws + stats.NumLosses;
                    if (numgames == 0) numgames = 1;
                    double points = (double)stats.NumWins + (double)stats.NumDraws / 2;
                    double winpercent = points/ numgames * 100;
                    double pointspergame = points / numgames;

                    double los = 0.00;
                    double LOS = (.5 + .5 * Erf((stats.NumWins - stats.NumLosses) / Math.Sqrt(2.0 * (stats.NumWins + stats.NumLosses)))) * 100;
                    if (!Double.IsNaN(LOS)) los = LOS;

                    double elo = -Math.Log(1.0 / pointspergame - 1.0) * 400.0 / Math.Log(10.0);

                    DrawNextText(stats.BotName + ":", nameFontSize, Color.WHITE);
                    DrawNextText($"Result: +{stats.NumWins} ={stats.NumDraws} -{stats.NumLosses}", regularFontSize, col);
                    DrawNextText($"Score: {points.ToString("0.0")}/{numgames}", regularFontSize, col);
                    DrawNextText($"Win%: {winpercent.ToString("0.00")}", regularFontSize, col);
                    DrawNextText($"LOS%: {los.ToString("0.00")}", regularFontSize, col);
                    DrawNextText($"Elo: {elo.ToString("0")}", regularFontSize, col);
                    DrawNextText($"Timeouts: {stats.NumTimeouts}", regularFontSize, col);
                    DrawNextText($"Illegal Moves: {stats.NumIllegalMoves}", regularFontSize, col);
                }
           
                void DrawNextText(string text, int fontSize, Color col)
                {
                    UIHelper.DrawText(text, startPos, fontSize, 1, col);
                    startPos.Y += spacingY;
                }
            }
        }
    }
}