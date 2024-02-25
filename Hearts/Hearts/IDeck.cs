namespace Hearts;

public interface IDeck
{
    void DealShuffled(List<Player> players);
    Card StartingCard { get; }
}