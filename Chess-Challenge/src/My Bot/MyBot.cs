using ChessChallenge.API;
using System;
using System.Security.Cryptography;


public class MyBot : IChessBot
{
    //bool ranoBool = false; 4 tokens!
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves(false);
        return Apply(board,moves,0,timer);   
    }

    private static Move[] MoveFilter(Board board,Func<Board,Move,bool> condition){
        int outSize = 0;

        Move[] moves = board.GetLegalMoves(false);
        bool[] isInteresting = new bool[moves.Length];

        for(int i=0;i<moves.Length;i++){
            board.MakeMove(moves[i]);
            if(condition(board,moves[i])){
                isInteresting[i] = true;
                outSize += 1;
            }
            board.UndoMove(moves[i]);
        }

        Move[] output = new Move[outSize];

        int oi = 0;
        for(int i=0;i<moves.Length;i++){
            if(isInteresting[i]){
                output[oi] = moves[i];
            }
        }

        return output;
    }

    public static bool Interesting(Board board,Move move){
        return !board.IsDraw()
                && (board.IsInCheck() 
                    || move.IsCastles 
                    || move.IsCapture 
                    || move.PromotionPieceType.Equals(6) 
                    || move.MovePieceType.Equals(1));
    }


    private static float Eval(Board board){
        return EvalPoints(board)*EvalPoints(board)*EvalPoints(board) + EvalPositional(board);
    }

    private static Move RandoChoice(Move[] moves){
        Random rnd = new Random();
        return moves[rnd.Next() % moves.Length];
    }

    private static Move Apply(Board board,Move[] moves,int depth,Timer timer){

        int side;
        if(board.IsWhiteToMove){
            side = 1;
        }else{
            side = -1;
        }

        Move bestMove =  RandoChoice(moves);

        board.MakeMove(bestMove);
        float bestScore = side*Eval(board);
        board.UndoMove(bestMove);

        foreach (Move move in moves)
        {
            ///Console.WriteLine(Indent(depth*4)+"Consitering "+PP(move));

            board.MakeMove(move);

            Move[] possibleResponces = MoveFilter(board,Interesting);

            float newEval;
            if(possibleResponces.Length != 0 && depth <= 3 + timer.MillisecondsRemaining/20000){
                Move bestResponce = Apply(board,possibleResponces,depth+1,timer);
                board.MakeMove(bestResponce);
                newEval = side*Eval(board);
                board.UndoMove(bestResponce);
            }else{
                newEval = side*Eval(board);
            }

            board.UndoMove(move);

            if(newEval > bestScore){
                bestScore = newEval;
                bestMove = move;
            }
        }

        ///Console.WriteLine(Indent(depth*4)+"Decided "+PP(bestMove));
        return bestMove;
    }

    private static string Indent(int depth){
        String output = "";
        for(int i=0;i<depth;i++){
            output += "-";
        }
        return output;
    }

    private static string PP(Move move){
        String[] names = {"NULL","P","N","B","R","Q","K"};
        return names[(int)move.MovePieceType] 
                + " " 
                + move.StartSquare.Name 
                + move.TargetSquare.Name 
                + " x " 
                + names[(int)move.CapturePieceType];
    }

    private static float EvalPoints(Board board){
        PieceList[] pieces = board.GetAllPieceLists();

        if(board.IsInCheckmate()){
            if(board.IsWhiteToMove){
                return -1000;
            }else{
                return 1000;
            }
        }else if(board.IsDraw()){
            return 0;
        }else{
            int[] weights = {1,3,3,5,11,0,-1,-3,-3,-5,-11,0};

            int sum = 0;
            for(int i =0;i<pieces.Length;i++){
                sum += weights[i]*pieces[i].Count;
            }

            return sum;
        }
    }

    private static float EvalPositional(Board board){
        float output = 0;
        if(board.IsWhiteToMove){
            output += board.GetLegalMoves().Length;
            if(!board.IsInCheck()){
                board.TrySkipTurn(); //CATCH in check
                output -= board.GetLegalMoves().Length;
                board.UndoSkipTurn();
            }
        }else{
            output -= board.GetLegalMoves().Length;
            if(!board.IsInCheck()){
                board.TrySkipTurn(); //CATCH in check
                output += board.GetLegalMoves().Length;
                board.UndoSkipTurn();
            }
        }

        return output;
    }
}