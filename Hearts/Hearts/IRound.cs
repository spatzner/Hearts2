using System.Collections.ObjectModel;

namespace Hearts;

public interface IRound
{
    ReadOnlyCollection<ITrick> Tricks { get; }
    bool HeartsBroken { get; set; }
    ITrick? CurrentTrick { get; }
    event EventHandler? TrickCompleted;
    event EventHandler? RoundCompleted;
    event ActionRequestedEventHandler? ActionRequested;
    void PlayCard(Player player, Card card);
    void StartRound();
}