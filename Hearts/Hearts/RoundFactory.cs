namespace Hearts;

public class RoundFactory(ITrickFactory trickFactory) : IRoundFactory
{
    public IRound CreateRound(List<Player> players)
    {
        return new Round(players, trickFactory);
    }
}