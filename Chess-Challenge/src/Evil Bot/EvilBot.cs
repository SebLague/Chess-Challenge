using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.Application;


namespace ChessChallenge.Example
{
    public class EvilEdge
    {
        public Move move;
        public EvilNode node;

        public EvilEdge(Move move, EvilNode node)
        {
            this.move = move;
            this.node = node;
        }
    }

    public class EvilNode
    {
        public Move currentBestMove { get; set; } = Move.NullMove;
        public double? moveStrength { get; set; } = null;
        public ulong? position { get; set; } = null;
        public List<EvilEdge>? edges { get; set; } = null;
    }

    public class EvilBot : IChessBot
    {
        double[] pieceValues = { 0, 1, 3.2, 3, 5, 9, 500 };

        private EvilNode root;

        public Move Think(Board board, Timer timer)
        {
            double bestMoveEval = 1000000;

            var root = new EvilNode();

            Search(board, root, 4);

            return root.currentBestMove;
        }

        void Search(Board board, EvilNode node, int depth)
        {
            if (depth == 0)
            {
                node.moveStrength = EvaluatePosition(board);
                return;
            }

            Move[] allMoves = board.GetLegalMoves();
            if (allMoves.Length == 0)
            {
                node.moveStrength = EvaluatePosition(board);
                return;
            }

            if (node.edges == null)
            {
                node.edges = board.GetLegalMoves().Select(move => new EvilEdge(move, new EvilNode())).ToList();
            }

            var bestMoves = new List<Move>();
            var opponentsEvaluation = 1000000.0;

            foreach (var edge in node.edges)
            {
                board.MakeMove(edge.move);
                Search(board, edge.node, depth - 1);
                if (edge.node.moveStrength == opponentsEvaluation)
                {
                    bestMoves.Add(edge.move);
                }
                else if (edge.node.moveStrength < opponentsEvaluation)
                {
                    opponentsEvaluation = (double)edge.node.moveStrength;
                    bestMoves = new List<Move> { edge.move };
                }

                board.UndoMove(edge.move);
            }

            var rng = new Random();
            node.currentBestMove = bestMoves[rng.Next(bestMoves.Count)];

            node.moveStrength = -opponentsEvaluation;
        }


        double EvaluatePosition(Board board)
        {
            if (board.IsInCheckmate())
            {
                return -100000;
            }

            if (board.IsDraw())
            {
                return 0;
            }

            var allPieceLists = board.GetAllPieceLists();

            var evaluation = 0.0;

            foreach (var pieceList in allPieceLists)
            {
                double pieceListValue = pieceList.Count * pieceValues[(int)pieceList.TypeOfPieceInList];

                if (!pieceList.IsWhitePieceList)
                {
                    pieceListValue *= -1;
                }

                evaluation += pieceListValue;
            }

            if (!board.IsWhiteToMove)
            {
                evaluation *= -1;
            }

            return evaluation;
        }
    }
}