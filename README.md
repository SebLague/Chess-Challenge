# Chess Coding Challenge (C#)
Welcome to the [chess coding challenge](https://youtu.be/iScy18pVR58)! This is a friendly competition in which your goal is to create a small chess bot (in C#) using the framework provided in this repository.
Once submissions close, these bots will battle it out to discover which bot is best!

I will then create a video exploring the implementations of the best and most unique/interesting bots.
I also plan to make a small game that features these most interesting/challenging entries, so that everyone can try playing against them.

## Submission Due Date
October 1st 2023.<br>
You can submit your entry [here](https://forms.gle/6jjj8jxNQ5Ln53ie6).

## How to Participate
* Install an an IDE such as [Visual Studio](https://visualstudio.microsoft.com/downloads/).
* Install [.NET 6.0](https://dotnet.microsoft.com/en-us/download)
* Download this repository and open the Chess-Challenge project in your IDE.
* Try building and running the project.
  * If a window with a chess board appears — great!
  * If it doesn't work, please take a look at the [issues page](https://github.com/SebLague/Chess-Challenge/issues) to see if anyone is having a similar issue. If not, post about it there with any details such as error messages, operating system etc.
* Open the MyBot.cs file _(located in src/MyBot)_ and write some code!
  * You might want to take a look at the [Documentation](https://seblague.github.io/chess-coding-challenge/documentation/) first, and the Rules too!
* Build and run the program again to test your changes.
  * For testing, you have three options in the program:
    * You can play against the bot yourself (Human vs Bot)
    * The bot can play a match against itself (MyBot vs MyBot)
    * The bot can play a match against a simple example bot (MyBot vs EvilBot).<br>You could also replace the EvilBot code with your own code, to test two different versions of your bot against one another.
* Once you're happy with your chess bot, head over to the [Submission Page](https://forms.gle/6jjj8jxNQ5Ln53ie6) to enter it into the competition.
  * You will be able to edit your entry up until the competition closes.
* if you wish to run using dotnet cli, follow these steps:
  * dotnet restore
  * cd Chess-Challenge
  * dotnet run
## Rules
* You may participate alone, or in a group of any size.
* Only the following namespaces are allowed:
    * ChessChallenge.API
    * System
    * System.Linq
    * System.Numerics
    * System.Collections.Generic
* As implied by the allowed namespaces, you may not read data from a file or access the internet, nor may you create any new threads or tasks to run code in parallel/in the background.
* You may not use the unsafe keyword.
* You may not store data inside the name of a variable/function/class etc (to be extracted with nameof(), GetType().ToString(), and so on).
   * Very clever idea though, thank you to [#12](https://github.com/SebLague/Chess-Challenge/issues/12).
* All of your code/data must be contained within the _MyBot.cs_ file.
   * Note: you may create additional scripts for testing/training your bot, but only the _MyBot.cs_ file will be submitted, so it must be able to run without them.
   * You may not rename the _MyBot_ struct or _Think_ function contained in the _MyBot.cs_ file.
   * The code in MyBot.cs may not exceed the _bot brain capacity_ of 1024 (see below).

## Bot Brain Capacity
There is a size limit on the code you create called the _bot brain capacity_. This is measured in ‘tokens’ and may not exceed 1024. The number of tokens you have used so far is displayed on the bottom of the screen when running the program.

All names (variables, functions, etc.) are counted as a single token, regardless of length. This means that both lines of code: `bool a = true;` and `bool myObscenelyLongVariableName = true;` count the same. Additionally, the following things do not count towards the limit: white space, new lines, comments, access modifiers, commas, and semicolons.

## FAQ
Nothing yet
  
