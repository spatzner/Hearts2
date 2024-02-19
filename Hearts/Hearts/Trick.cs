using System.Collections.ObjectModel;

namespace Hearts;

internal class Trick(List<Player> players, bool heartsBroken)
{
    internal Suit LeadingSuit { get; set; }
    internal ReadOnlyCollection<Player> PlayerOrder { get; } = players.AsReadOnly();
    internal Dictionary<Player, Card> Cards { get; } = [];
    internal Player? Winner { get; set; }
    internal bool TrickComplete { get; private set; }

    internal event ActionRequestedEventHandler? ActionRequested;
    internal event EventHandler? TrickCompleted;

    internal void StartTrick()
    {
        OnActionRequested(this, GetActionRequest());
    }

    private ActionRequestArgs GetActionRequest()
    {
        var nextPlayer = GetNextPlayer();

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

    internal Player? GetNextPlayer()
    {
        return PlayerOrder.Skip(Cards.Count).FirstOrDefault();
    }

    internal bool PlayCard(Player player, Card card)
    {
        if (player.Hand?.Count == 0)
            throw new InvalidOperationException("You cannot play a card when you have no cards.");

        if (Cards.ContainsKey(player))
            throw new InvalidOperationException("You have already played a card in this trick.");

        if(!GetValidCardsToPlay(player).Contains(card))
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

    internal int GetPoints()
    {
        return Cards.Select(x => x.Value).Sum(c => c.Points);
    }

    private IEnumerable<Card> GetValidCardsToPlay(Player? player)
    {
        if(player == null) throw new InvalidOperationException("You cannot get valid cards to play when there is no next player.");

        if (Cards.Count == 0 && player.HasFullHand())
            return player.Hand!.Where(c => c is { Rank: Rank.Two, Suit: Suit.Clubs });
        
        if(player.Hand!.All(c => c.Suit == Suit.Hearts))
            return player.Hand!;

        if (Cards.Count == 0)
            return player.Hand!.Where(c => heartsBroken || c.Suit != Suit.Hearts);

        if (player.Hand!.Any(c => c.Suit == LeadingSuit))
            return player.Hand!.Where(c => c.Suit == LeadingSuit);

        if (heartsBroken)
            return player.Hand!;

        return player.Hand!.Where(c => c.Suit != Suit.Hearts);
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
}