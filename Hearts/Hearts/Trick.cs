using System.Collections.ObjectModel;

namespace Hearts;

internal interface ITrick
{
    Suit LeadingSuit { get; set; }
    ReadOnlyCollection<Player> PlayerOrder { get; }
    Dictionary<Player, Card> Cards { get; }
    Player? Winner { get; set; }
    bool TrickComplete { get; }
    event ActionRequestedEventHandler? ActionRequested;
    event EventHandler? TrickCompleted;
    void StartTrick();
    Player? GetNextPlayer();
    bool PlayCard(Player player, Card card);
    int GetPoints();
}

internal class Trick(List<Player> players, bool heartsBroken) : ITrick
{
    public Suit LeadingSuit { get; set; }
    public ReadOnlyCollection<Player> PlayerOrder { get; } = players.AsReadOnly();
    public Dictionary<Player, Card> Cards { get; } = [];
    public Player? Winner { get; set; }
    public bool TrickComplete { get; private set; }

    public event ActionRequestedEventHandler? ActionRequested;
    public event EventHandler? TrickCompleted;

    public void StartTrick()
    {
        OnActionRequested(this, GetActionRequest());
    }

    public Player? GetNextPlayer()
    {
        return PlayerOrder.Skip(Cards.Count).FirstOrDefault();
    }

    public bool PlayCard(Player player, Card card)
    {
        if (PlayerOrder.Skip(Cards.Count).FirstOrDefault() != player)
            throw new InvalidOperationException("It is not your turn to play a card.");

        if (player.Hand?.Count == 0)
            throw new InvalidOperationException("You cannot play a card when you have no cards.");

        if (!GetValidCardsToPlay(player).Contains(card))
            throw new InvalidOperationException("You cannot play that card");

        if (card.Suit == Suit.Hearts)
            heartsBroken = true;

        Cards[player] = card;

        player.PlayCard(card);

        if (Cards.Count == PlayerOrder.Count)
            OnTrickCompleted();
        else
            OnActionRequested(this, GetActionRequest());

        return heartsBroken;
    }

    public int GetPoints()
    {
        return Cards.Select(x => x.Value).Sum(c => c.Points);
    }

    protected virtual void OnTrickCompleted()
    {
        TrickComplete = true;

        Winner = Cards.MaxBy(x => x.Value.Suit == LeadingSuit ? (int)x.Value.Rank : -1).Key;

        TrickCompleted?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnActionRequested(object source, ActionRequestArgs args)
    {
        ActionRequested?.Invoke(source, args);
    }

    private ActionRequestArgs GetActionRequest()
    {
        Player? nextPlayer = GetNextPlayer();

        return nextPlayer == null
            ? throw new InvalidOperationException("You cannot send an action request when there is no next player.")
            : new ActionRequestArgs
            {
                CardsPlayed = [.. Cards.Values],
                LeadingSuit = LeadingSuit,
                Player = nextPlayer,
                ValidCards = [.. GetValidCardsToPlay(nextPlayer).OrderBy(x => x.Suit).ThenBy(x => x.Rank)]
            };
    }

    private IEnumerable<Card> GetValidCardsToPlay(Player? player)
    {
        if (player == null)
            throw new InvalidOperationException("You cannot get valid cards to play when there is no next player.");

        if (Cards.Count == 0 && player.HasFullHand())
            return player.Hand.Where(c => c is { Rank: Rank.Two, Suit: Suit.Clubs });

        if (player.Hand.All(c => c.Suit == Suit.Hearts))
            return player.Hand;

        if (Cards.Count == 0)
            return player.Hand.Where(c => heartsBroken || c.Suit != Suit.Hearts);

        if (player.Hand.Any(c => c.Suit == LeadingSuit))
            return player.Hand.Where(c => c.Suit == LeadingSuit);

        if (heartsBroken)
            return player.Hand;

        return player.Hand.Where(c => c.Suit != Suit.Hearts);
    }
}