using System.Collections.ObjectModel;

namespace Hearts;

public class Round(List<Player> players, ITrickFactory trickFactory, IDeckFactory deckFactory) : IRound
{
    private readonly IDeck _deck = deckFactory.CreateDeck(players.Count);
    private readonly List<ITrick> _tricks = [];
    
    public event EventHandler? RoundCompleted;
    public event EventHandler? TrickCompleted;
    public event ActionRequestedEventHandler? ActionRequested;

    public ReadOnlyCollection<ITrick> Tricks => _tricks.AsReadOnly();
    public ITrick? CurrentTrick => _tricks.LastOrDefault();
    public bool HeartsBroken { get; set; }

    public void StartRound()
    {
        _deck.DealShuffled(players);
        StartTrick();
    }

    public void PlayCard(Player player, Card card)
    {
        if (CurrentTrick == null || CurrentTrick.TrickComplete)
            throw new InvalidOperationException("You cannot play a card when there is no active trick.");

        if (!GetValidCardsToPlay().Contains(card))
            throw new InvalidOperationException("You cannot play that card");

        CurrentTrick.PlayCard(player, card);

        if (card is { Suit: Suit.Hearts })
            HeartsBroken = true;

        if (CurrentTrick.TrickComplete)
            CompleteTrick();
        else
            ActionRequested?.Invoke(this, GetActionRequest());
    }

    protected virtual void OnRoundCompleted()
    {
        ScoreRound();
        RoundCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void StartTrick()
    {
        ITrick trick = trickFactory.CreateTrick(GetPlayerOrder());

        _tricks.Add(trick);

        ActionRequested?.Invoke(this, GetActionRequest());
    }

    private ActionRequestArgs GetActionRequest()
    {
        return CurrentTrick!.CurrentPlayer == null
            ? throw new InvalidOperationException("You cannot send an action request when there is no next player.")
            : new ActionRequestArgs
            {
                CardsPlayed = [.. CurrentTrick.Cards.Values],
                LeadingSuit = CurrentTrick!.LeadingSuit,
                Player = CurrentTrick!.CurrentPlayer,
                ValidCards = [.. GetValidCardsToPlay().OrderBy(x => x.Suit).ThenBy(x => x.Rank)]
            };
    }

    private IEnumerable<Card> GetValidCardsToPlay()
    {
        Player player = CurrentTrick?.CurrentPlayer
         ?? throw new InvalidOperationException("You cannot get valid cards to play when there is no next player.");

        if (Tricks.Count == 1 && CurrentTrick!.Cards.Count == 0)
            return [_deck.StartingCard];

        if (player.Hand.All(c => c.Suit == Suit.Hearts))
            return player.Hand;

        if (CurrentTrick!.Cards.Count == 0)
            return player.Hand.Where(c => HeartsBroken || c.Suit != Suit.Hearts);

        if (player.Hand.Any(c => c.Suit == CurrentTrick.LeadingSuit))
            return player.Hand.Where(c => c.Suit == CurrentTrick.LeadingSuit);

        if (HeartsBroken)
            return player.Hand;

        return player.Hand.Where(c => c.Suit != Suit.Hearts);
    }

    private List<Player> GetPlayerOrder()
    {
        Player start =
            (CurrentTrick == null ? players.First(p => p.Hand.Contains(_deck.StartingCard)) : CurrentTrick.Winner)
         ?? throw new InvalidOperationException();

        return players.SkipWhile(p => p != start).Concat(players.TakeWhile(p => p != start)).ToList();
    }

    private void CompleteTrick()
    {
        if (players.Any(p => p.Hand.Count == 0))
            OnRoundCompleted();
        else
        {
            TrickCompleted?.Invoke(this, EventArgs.Empty);
            StartTrick();
        }
    }

    private void ScoreRound()
    {
        if (PlayerShotTheMoon())
        {
            foreach (Player player in players.Where(p => p != _tricks.First().Winner))
                player.Score += _tricks.Sum(t => t.Points);
        }
        else
        {
            foreach (ITrick trick in _tricks)
                trick.Winner!.Score += trick.Points;
        }
    }

    private bool PlayerShotTheMoon()
    {
        var pointedTricks = _tricks.Where(t => t.Points > 0).ToList();
        return pointedTricks.All(x => x.Winner == pointedTricks.First().Winner);
    }
}