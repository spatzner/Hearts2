namespace Hearts;

public class Round(List<Player> players, ITrickFactory trickFactory) : IRound
{
    public List<ITrick> Tricks { get; } = [];
    public ITrick? CurrentTrick => Tricks.LastOrDefault();
    public bool HeartsBroken { get; set; }

    public event EventHandler? RoundCompleted;
    public event EventHandler? TrickCompleted;
    public event ActionRequestedEventHandler? ActionRequested;

    public void StartTrick()
    {
        //Shouldn't matter in this case because flow should prevent event triggering on old tricks
        //And also that the Trick doesn't have a longer lifespan tha the Round
        //But it's a good practice to remove event handlers when they are no longer needed
        if (CurrentTrick != null)
        {
            CurrentTrick.ActionRequested -= OnActionRequested;
            CurrentTrick.TrickCompleted -= OnTrickCompleted;
        }

        ITrick trick = trickFactory.CreateTrick(GetPlayerOrder(), HeartsBroken);
        trick.ActionRequested += OnActionRequested;
        trick.TrickCompleted += OnTrickCompleted;

        Tricks.Add(trick);

        trick.StartTrick();
    }

    public void PlayCard(Player player, Card card)
    {
        if (CurrentTrick == null || CurrentTrick.TrickComplete)
            throw new InvalidOperationException("You cannot play a card when there is no active trick.");

        HeartsBroken = CurrentTrick.PlayCard(player, card);
    }

    protected virtual void OnRoundCompleted()
    {
        ScoreRound();
        RoundCompleted?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnActionRequested(object source, ActionRequestArgs args)
    {
        ActionRequested?.Invoke(source, args);
    }

    private List<Player> GetPlayerOrder()
    {
        Player start = (CurrentTrick == null ? players.First(p => p.HasRoundStartCard()) : CurrentTrick.Winner)
         ?? throw new InvalidOperationException();

        return players.SkipWhile(p => p != start).Concat(players.TakeWhile(p => p != start)).ToList();
    }

    private void OnTrickCompleted(object? sender, EventArgs args)
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
            GivePointsToNonWinners();
        else
            GivePlayerPoints();
    }

    private bool PlayerShotTheMoon()
    {
        var pointedTricks = Tricks.Where(t => t.Points > 0);
        return pointedTricks.All(x => x.Winner == pointedTricks.First().Winner);
    }

    private void GivePointsToNonWinners()
    {
        foreach (Player player in players.Where(p => p != Tricks.First().Winner))
            player.Score += Tricks.Sum(t => t.Points);
    }

    private void GivePlayerPoints()
    {
        foreach (ITrick trick in Tricks)
            trick.Winner!.Score += trick.Points;
    }
}