namespace Hearts;

public interface ITrickFactory
{
    public ITrick CreateTrick(List<Player> players);
}