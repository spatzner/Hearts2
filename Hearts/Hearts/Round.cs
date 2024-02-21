namespace Hearts;

internal class Round(List<Player> players, ITrickFactory trickFactory) : IRound
{
    public List<ITrick> Tricks { get; } = [];
    public ITrick? CurrentTrick => Tricks.LastOrDefault();
    public bool HeartsBroken { get; set; }

    public event EventHandler? RoundCompleted;
    public event EventHandler? TrickCompleted;
    public event ActionRequestedEventHandler? ActionRequested;

    public void StartTrick()
    {
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
        IEnumerable<ITrick> pointedTricks = Tricks.Where(t => t.GetPoints() > 0);
        return pointedTricks.All(x => x.Winner == pointedTricks.First().Winner); 
    }

    private void GivePointsToNonWinners()
    {
        foreach (Player player in players.Where(p => p != Tricks.First().Winner))
            player.TakePoints(Tricks.Sum(t => t.GetPoints()));
    }

    private void GivePlayerPoints()
    {
        foreach (ITrick trick in Tricks)
            trick.Winner!.TakePoints(trick.GetPoints());
    }
}