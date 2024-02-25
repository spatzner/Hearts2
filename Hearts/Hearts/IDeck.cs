namespace Hearts;

public interface IDeck
{
    Card StartingCard { get; }
    void DealShuffled(List<Player> players);
}