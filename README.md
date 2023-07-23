# Chess Coding Challenge (C#)
Welcome to the [chess coding challenge](https://youtu.be/iScy18pVR58)! This is a friendly competition in which your goal is to create a small chess bot (in C#) using the framework provided in this repository.
Once submissions close, these bots will battle it out to discover which bot is best!

I will then create a video exploring the implementations of the best and most unique/interesting bots.
I also plan to make a small game that features these most interesting/challenging entries, so that everyone can try playing against them.

## Change Log
I unfortunately missed a serious bug in the API and have had to update the project. Please keep an eye on the change log here in case I've made any other horrifying mistakes. Apologies for the inconvenience. The version you are currently using will be printed to the console when running the program (unless you are using v1.0, in which case nothing will be printed).
* V1.1 Bug fix for `board.GetPiece()` and `PieceList` functions. Added `Board.CreateBoardFromFEN()` function.
  * V1.11 UI changes: Added coordinate names to board UI and fixed human player input bug.
  * V1.12 Small fixes to `board.IsDraw()`: Fifty move counter is now updated properly during search, and insufficient material is now detected for lone bishops on the same square colour.

## Submission Due Date
October 1st 2023.<br>
You can submit your entry [here](https://forms.gle/6jjj8jxNQ5Ln53ie6).

## How to Participate
* Install an IDE such as [Visual Studio](https://visualstudio.microsoft.com/downloads/).
* Install [.NET 6.0](https://dotnet.microsoft.com/en-us/download)
* Download this repository and open the Chess-Challenge project in your IDE.
* Try building and running the project.
  * If a window with a chess board appears — great!
  * If it doesn't work, please take a look at the [issues page](https://github.com/SebLague/Chess-Challenge/issues) to see if anyone is having a similar issue. If not, post about it there with any details such as error messages, operating system etc.
    * See also the FAQ/troubleshooting section at the bottom of the page.
* Open the MyBot.cs file _(located in src/MyBot)_ and write some code!
  * You might want to take a look at the [Documentation](https://seblague.github.io/chess-coding-challenge/documentation/) first, and the Rules too!
* Build and run the program again to test your changes.
  * For testing, you have three options in the program:
    * You can play against the bot yourself (Human vs Bot)
    * The bot can play a match against itself (MyBot vs MyBot)
    * The bot can play a match against a simple example bot (MyBot vs EvilBot).<br>You could also replace the EvilBot code with your own code, to test two different versions of your bot against one another.
* Once you're happy with your chess bot, head over to the [Submission Page](https://forms.gle/6jjj8jxNQ5Ln53ie6) to enter it into the competition.
  * You will be able to edit your entry up until the competition closes.

## Rules
* You may participate alone, or in a group of any size.
* You may submit a maximum of two entries.
  * Please only submit a second entry if it is significantly different from your first bot (not just a minor tweak).
  * Note: you will need to log in with a second Google account if you want submit a second entry.
* Only the following namespaces are allowed:
    * `ChessChallenge.API`
    * `System`
    * `System.Numerics`
    * `System.Collections.Generic`
    * `System.Linq`
      * You may not use the `AsParallel()` function
* As implied by the allowed namespaces, you may not read data from a file or access the internet, nor may you create any new threads or tasks to run code in parallel/in the background.
* You may not use the unsafe keyword.
* You may not store data inside the name of a variable/function/class etc (to be extracted with `nameof()`, `GetType().ToString()`, `Environment.StackTrace` and so on). Thank you to [#12](https://github.com/SebLague/Chess-Challenge/issues/12) and [#24](https://github.com/SebLague/Chess-Challenge/issues/24).
* If your bot makes an illegal move or runs out of time, it will lose the game.
   * Games are played with 1 minute per side by default (this can be changed in the settings class). The final tournament time control is TBD, so your bot should not assume a particular time control, and instead respect the amount of time left on the timer (given in the Think function).
* Your bot may not use more than 256mb of memory for creating look-up tables (such as a transposition table).
* If you have added a constructor to MyBot (for generating look up tables, etc.) it may not take longer than 5 seconds to complete.
* All of your code/data must be contained within the _MyBot.cs_ file.
   * Note: you may create additional scripts for testing/training your bot, but only the _MyBot.cs_ file will be submitted, so it must be able to run without them.
   * You may not rename the _MyBot_ struct or _Think_ function contained in the _MyBot.cs_ file.
   * The code in MyBot.cs may not exceed the _bot brain capacity_ of 1024 (see below).

## Bot Brain Capacity
There is a size limit on the code you create called the _bot brain capacity_. This is measured in ‘tokens’ and may not exceed 1024. The number of tokens you have used so far is displayed on the bottom of the screen when running the program.

All names (variables, functions, etc.) are counted as a single token, regardless of length. This means that both lines of code: `bool a = true;` and `bool myObscenelyLongVariableName = true;` count the same. Additionally, the following things do not count towards the limit: white space, new lines, comments, access modifiers, commas, and semicolons.

## FAQ and Troubleshooting
* [Running on Linux](https://github.com/SebLague/Chess-Challenge/discussions/3)
* [How to run if using a different code editor](https://github.com/SebLague/Chess-Challenge/issues/85)
* Issues with illegal moves or errors when making/undoing a move
  * Make sure that you are making and undoing moves in the correct order, and that you don't forget to undo a move when exitting early from a function for example.
* How to tell what colour MyBot is playing
  * You can look at `board.IsWhiteToMove` when the Think function is called
* `GetPiece()` function is giving a null piece after making a move
  * Please make sure you are using the latest version of the project, there was a bug with this function in the original version
  
