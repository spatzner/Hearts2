namespace Hearts;

internal class Player(string name) : IComparable
{
    internal string Name { get; } = name;
    internal Guid Id { get; } = Guid.NewGuid();
    internal IReadOnlyCollection<Card> Hand => HandInternal.AsReadOnly();

    private List<Card> HandInternal { get; set; } = [];

    internal int Score { get; private set; }

    internal void DealHand(IEnumerable<Card> hand)
    {
        HandInternal = hand.ToList();
    }

    internal void PlayCard(Card card)
    {
        if (HandInternal == null || HandInternal.Count == 0)
            throw new InvalidOperationException("You cannot play a card when you have no cards.");

        if (HandInternal.All(c => c != card))
            throw new InvalidOperationException("You cannot play a card that you do not have.");

        HandInternal.Remove(card);
    }

    internal bool HasFullHand()
    {
        return Hand?.Count == 13;
    }

    internal bool HasRoundStartCard()
    {
        return Hand?.Any(c => c is { Rank: Rank.Two, Suit: Suit.Clubs }) ?? false;
    }

    internal void TakePoints(int points)
    {
        Score += points;
    }

    public int CompareTo(object? obj)
    {
        if(obj is Player otherPlayer)
            return Id.CompareTo(otherPlayer.Id);

        return 1;
    }
}