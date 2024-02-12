namespace Hearts;

internal class Deck
{
    internal List<Card> Cards { get; private set; }

    internal Deck()
    {
        Cards = [];

        foreach (Suit suit in Enum.GetValues<Suit>())
        foreach (Rank rank in Enum.GetValues<Rank>())
            Cards.Add(new Card(suit, rank));

        Shuffle();
    }

    internal void Shuffle()
    {
        var random = new Random();
        Cards = [.. Cards.OrderBy(_ => random.Next())];
    }
}