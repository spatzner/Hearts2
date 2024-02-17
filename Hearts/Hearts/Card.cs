namespace Hearts;

internal record Card(Suit Suit, Rank Rank)
{
    internal Suit Suit { get; } = Suit;
    internal Rank Rank { get; } = Rank;

    public override string ToString()
    {
        return $"{Rank} of {Suit}";
    }
}