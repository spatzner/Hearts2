namespace Hearts;

public class DeckFactory : IDeckFactory
{
    public IDeck CreateDeck(int playerCount)
    {
        return new Deck(playerCount);
    }
}