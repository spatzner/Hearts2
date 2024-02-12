using System.Collections.ObjectModel;

namespace Hearts;

internal class Trick
{
    internal Suit LeadingSuit { get; set; }
    internal Player LeadingPlayer => PlayerOrder.First();
    internal Player? NextPlayer => PlayerOrder.Skip(Cards.Count).FirstOrDefault();
    internal ReadOnlyCollection<Player> PlayerOrder { get; }
    internal Dictionary<Player, Card> Cards { get; } = [];
    internal Player? Winner { get; set; }
    internal int Points { get; set; }
    internal bool TrickComplete { get; private set; }

    internal event EventHandler? TrickCompleted;

    internal Trick(List<Player> players)
    {
        PlayerOrder = players.AsReadOnly();

        new Deck()
           .Cards.Select((card, i) => new { card, idx = i % PlayerOrder.Count })
           .GroupBy(x => x.idx)
           .ToList()
           .ForEach(grp => PlayerOrder[grp.Key].DealHand(grp.Select(x => x.card)));
    }

    public ActionRequestArgs GetActionRequest(bool heartsBroken)
    {
        if (NextPlayer == null)
            throw new InvalidOperationException("You cannot send an action request when there is no next player.");

        return new ActionRequestArgs
        {
            CardsPlayed = [.. Cards.Values],
            LeadingSuit = LeadingSuit,
            Player = NextPlayer,
            ValidActions = GetValidCardsToPlay(heartsBroken)
        };
    }

    protected virtual void OnTrickCompleted()
    {
        TrickComplete = true;
        
        Points = Cards
           .Select(x => x.Value)
           .Sum(c => c.Suit == Suit.Hearts ? 1 : c is { Rank: Rank.Queen, Suit: Suit.Spades } ? 13 : 0);

        Winner = Cards.MaxBy(x => x.Value.Suit == LeadingSuit ? (int)x.Value.Rank : -1).Key;

        Winner.TakeTrickPoints(this);

        TrickCompleted?.Invoke(this, EventArgs.Empty);
    }

    internal List<Card> GetValidCardsToPlay(bool heartsBroken)
    {
        if (Cards.Count == 0)
            return [.. NextPlayer!.Hand!];

        if (NextPlayer!.Hand!.Any(c => c.Suit == LeadingSuit))
            return NextPlayer!.Hand!.Where(c => c.Suit == LeadingSuit).ToList();

        return heartsBroken ? [.. NextPlayer!.Hand!] : NextPlayer!.Hand!.Where(c => c.Suit != Suit.Hearts).ToList();
    }

    internal void PlayCard(Player player, Card card, ref bool heartsBroken)
    {
        if (player.Hand?.Count == 0)
            throw new InvalidOperationException("You cannot play a card when you have no cards.");

        if (Cards.ContainsKey(player))
            throw new InvalidOperationException("You have already played a card in this trick.");

        if (card.Suit == Suit.Hearts && !heartsBroken && Cards.Count != 0 && player.Hand!.Any(c => c.Suit == LeadingSuit))
            throw new InvalidOperationException("You cannot play hearts until they have been broken.");

        if (card.Suit == Suit.Hearts || card is { Rank: Rank.Queen, Suit: Suit.Spades })
            heartsBroken = true;

        Cards[player] = card;

        if (Cards.Count == PlayerOrder.Count)
            OnTrickCompleted();
    }
}