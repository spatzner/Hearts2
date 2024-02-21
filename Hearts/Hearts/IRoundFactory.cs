namespace Hearts;

public interface IRoundFactory
{
    IRound CreateRound(List<Player> players);
}