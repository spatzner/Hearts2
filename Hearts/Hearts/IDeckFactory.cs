namespace Hearts
{
    public interface IDeckFactory
    {
        IDeck CreateDeck(int playerCount);
    }
}