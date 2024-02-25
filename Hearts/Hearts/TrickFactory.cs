namespace Hearts;

public class TrickFactory : ITrickFactory
{
    public ITrick CreateTrick(List<Player> players)
    {
        return new Trick(players);
    }
}