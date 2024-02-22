namespace Hearts;

public record Card(Suit Suit, Rank Rank)
{
    public int Points { get; } = Suit switch
    {
        Suit.Spades when Rank == Rank.Queen => 13,
        Suit.Hearts => 1,
        _ => 0
    };

    public override string ToString()
    {
        return $"{Rank} of {Suit}";
    }
}