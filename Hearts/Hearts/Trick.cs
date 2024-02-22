using System.Collections.ObjectModel;

namespace Hearts;

public class Trick(List<Player> players, bool heartsBroken) : ITrick
{
    private Suit? LeadingSuit => Cards.FirstOrDefault().Value?.Suit;
    private ReadOnlyCollection<Player> PlayerOrder { get; } = players.AsReadOnly();
    public Player? CurrentPlayer => PlayerOrder.Skip(Cards.Count).FirstOrDefault();
    public SortedDictionary<Player, Card> Cards { get; } = [];
    public Player? Winner { get; private set; }
    public bool TrickComplete { get; private set; }
    public int Points => Cards.Select(x => x.Value).Sum(c => c.Points);

    public event ActionRequestedEventHandler? ActionRequested;
    public event EventHandler? TrickCompleted;

    public void StartTrick()
    {
        OnActionRequested(this, GetActionRequest());
    }

    public bool PlayCard(Player player, Card card)
    {
        if (CurrentPlayer != player)
            throw new InvalidOperationException("It is not your turn to play a card.");

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

    private ActionRequestArgs GetActionRequest()
    {
        return CurrentPlayer == null
            ? throw new InvalidOperationException("You cannot send an action request when there is no next player.")
            : new ActionRequestArgs
            {
                CardsPlayed = [.. Cards.Values],
                LeadingSuit = LeadingSuit,
                Player = CurrentPlayer,
                ValidCards = [.. GetValidCardsToPlay(CurrentPlayer).OrderBy(x => x.Suit).ThenBy(x => x.Rank)]
            };
    }

    private IEnumerable<Card> GetValidCardsToPlay(Player? player)
    {
        if (player == null)
            throw new InvalidOperationException("You cannot get valid cards to play when there is no next player.");

        if (Cards.Count == 0 && player.Hand.Count == 13)
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