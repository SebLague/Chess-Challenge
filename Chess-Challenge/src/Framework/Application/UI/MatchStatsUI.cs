using Raylib_cs;
using System.Numerics;
using System;
using static System.Formats.Asn1.AsnWriter;

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
                Color white = new(225, 225, 225, 225);
                Color red = new Color(200, 0, 0, 255);
                Color green = new Color(0, 200, 0, 255);
                Vector2 startPos = UIHelper.Scale(new Vector2(1500, 150));
                float spacingY = UIHelper.Scale(35);

                DrawNextText($"Game {controller.CurrGameNumber} of {controller.TotalGameCount}", headerFontSize, Color.WHITE);
                startPos.Y += spacingY * 2;

                DrawStats(controller.BotStatsA);
                startPos.Y += spacingY * 2;
                DrawStats(controller.BotStatsB);

                startPos.Y += spacingY * 2;

                string eloDifference = CalculateElo(controller.BotStatsA.NumWins, controller.BotStatsA.NumDraws, controller.BotStatsA.NumLosses);
                string errorMargin = CalculateErrorMargin(controller.BotStatsA.NumWins, controller.BotStatsA.NumDraws, controller.BotStatsA.NumLosses);

                DrawNextText($"Elo Difference:", headerFontSize, Color.WHITE);
                DrawNextText($"{eloDifference} {errorMargin}", regularFontSize, Color.GRAY);

                void DrawStats(ChallengeController.BotMatchStats stats)
                {
                    DrawNextText(stats.BotName + ":", nameFontSize, Color.WHITE);
                    DrawNextText($"Score: +{stats.NumWins} ={stats.NumDraws} -{stats.NumLosses}", regularFontSize, white);
                    DrawNextText($"Num Timeouts: {stats.NumTimeouts}", regularFontSize, col);
                    DrawNextText($"Num Illegal Moves: {stats.NumIllegalMoves}", regularFontSize, col);
                    DrawNextText($"Winrate: {(float)stats.NumWins / (controller.CurrGameNumber - 1) * 100}%", regularFontSize, green);
                    DrawNextText($"Draw rate: {(float)stats.NumDraws / (controller.CurrGameNumber - 1) * 100}%", regularFontSize, white);
                    DrawNextText($"Loss rate: {(float)stats.NumLosses / (controller.CurrGameNumber - 1) * 100}%", regularFontSize, red);
                }
                DrawNextText($"Average moves per game: {controller.trueTotalMovesPlayed / controller.CurrGameNumber - 1}", regularFontSize, white);


                void DrawNextText(string text, int fontSize, Color col)
                {
                    UIHelper.DrawText(text, startPos, fontSize, 1, col);
                    startPos.Y += spacingY;
                }
            }
        }

        private static string CalculateElo(int wins, int draws, int losses)
        {
            double score = wins + draws / 2;
            int totalGames = wins + draws + losses;
            double difference = CalculateEloDifference(score / totalGames);
            if ((int)difference == -2147483648)
            {
                if (difference > 0) return "+Inf";
                else return "-Inf";
            }

            return $"{(int)difference}";
        }

        private static double CalculateEloDifference(double percentage)
        {
            return -400 * Math.Log(1 / percentage - 1) / 2.302;
        }

        private static string CalculateErrorMargin(int wins, int draws, int losses)
        {
            double total = wins + draws + losses;
            double winP = wins / total;
            double drawP = draws / total;
            double lossP = losses / total;

            double percentage = (wins + draws / 2) / total;
            double winDev = winP * Math.Pow(1 - percentage, 2);
            double drawsDev = drawP * Math.Pow(0.5 - percentage, 2);
            double lossesDev = lossP * Math.Pow(0 - percentage, 2);

            double stdDeviation = Math.Sqrt(winDev + drawsDev + lossesDev) / Math.Sqrt(total);

            double confidenceP = 0.95;
            double minConfidenceP = (1 - confidenceP) / 2;
            double maxConfidenceP = 1 - minConfidenceP;
            double devMin = percentage + PhiInv(minConfidenceP) * stdDeviation;
            double devMax = percentage + PhiInv(maxConfidenceP) * stdDeviation;

            double difference = CalculateEloDifference(devMax) - CalculateEloDifference(devMin);
            double margin = Math.Round(difference / 2);
            if (double.IsNaN(margin)) return "";
            return $"+/- {margin}";
        }

        private static double PhiInv(double p)
        {
            return Math.Sqrt(2) * CalculateInverseErrorFunction(2 * p - 1);
        }

        private static double CalculateInverseErrorFunction(double x)
        {
            double a = 8 * (Math.PI - 3) / (3 * Math.PI * (4 - Math.PI));
            double y = Math.Log(1 - x * x);
            double z = 2 / (Math.PI * a) + y / 2;

            double ret = Math.Sqrt(Math.Sqrt(z * z - y / a) - z);
            if (x < 0) return -ret;
            return ret;
        }
    }
}
