using System.Collections.ObjectModel;

namespace Hearts;

internal class Round(List<Player> players)
{
    internal List<Trick> Tricks { get; } = [];
    internal Trick? CurrentTrick => Tricks.LastOrDefault();
    internal bool HeartsBroken { get; set; } = false;
    internal bool RoundComplete { get; private set; }

    internal event EventHandler? RoundCompleted;
    internal event ActionRequestedEventHandler? ActionRequested;

    internal void StartTrick()
    {
        var trick = new Trick(GetPlayerOrder(), HeartsBroken);
        trick.ActionRequested += OnActionRequested;
        trick.TrickCompleted += OnTrickCompleted;

        Tricks.Add(trick);

        trick.StartTrick();
    }

    private List<Player> GetPlayerOrder()
    {
        Player start = (CurrentTrick == null ? players.First(p => p.HasRoundStartCard()) : CurrentTrick.Winner)
         ?? throw new InvalidOperationException();

        return players.SkipWhile(p => p != start).Concat(players.TakeWhile(p => p != start)).ToList();
    }

    private void OnTrickCompleted(object? sender, EventArgs args)
    {
        if (players.Any(p => p.Hand!.Count == 0))
        {
            RoundComplete = true;
            OnRoundCompleted();
        }
        else
            StartTrick();
    }

    internal void PlayCard(Player player, Card card)
    {
        if (CurrentTrick == null || CurrentTrick.TrickComplete)
            throw new InvalidOperationException("You cannot play a card when there is no current trick.");

        HeartsBroken = CurrentTrick.PlayCard(player, card);
    }

    protected virtual void OnRoundCompleted()
    {
        ScoreRound();
        RoundCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void ScoreRound()
    {
        if (PlayerShotTheMoon())
            GivePointsToNonWinners();
        else
            GivePlayerPoints();
    }

    private void GivePlayerPoints()
    {
        foreach (Trick trick in Tricks)
            trick.Winner!.TakePoints(trick.GetPoints());
    }

    private void GivePointsToNonWinners()
    {
        foreach (Player player in players.Where(p => p != Tricks.First().Winner))
            player.TakePoints(Tricks.Sum(t => t.GetPoints()));
    }

    private bool PlayerShotTheMoon()
    {
        return Tricks.Where(t => t.GetPoints() > 0).All(x => x.Winner == Tricks.First().Winner);
    }

    protected virtual void OnActionRequested(object source, ActionRequestArgs args)
    {
        ActionRequested?.Invoke(source, args);
    }
}