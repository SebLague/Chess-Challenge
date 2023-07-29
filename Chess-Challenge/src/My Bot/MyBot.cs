using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChessChallenge.Application;
using Timer = ChessChallenge.API.Timer;
using System.Xml.Linq;

//using static ChessChallenge.Application.ConsoleHelper;

public class Edge : IComparable<Edge>
{
    public Move move;
    public Node node;

    public Edge(Move move, Node node)
    {
        this.move = move;
        this.node = node;
    }
    public int CompareTo(Edge? other)
    {
        return other.node.moveStrength.CompareTo(this.node.moveStrength);
    }
}
public class Node
{
    public int moveStrength { get; set; }
    //public ulong? position { get; set; } = null;
    public List<Edge>? edges { get; set; } = null;

    public Node(int moveStrength)
    {
        this.moveStrength = moveStrength;
    }
}

public class MyBot : IChessBot
{
    private readonly int[] _pieceValues = { 0, 100, 320, 300, 500, 900, 50000 };

    private Node? _root = null;

    private bool _isWhite = false;

    private int _bigNumber = Int32.MaxValue / 10;

    public Move Think(Board board, Timer timer)
    {
        //_root = new Node();
        if (_root == null)
        {
            _isWhite = board.IsWhiteToMove;
            _root = new Node(_isWhite ? -_bigNumber : _bigNumber);
        }
        else
        {
            var lastMove = board.GameMoveHistory[^1];
            var chosenEdge = _root.edges?.Where(edge => edge.move.Equals(lastMove))?.First();
            if (chosenEdge != null)
            {
                _root = chosenEdge.node;
            }
            else
            {
                _root = new Node(_isWhite ? -_bigNumber : _bigNumber);
            }
        }

        
        Search(board, _root, 4);

        ConsoleHelper.Log(_root.moveStrength.ToString());
        var ourMove = _isWhite ? _root.edges.First() : _root.edges.Last();
        _root = ourMove.node;
        return ourMove.move;
    }

    void Search(Board board, Node node, int depth)
    {
        if (depth == 0)
        {
            node.moveStrength = EvaluatePosition(board);
            return;
        }

        if (node.edges == null)
        {
            node.edges = board.GetLegalMoves().Select(move => new Edge(move, new Node(!board.IsWhiteToMove ? -_bigNumber : _bigNumber))).ToList();
        }

        if (node.edges.Count == 0)
        {
            node.moveStrength = EvaluatePosition(board);
            return;
        }

        foreach (var edge in node.edges)
        {
            board.MakeMove(edge.move);
            Search(board, edge.node, depth - 1);
            board.UndoMove(edge.move);
        }

        if (depth == 4)
        {
            ConsoleHelper.Log(String.Join(',', node.edges.Select(edge => edge.node.moveStrength)));
            node.edges.Sort();
            ConsoleHelper.Log(String.Join(',', node.edges.Select(edge => edge.node.moveStrength)));
        }
        else
        {
            node.edges.Sort();
        }

        node.moveStrength = board.IsWhiteToMove ? node.edges.First().node.moveStrength : node.edges.Last().node.moveStrength;
    }


    int EvaluatePosition(Board board)
    {
        int evaluation = 0;
        if (board.IsInCheckmate())
        {
            evaluation = board.IsWhiteToMove ? -_bigNumber : _bigNumber;
        }
        else if (board.IsDraw())
        {
            evaluation = 0;
        }
        else
        {
            var allPieceLists = board.GetAllPieceLists();

            foreach (var pieceList in allPieceLists)
            {
                int pieceListValue = pieceList.Count * _pieceValues[(int)pieceList.TypeOfPieceInList];

                if (!pieceList.IsWhitePieceList)
                {
                    pieceListValue *= -1;
                }

                evaluation += pieceListValue;
            }
        }

        return evaluation;
    }
}