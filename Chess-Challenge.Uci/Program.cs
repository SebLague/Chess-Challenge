// See https://aka.ms/new-console-template for more information

using ChessChallenge.Bot;

var engine = new UciEngine();
var message = string.Empty;
var run = true;
while (run)
{
    message = Console.ReadLine();
    run = string.IsNullOrEmpty(message) || engine.ReceiveCommand(message);
}