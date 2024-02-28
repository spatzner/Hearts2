namespace Hearts;

public class Deck : IDeck
{
    private readonly List<Card> _cardsToRemove =
    [
        new Card(Suit.Diamonds, Rank.Two),
        new Card(Suit.Clubs, Rank.Two),
        new Card(Suit.Diamonds, Rank.Three),
        new Card(Suit.Clubs, Rank.Three)
    ];

    private List<Card> _cards;

    public Card StartingCard { get; }

    public Deck(int playerCount)
    {
        _cards = [];

        foreach (Suit suit in Enum.GetValues<Suit>())
        foreach (Rank rank in Enum.GetValues<Rank>())
            _cards.Add(new Card(suit, rank));

        _cardsToRemove.Take(52 % playerCount).ToList().ForEach(c => _cards.Remove(c));

        StartingCard = _cards.Where(c => c.Suit == Suit.Clubs).MinBy(c => c.Rank)!;
    }

    public void DealShuffled(List<Player> players)
    {
        Shuffle();

        _cards
           .Select((card, i) => new { card, grp = i % players.Count })
           .GroupBy(x => x.grp)
           .ToList()
           .ForEach(grp => players[grp.Key].DealHand(grp.Select(x => x.card)));
    }

    private void Shuffle()
    {
        var random = new Random();
        _cards = [.. _cards.OrderBy(_ => random.Next())];
    }
}