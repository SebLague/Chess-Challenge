using ChessChallenge.Chess;
using Raylib_cs;
using System.Numerics;

namespace ChessChallenge.Application
{
    public class HumanPlayer
    {
        public event System.Action<Move>? MoveChosen;

        readonly Board board;
        readonly BoardUI boardUI;

        // State
        bool isDragging;
        int selectedSquare;
        bool isTurnToMove;


        public HumanPlayer(BoardUI boardUI)
        {
            board = new();
            board.LoadStartPosition();
            this.boardUI = boardUI;
        }

        public void NotifyTurnToMove()
        {
            isTurnToMove = true;
        }

        public void SetPosition(string fen)
        {
            board.LoadPosition(fen);
        }

        public void Update()
        {
            if (!isTurnToMove)
            {
                return;
            }
            Vector2 mouseScreenPos = Raylib.GetMousePosition();
            Vector2 mouseWorldPos = Program.ScreenToWorldPos(mouseScreenPos);

            if (LeftMousePressedThisFrame())
            {
                if (boardUI.TryGetSquareAtPoint(mouseWorldPos, out int square))
                {
                    int piece = board.Square[square];
                    if (PieceHelper.IsColour(piece, board.IsWhiteToMove ? PieceHelper.White : PieceHelper.Black))
                    {
                        isDragging = true;
                        selectedSquare = square;
                        boardUI.HighlightLegalMoves(board, square);
                    }
                }
            }

            if (isDragging)
            {
                if (LeftMouseReleasedThisFrame())
                {
                    CancelDrag();
                    if (boardUI.TryGetSquareAtPoint(mouseWorldPos, out int square))
                    {
                        TryMakeMove(selectedSquare, square);
                    }
                }
                else if (RightMousePressedThisFrame())
                {
                    CancelDrag();
                }
                else
                {
                    boardUI.DragPiece(selectedSquare, mouseWorldPos);
                }
            }
        }

        void CancelDrag()
        {
            isDragging = false;
            boardUI.ResetSquareColours(true);
        }

        void TryMakeMove(int startSquare, int targetSquare)
        {
            bool isLegal = false;
            Move move = Move.NullMove;

            MoveGenerator generator = new();
            var legalMoves = generator.GenerateMoves(board);
            foreach (var legalMove in legalMoves)
            {
                if (legalMove.StartSquareIndex == startSquare && legalMove.TargetSquareIndex == targetSquare)
                {
                    isLegal = true;
                    move = legalMove;
                    break;
                }
            }

            if (isLegal)
            {
                isTurnToMove = false;
                MoveChosen?.Invoke(move);
            }
        }

        static bool LeftMousePressedThisFrame() => Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
        static bool LeftMouseReleasedThisFrame() => Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT);
        static bool RightMousePressedThisFrame() => Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT);

    }
}