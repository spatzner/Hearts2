namespace Hearts;

internal class TrickFactory : ITrickFactory
{
    public ITrick CreateTrick(List<Player> players, bool heartsBroken)
    {
        return new Trick(players, heartsBroken);
    }
}