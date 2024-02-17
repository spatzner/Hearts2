namespace Hearts;

internal class GameEndedEventHandlerArgs(List<Player> players)
{
    internal List<Player> Players => players;
}