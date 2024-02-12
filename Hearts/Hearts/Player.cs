namespace Hearts;

public class Player(int id, string name)
{
    internal string Name { get; } = name;
    internal int Id { get; } = id;
    internal IReadOnlyCollection<Card>? Hand => HandInternal?.AsReadOnly();

    private List<Card>? HandInternal { get; set; }

    internal int Score { get; private set; }

    internal void DealHand(IEnumerable<Card> hand)
    {
        HandInternal = hand.ToList();
    }

    internal void TakeTrickPoints(Trick trick)
    {
        Score += trick
           .Cards.Select(x => x.Value)
           .Sum(c => c.Suit == Suit.Hearts ? 1 : c is { Rank: Rank.Queen, Suit: Suit.Spades } ? 13 : 0);
    }
}