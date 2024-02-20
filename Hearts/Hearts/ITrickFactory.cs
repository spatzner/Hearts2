namespace Hearts;

internal interface ITrickFactory
{
    public ITrick CreateTrick(List<Player> players, bool heartsBroken);
}