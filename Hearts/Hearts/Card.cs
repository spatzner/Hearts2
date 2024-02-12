namespace Hearts;

public record Card(Suit Suit, Rank Rank)
{
    public Suit Suit { get; } = Suit;
    public Rank Rank { get; } = Rank;
}