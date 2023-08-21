using Raylib_cs;
using System.Numerics;
using System;

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
                Color Red = new(255, 0, 0, 255);
                Color White = new(255, 255, 255);
                Color Green = new(0, 255, 0);
                Vector2 startPos = UIHelper.Scale(new Vector2(1500, 250));
                float spacingY = UIHelper.Scale(35);

                DrawNextText($"Game {controller.CurrGameNumber} of {controller.TotalGameCount}", headerFontSize, Color.WHITE);
                startPos.Y += spacingY * 2;

                DrawStats(controller.BotStatsA);
                startPos.Y += spacingY * 2;
                DrawStats(controller.BotStatsB);
                startPos.Y += spacingY * 2;
                
                void DrawStats(ChallengeController.BotMatchStats stats)
                {
                    DrawNextText(stats.BotName + ":", nameFontSize, Color.WHITE);
                    DrawNextText($"Score: +{stats.NumWins} ={stats.NumDraws} -{stats.NumLosses}", regularFontSize, col);
                    DrawNextText($"Num Timeouts: {stats.NumTimeouts}", regularFontSize, col);
                    DrawNextText($"Num Illegal Moves: {stats.NumIllegalMoves}", regularFontSize, col);
                    DrawNextText($"Win Percent: {(double)stats.NumWins/(controller.CurrGameNumber - 1)}", regularFontSize, Green);
                    DrawNextText($"Draw Percent: {(double)stats.NumDraws/(controller.CurrGameNumber - 1)}", regularFontSize, White);
                    DrawNextText($"Lose Percent: {(double)stats.NumLosses/(controller.CurrGameNumber - 1)}", regularFontSize, Red);
                }
           
                void DrawNextText(string text, int fontSize, Color col)
                {
                    UIHelper.DrawText(text, startPos, fontSize, 1, col);
                    startPos.Y += spacingY;
                }

                double CalculateEloDifference(double percent){
                    return -400 * Math.log(1 / percentage - 1) / Math.log(10)
                }
            }
        }
    }
}
