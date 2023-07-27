using Raylib_cs;
using System.Numerics;
using System;
using System.IO;
using System.Collections.Generic;

namespace ChessChallenge.Application
{
    public static class MenuUI
    {
        public static bool PersistWhiteDropdown;
        public static bool PersistBlackDropdown;

        public static void DrawButtons(ChallengeController controller)
        {
            Vector2 buttonPos = UIHelper.Scale(new Vector2(260, 210));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(260, 55));
            float spacing = buttonSize.Y * 1.2f;
            float breakSpacing = spacing * 0.6f;

            // Game Buttons
            //if (NextButtonInRow("Human vs MyBot", ref buttonPos, spacing, buttonSize))
            //{
            //    var whiteType = controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.Bot : ChallengeController.PlayerType.Human;
            //    var blackType = !controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.Bot : ChallengeController.PlayerType.Human;
            //    controller.StartNewGame(new ChallengeController.PlayerArgs(whiteType), new ChallengeController.PlayerArgs(blackType));
            //}
            //if (NextButtonInRow("MyBot vs MyBot", ref buttonPos, spacing, buttonSize))
            //{
            //    controller.StartNewBotMatch(typeof(Bots.MyBot), typeof(Bots.MyBot));
            //}
            //if (NextButtonInRow("MyBot vs EvilBot", ref buttonPos, spacing, buttonSize))
            //{
            //    controller.StartNewBotMatch(typeof(Bots.MyBot), typeof(Bots.EvilBot));
            //}

            ChallengeController.PlayerArgs? result;
            if ((result = NextDropdownInRow<ChallengeController.PlayerArgs>(controller.AllPlayerArgs,
                controller.PlayerWhite.PlayerArgs,
                ref buttonPos,
                spacing,
                buttonSize,
                ref PersistWhiteDropdown)) != null)
            {
                controller.StartNewGame(result, controller.PlayerBlack.PlayerArgs);
            }
            if ((result = NextDropdownInRow<ChallengeController.PlayerArgs>(controller.AllPlayerArgs,
                controller.PlayerBlack.PlayerArgs,
                ref buttonPos,
                spacing,
                buttonSize,
                ref PersistBlackDropdown)) != null)
            {
                controller.StartNewGame(controller.PlayerWhite.PlayerArgs, result);
            }

            if (NextButtonInRow("ELO Tourney", ref buttonPos, spacing, buttonSize))
            {
              controller.StartELOTourney();
            }

            // Page buttons
            buttonPos.Y += breakSpacing;

            if (NextButtonInRow("Save Games", ref buttonPos, spacing, buttonSize))
            {
                string pgns = controller.AllPGNs;
                string directoryPath = Path.Combine(FileHelper.AppDataPath, "Games");
                Directory.CreateDirectory(directoryPath);
                string fileName = FileHelper.GetUniqueFileName(directoryPath, "games", ".txt");
                string fullPath = Path.Combine(directoryPath, fileName);
                File.WriteAllText(fullPath, pgns);
                ConsoleHelper.Log("Saved games to " + fullPath, false, ConsoleColor.Blue);
            }
            if (NextButtonInRow("Rules & Help", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
            }
            if (NextButtonInRow("Documentation", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
            }
            if (NextButtonInRow("Submission Page", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");
            }

            // Window and quit buttons
            buttonPos.Y += breakSpacing;

            bool isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
            string windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
            if (NextButtonInRow(windowButtonName, ref buttonPos, spacing, buttonSize))
            {
                Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
            }
            if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacing, buttonSize))
            {
                Environment.Exit(0);
            }

            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size)
            {
                bool pressed = UIHelper.Button(name, pos, size);
                pos.Y += spacingY;
                return pressed;
            }

            T? NextDropdownInRow<T>(IEnumerable<T> values, T selected, ref Vector2 pos, float spacingY, Vector2 size, ref bool persist)
            {
                var chosen = UIHelper.Dropdown<T>(values, selected, pos, size, ref persist);
                pos.Y += spacingY;
                return chosen;
            }
        }
    }
}
