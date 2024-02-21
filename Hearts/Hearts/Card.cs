namespace Hearts;

public record Card
{
    public Suit Suit { get; }
    public Rank Rank { get; }
    public int Points { get; }

    public Card(Suit suit, Rank rank)
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