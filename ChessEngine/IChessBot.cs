namespace ChessEngine
{
    public interface IChessBot
    {
        Move Think(Board board, Timer timer);
    }
}
