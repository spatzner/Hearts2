namespace Hearts;

public class Deck
{
    private List<Card> _cards;

    public Deck()
    {
        _cards = [];

        foreach (Suit suit in Enum.GetValues<Suit>())
        foreach (Rank rank in Enum.GetValues<Rank>())
            _cards.Add(new Card(suit, rank));
    }

    public void DealShuffled(List<Player> players)
    {
        Shuffle();

        _cards
           .Select((card, i) => new { card, idx = i % players.Count })
           .GroupBy(x => x.idx)
           .ToList()
           .ForEach(grp => players[grp.Key].DealHand(grp.Select(x => x.card)));
    }

    private void Shuffle()
    {
        var random = new Random();
        _cards = [.. _cards.OrderBy(_ => random.Next())];
    }
}