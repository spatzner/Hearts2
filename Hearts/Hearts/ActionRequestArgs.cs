namespace Hearts;

internal class ActionRequestArgs : EventArgs
{
    internal required Player Player { get; init; }
    internal required List<Card> CardsPlayed { get; init; }
    internal required Suit? LeadingSuit { get; init; }
    internal required List<Card> ValidCards { get; init; }
}