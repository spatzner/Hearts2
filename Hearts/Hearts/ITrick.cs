namespace Hearts;

public interface ITrick
{
    Player? Winner { get; }
    bool TrickComplete { get; }
    SortedDictionary<Player, Card> Cards { get; }
    int Points { get; }
    Suit? LeadingSuit { get; }
    Player? CurrentPlayer { get; }
    void PlayCard(Player player, Card card);
}