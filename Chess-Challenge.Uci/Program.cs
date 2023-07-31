// See https://aka.ms/new-console-template for more information

using ChessChallenge.Bot;

var engine = new UciEngine();
var message = string.Empty;
while (message != "quit")
{
    message = Console.ReadLine();
    if (!string.IsNullOrEmpty(message)) engine.ReceiveCommand(message);
}