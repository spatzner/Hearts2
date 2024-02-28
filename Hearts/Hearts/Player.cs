namespace Hearts;

public class Player(string name) : IComparable
{
    public string Name { get; } = name;
    public Guid Id { get; } = Guid.NewGuid();
    public IReadOnlyCollection<Card> Hand => _handInternal.AsReadOnly();

    public int Score { get; set; }

    public int CompareTo(object? obj)
    {
        if (obj is Player otherPlayer)
            return Id.CompareTo(otherPlayer.Id);

        return 1;
    }

    public void DealHand(IEnumerable<Card> hand)
    {
        _handInternal.Clear();
        _handInternal.AddRange(hand);
    }

    public void PlayCard(Card card)
    {
        if (_handInternal == null || _handInternal.Count == 0)
            throw new InvalidOperationException("You cannot play a card when you have no cards.");

        if (_handInternal.All(c => c != card))
            throw new InvalidOperationException("You cannot play a card that you do not have.");

        _handInternal.Remove(card);
    }

    private readonly List<Card> _handInternal = [];
}