namespace Hearts;

internal interface IRoundFactory
{
    IRound CreateRound(List<Player> players);
}