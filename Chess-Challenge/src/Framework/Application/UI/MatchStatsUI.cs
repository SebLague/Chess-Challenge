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
                Color white = new(225, 225, 225, 225);
                Color red = new Color(200, 0, 0, 255);
                Color green = new Color(0, 200, 0, 255);
                Vector2 startPos = UIHelper.Scale(new Vector2(1500, 100));
                float spacingY = UIHelper.Scale(35);

                DrawNextText($"Game {controller.CurrGameNumber} of {controller.TotalGameCount}", headerFontSize, Color.WHITE);
                startPos.Y += spacingY * 2;

                DrawStats(controller.BotStatsA);
                startPos.Y += spacingY * 2;
                DrawStats(controller.BotStatsB);
                startPos.Y += spacingY * 2;

                double eloDifference = CalculateElo(controller.BotStatsA.NumWins, controller.BotStatsA.NumDraws, controller.BotStatsA.NumLosses);
                double eloMargin = CalculateErrorMargin(controller.BotStatsA.NumWins, controller.BotStatsA.NumDraws, controller.BotStatsA.NumLosses);

                DrawNextText($"Elo Difference:", headerFontSize, Color.WHITE);
                if(double.IsNaN(eloMargin))
                    if(double.IsNaN(eloDifference))
                        DrawNextText("Nan", regularFontSize, Color.GRAY);
                    else if(double.IsInfinity(eloDifference))
                    if((int)eloDifference > 0)
                        DrawNextText("Inf", regularFontSize, Color.GRAY);
                    else DrawNextText("-Inf", regularFontSize, Color.GRAY);
                    else
                        DrawNextText($"{(int)eloDifference}", regularFontSize, Color.GRAY);
                else
                    if(double.IsNaN(eloDifference))
                        DrawNextText($"Nan +/- {(int)eloMargin}", regularFontSize, Color.GRAY);
                    else if(double.IsInfinity(eloDifference))
                    if((int)eloDifference > 0)
                        DrawNextText($"Inf +/- {(int)eloMargin}", regularFontSize, Color.GRAY);
                    else DrawNextText($"-Inf +/- {(int)eloMargin}", regularFontSize, Color.GRAY);
                    else
                        DrawNextText($"{(int)eloDifference} +/- {(int)eloMargin}", regularFontSize, Color.GRAY);

                double los = LOS(controller.BotStatsA.NumWins, controller.BotStatsA.NumLosses);
                if(!double.IsNaN(los)){
                    DrawNextText("LOS:", headerFontSize, Color.WHITE);
                    DrawNextText($"{(double)(int)(los*1000)/10}%", regularFontSize, Color.GRAY);
                }
                    

                void DrawStats(ChallengeController.BotMatchStats stats)
                {
                    DrawNextText(stats.BotName + ":", nameFontSize, Color.WHITE);
                    DrawNextText($"Score: +{stats.NumWins} ={stats.NumDraws} -{stats.NumLosses}", regularFontSize, col);
                    DrawNextText($"Num Timeouts: {stats.NumTimeouts}", regularFontSize, col);
                    DrawNextText($"Num Illegal Moves: {stats.NumIllegalMoves}", regularFontSize, col);
                    DrawNextText($"Winrate: {(float)stats.NumWins / (controller.CurrGameNumber - 1) * 100}%", regularFontSize, green);
                    DrawNextText($"Draw rate: {(float)stats.NumDraws / (controller.CurrGameNumber - 1) * 100}%", regularFontSize, white);
                    DrawNextText($"Loss rate: {(float)stats.NumLosses / (controller.CurrGameNumber - 1) * 100}%", regularFontSize, red);
                }
           
                void DrawNextText(string text, int fontSize, Color col)
                {
                    UIHelper.DrawText(text, startPos, fontSize, 1, col);
                    startPos.Y += spacingY;
                }
            }
        }

        private static double CalculateElo(int wins, int draws, int losses){
            double score = wins + (double)draws / 2;
            int total = wins + draws + losses;
            double percentage = score / total;
            double EloDifference = CalculateEloDifference(percentage);
            return EloDifference;
        }

        private static double CalculateEloDifference(double percentage){
            return -400 * Math.Log(1 / percentage - 1) / Math.Log(10);
        }

        private static double CalculateErrorMargin(int wins, int draws, int losses){
            int total = wins + draws + losses;
            double winP = (double)wins / total;
            double drawP = (double)draws / total;
            double lossP = (double)losses / total;
            double percentage = (wins + draws * 0.5) / total;
            double winsDev = winP * Math.Pow(1 - percentage, 2);
            double drawsDev = drawP * Math.Pow(0.5 - percentage, 2);
            double lossesDev = lossP * Math.Pow(0 - percentage, 2);
            double stdDeviation = Math.Sqrt(winsDev + drawsDev + lossesDev) / Math.Sqrt(total);

            double confidenceP = 0.95;
            double minConfidenceP = (1 - confidenceP) / 2;
	        double maxConfidenceP = 1 - minConfidenceP;
            double devMin = percentage + phiInv(minConfidenceP) * stdDeviation;
            double devMax = percentage + phiInv(maxConfidenceP) * stdDeviation;

            double difference = CalculateEloDifference(devMax) - CalculateEloDifference(devMin);

            return difference / 2;

        }

        private static double phiInv(double p){
            return Math.Sqrt(2) * CalculateInverseErrorFunction(2 * p - 1);
        }
        private static double CalculateInverseErrorFunction(double x){
            double pi = Math.PI;
            double a = 8 * (pi - 3) / (3 * pi * (4 - pi));
            double y = Math.Log(1 - x * x);
            double z = 2 / (pi * a) * y / 2;
            
            double ret = Math.Sqrt(Math.Sqrt(z * z - y / a) - z);

            if(x < 0)
                return -ret;

            return ret;
        }
        private static double LOS(int wins, int losses){
            return .5 + .5 * erf((wins - losses)/Math.Sqrt(2 * (wins + losses)));
        }
        private static double erf(double x){
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            double t = 1.0 / (1.0 + p*x);
            double y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t*Math.Exp(-x*x);

            return sign * y;
        }
    }
}
