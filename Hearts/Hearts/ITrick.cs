namespace Hearts;

public interface ITrick
{
    Player? Winner { get; set; }
    bool TrickComplete { get; }
    SortedDictionary<Player, Card> Cards { get; }
    void StartTrick();
    bool PlayCard(Player player, Card card);
    int GetPoints();
    event ActionRequestedEventHandler? ActionRequested;
    event EventHandler? TrickCompleted;
}