using System.Collections.ObjectModel;

namespace Hearts;

public class Trick(List<Player> players) : ITrick
{
    public Player? CurrentPlayer => _playerOrder.Skip(Cards.Count).FirstOrDefault();
    private readonly ReadOnlyCollection<Player> _playerOrder = players.AsReadOnly();

    public Suit? LeadingSuit { get; private set; }
    public SortedDictionary<Player, Card> Cards { get; } = [];
    public Player? Winner { get; private set; }
    public bool TrickComplete { get; private set; }
    public int Points => Cards.Select(x => x.Value).Sum(c => c.Points);

    public void PlayCard(Player player, Card card)
    {
        if (CurrentPlayer != player)
            throw new InvalidOperationException("It is not your turn to play a card.");

        if (Cards.Count == 0)
            LeadingSuit = card.Suit;

        Cards[player] = card;

        player.PlayCard(card);

        if (Cards.Count != _playerOrder.Count)
            return;
        
        TrickComplete = true;
        Winner = Cards.MaxBy(x => x.Value.Suit == LeadingSuit ? (int)x.Value.Rank : -1).Key;
    }
        
}