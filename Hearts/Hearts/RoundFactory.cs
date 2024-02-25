namespace Hearts;

public class RoundFactory(ITrickFactory trickFactory, IDeckFactory deckFactory) : IRoundFactory
{
    public IRound CreateRound(List<Player> players)
    {
        return new Round(players, trickFactory, deckFactory);
    }
}