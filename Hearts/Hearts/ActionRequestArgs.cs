namespace Hearts;

public class ActionRequestArgs : EventArgs
{
    public required Player Player { get; init; }
    public required List<Card> CardsPlayed { get; init; }
    public required Suit? LeadingSuit { get; init; }
    public required List<Card> ValidCards { get; init; }
}