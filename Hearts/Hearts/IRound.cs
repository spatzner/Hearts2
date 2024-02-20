namespace Hearts;

internal interface IRound
{
    List<ITrick> Tricks { get; }
    ITrick? CurrentTrick { get; }
    bool HeartsBroken { get; set; }
    event EventHandler? RoundCompleted;
    event ActionRequestedEventHandler? ActionRequested;
    void StartTrick();
    void PlayCard(Player player, Card card);
}