namespace Hearts;

public class Player(string name) : IComparable
{
    public string Name { get; } = name;
    public Guid Id { get; } = Guid.NewGuid();
    public IReadOnlyCollection<Card> Hand => HandInternal.AsReadOnly();

    private List<Card> HandInternal { get; set; } = [];

    public int Score { get; private set; }

    public int CompareTo(object? obj)
    {
        if (obj is Player otherPlayer)
            return Id.CompareTo(otherPlayer.Id);

        return 1;
    }

    public void DealHand(IEnumerable<Card> hand)
    {
        HandInternal = hand.ToList();
    }

    public void PlayCard(Card card)
    {
        if (HandInternal == null || HandInternal.Count == 0)
            throw new InvalidOperationException("You cannot play a card when you have no cards.");

        if (HandInternal.All(c => c != card))
            throw new InvalidOperationException("You cannot play a card that you do not have.");

        HandInternal.Remove(card);
    }

    public bool HasFullHand()
    {
        return Hand?.Count == 13;
    }

    public bool HasRoundStartCard()
    {
        return Hand?.Any(c => c is { Rank: Rank.Two, Suit: Suit.Clubs }) ?? false;
    }

    public void TakePoints(int points)
    {
        Score += points;
    }
}