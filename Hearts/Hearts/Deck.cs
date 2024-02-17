using System.Collections;

namespace Hearts;

internal class Deck
{
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();
    
    private List<Card> _cards;

    internal Deck()
    {
        _cards = [];

        foreach (Suit suit in Enum.GetValues<Suit>())
        foreach (Rank rank in Enum.GetValues<Rank>())
            _cards.Add(new Card(suit, rank));
    }

    internal void Deal(List<Player> players)
    {
        _cards
           .Select((card, i) => new { card, idx = i % players.Count })
           .GroupBy(x => x.idx)
           .ToList()
           .ForEach(grp => players[grp.Key].DealHand(grp.Select(x => x.card)));
    }

    internal void Shuffle()
    {
        var random = new Random();
        _cards = [.. _cards.OrderBy(_ => random.Next())];
    }
}