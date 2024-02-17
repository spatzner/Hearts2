using System.Collections.ObjectModel;

namespace Hearts;

internal class Game
{
    internal Guid Id { get; set; }
    internal List<Round> Rounds { get; } = [];
    internal Round? CurrentRound => Rounds.LastOrDefault();
    internal bool GameComplete { get; private set; }

    private readonly List<Player> _players = [];

    private readonly Deck _deck = new();

    private bool _gameStarted;

    private readonly int _pointsToEndGame;

    internal event ActionRequestedEventHandler? ActionRequested;

    internal event GameEndedEventHandler? GameEnded;

    internal Game(int pointsToEndGame)
    {
        _pointsToEndGame = pointsToEndGame;
    }

    internal void AddPlayer(Player player)
    {
        if (_players.Count == 4)
            throw new InvalidOperationException("A game of hearts can only have 4 players.");

        if (_gameStarted)
            throw new InvalidOperationException("You cannot add a player to a game that has already started.");

        _players.Add(player);
    }

    internal void StartGame()
    {
        if (_gameStarted)
            throw new InvalidOperationException("The game has already started.");

        if (_players.Count != 4)
            throw new InvalidOperationException("A game of hearts must have exactly 4 players.");

        _gameStarted = true;

        StartRound();
    }

    private void StartRound()
    {
        var round = new Round(_players);
        round.ActionRequested += OnActionRequested;
        round.RoundCompleted += OnRoundCompleted;
        Rounds.Add(round);

        DealCards();

        round.StartTrick();
    }

    private void DealCards()
    {
        _deck.Shuffle();

        _deck.Deal(_players);
    }

    private void OnActionRequested(object source, ActionRequestArgs args)
    {
        ActionRequested?.Invoke(source, args);
    }

    private void OnRoundCompleted(object? sender, EventArgs args)
    {
        if (_players.Any(p => p.Score >= _pointsToEndGame))
            EndGame();
        else
            StartRound();
    }

    private void EndGame()
    {
        GameComplete = true;
        GameEnded?.Invoke(this, new GameEndedEventHandlerArgs([.. _players]));
    }

    internal void PlayCard(Player player, Card card)
    {
        if (CurrentRound == null)
            throw new InvalidOperationException("You cannot play a card when there is no current round.");

        CurrentRound.PlayCard(player, card);
    }
}