﻿namespace Hearts;

public class Game(int pointsToEndGame, IRoundFactory roundFactory)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<IRound> Rounds { get; } = [];
    public IRound? CurrentRound => Rounds.LastOrDefault();
    public bool GameComplete { get; private set; }
    public IReadOnlyList<Player> Players => _players.AsReadOnly();

    private readonly List<Player> _players = [];
    private bool _gameStarted;

    public event ActionRequestedEventHandler? ActionRequested;
    public event EventHandler? GameCompleted;
    public event EventHandler? RoundCompleted;
    public event EventHandler? TrickCompleted;

    public void AddPlayer(Player player)
    {
        if (_players.Count == 8)
            throw new InvalidOperationException("A game of hearts can only have 8 players.");

        if (_gameStarted)
            throw new InvalidOperationException("You cannot add a player to a game that has already started.");

        _players.Add(player);
    }

    public void StartGame()
    {
        if (_gameStarted)
            throw new InvalidOperationException("The game has already started.");

        if (_players.Count < 3)
            throw new InvalidOperationException("A game of hearts must have at least 3 players.");

        _gameStarted = true;

        StartRound();
    }

    public void PlayCard(Player player, Card card)
    {
        if (CurrentRound == null)
            throw new InvalidOperationException("You cannot play a card when there is no current round.");

        CurrentRound.PlayCard(player, card);
    }

    private void StartRound()
    {
        //Shouldn't matter in this case because flow should prevent event triggering on old Rounds
        //And also that the Round doesn't have a longer lifespan tha the Game
        //But it's a good practice to remove event handlers when they are no longer needed
        if (CurrentRound != null)
        {
            CurrentRound.ActionRequested -= OnActionRequested;
            CurrentRound.RoundCompleted -= OnRoundCompleted;
            CurrentRound.TrickCompleted -= OnTrickCompleted;
        }

        IRound round = roundFactory.CreateRound(_players);
        round.ActionRequested += OnActionRequested;
        round.RoundCompleted += OnRoundCompleted;
        round.TrickCompleted += OnTrickCompleted;
        Rounds.Add(round);

        round.StartRound();
    }

    private void EndGame()
    {
        GameComplete = true;
        GameCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void OnTrickCompleted(object? sender, EventArgs e)
    {
        TrickCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void OnActionRequested(object source, ActionRequestArgs args)
    {
        ActionRequested?.Invoke(source, args);
    }

    private void OnRoundCompleted(object? sender, EventArgs args)
    {
        if (_players.Any(p => p.Score >= pointsToEndGame))
            EndGame();
        else
        {
            RoundCompleted?.Invoke(this, EventArgs.Empty);
            StartRound();
        }
    }
}