namespace Hearts;

internal record Card
{
    internal Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
        
        switch (suit)
        {
            case Suit.Spades when rank == Rank.Queen:
                Points = 13;
                break;
            case Suit.Hearts:
                Points = 1;
                break;
            default:
                Points = 0;
                break;
        }
    }
    
    internal Suit Suit { get; } 
    internal Rank Rank { get; } 
    internal int Points { get; }

    public override string ToString()
    {
        return $"{Rank} of {Suit}";
    }
}