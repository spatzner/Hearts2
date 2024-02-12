using System.Collections.ObjectModel;

namespace Hearts;

public class Round
{
    public Round(ReadOnlyCollection<Player> players)
    {
        _players = players;
        StartTrick();
    }
    
    internal List<Trick> Tricks { get; } = [];
    internal Trick CurrentTrick => Tricks.Last();
    internal bool HeartsBroken { get; set; }
    internal bool RoundComplete { get; private set; }

    internal event EventHandler? RoundCompleted;
    
    private readonly ReadOnlyCollection<Player> _players;

    internal void StartTrick()
    {
        var trick = new Trick(GetPlayerOrder());
        trick.TrickCompleted += OnTrickCompleted;

        Tricks.Add(trick);
    }

    private List<Player> GetPlayerOrder()
    {
        return _players
           .SkipWhile(p => p != CurrentTrick.Winner)
           .Concat(_players.TakeWhile(p => p != CurrentTrick.Winner))
           .ToList();
    }

    private void OnTrickCompleted(object? sender, EventArgs args)
    {
        if (_players.Any(p => p.Hand!.Count == 0))
        {
            RoundComplete = true;
            OnRoundCompleted();
        }
        else
            StartTrick();
    }
    protected virtual void OnRoundCompleted()
    {
        RoundCompleted?.Invoke(this, EventArgs.Empty);
    }
}