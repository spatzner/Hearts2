namespace Hearts;

public interface ITrick
{
    Player? Winner { get; }
    bool TrickComplete { get; }
    SortedDictionary<Player, Card> Cards { get; }
    int Points { get; }
    void StartTrick();
    bool PlayCard(Player player, Card card);
    event ActionRequestedEventHandler? ActionRequested;
    event EventHandler? TrickCompleted;
}