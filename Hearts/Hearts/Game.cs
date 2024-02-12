using System.Collections.ObjectModel;

namespace Hearts;

public class Game
{
    public List<Round> Rounds { get; } = [];
    internal Round? CurrentRound => Rounds.LastOrDefault();
    internal ReadOnlyCollection<Player> Players { get; }

    public event ActionRequestedEventHandler? ActionRequested;

    public delegate void ActionRequestedEventHandler(object source, ActionRequestArgs args);

    public bool GameComplete { get; private set; }

    public event GameEndedEventHandler? GameEnded;


    private readonly int _pointsToEndGame;
    
    internal Game(List<Player> players, int pointsToEndGame)
    {
        Players = players.AsReadOnly();
        _pointsToEndGame = pointsToEndGame;
    }

    internal void StartGame()
    {
        if (Players.Count != 4)
            throw new InvalidOperationException("A game of hearts must have exactly 4 players.");

        StartRound();
    }

    internal ActionRequestArgs GetActionRequest()
    {
        if (CurrentRound?.CurrentTrick == null)
            throw new InvalidOperationException("You cannot send an action request when there is no current trick.");

        if (CurrentRound.CurrentTrick.NextPlayer == null)
            throw new InvalidOperationException("You cannot send an action request when there is no next player.");

        return CurrentRound.CurrentTrick.GetActionRequest(CurrentRound.HeartsBroken);
    }

    private void StartRound()
    {
        var round = new Round(Players);
        round.RoundCompleted += OnRoundCompleted;

        Rounds.Add(round);
    }

    private void OnRoundCompleted(object? sender, EventArgs args)
    {
        if (Players.Any(p => p.Score >= _pointsToEndGame))
            EndGame();
        else
            StartRound();
    }

    private void EndGame()
    {
        GameComplete = true;
        GameEnded?.Invoke(this, new GameEndedEventHandlerArgs([.. Players]));
    }

    protected virtual void OnActionRequested()
    {
        ActionRequested?.Invoke(this, GetActionRequest());
    }
}

public delegate void GameEndedEventHandler(object sender, GameEndedEventHandlerArgs args);

public class GameEndedEventHandlerArgs(List<Player> players)
{
    public List<Player> Players => players;
}

public class ActionRequestArgs : EventArgs
{
    public required Player Player { get; init; }
    public required List<Card> CardsPlayed { get; init; }
    public required Suit LeadingSuit { get; init; }
    public required List<Card> ValidActions { get; init; }
}