namespace Hearts;

internal record Card
{
    internal Suit Suit { get; }
    internal Rank Rank { get; }
    internal int Points { get; }

    internal Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;

        Points = suit switch
        {
            Suit.Spades when rank == Rank.Queen => 13,
            Suit.Hearts => 1,
            _ => 0
        };
    }

    public override string ToString()
    {
        return $"{Rank} of {Suit}";
    }
}