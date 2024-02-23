using Microsoft.Extensions.Hosting;

namespace Hearts;

public class GameService : IHostedService
{
    readonly ManualResetEvent _gameEnded = new(false);
    private readonly Game _game;

    public GameService(Game game)
    {
        _game = game;

        //TODO: move to console prompts
        _game.AddPlayer(new Player("Alice"));
        _game.AddPlayer(new Player("Bob"));
        _game.AddPlayer(new Player("Charlie"));
        _game.AddPlayer(new Player("David"));

        _game.ActionRequested += GameActionRequested_Automated;
        _game.TrickCompleted += GameOnTrickCompleted;
        _game.RoundCompleted += OnRoundCompleted;
        _game.GameCompleted += GameCompleted;

        _game.GameCompleted += (_, _) => _gameEnded.Set();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => _game.StartGame(), cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    void GameActionRequested_Automated(object? source, ActionRequestArgs args)
    {
        Card cardToPlay = args.ValidCards.Skip(Random.Shared.Next(0, args.ValidCards.Count - 1)).First();

        Console.WriteLine($"{args.Player.Name}: {cardToPlay}");

        _game.PlayCard(args.Player, cardToPlay);
    }

    void GameActionRequested(object? source, ActionRequestArgs args)
    {
        var cardMap = args.ValidCards.Select((card, i) => new { card, idx = i }).ToDictionary(x => x.idx, x => x.card);

        Console.WriteLine();
        Console.WriteLine($"{args.Player.Name} is being asked for a card.");
        Console.WriteLine("They have:");
        Console.WriteLine(string.Join(Environment.NewLine,
            args.Player.Hand.OrderBy(c => c.Suit).ThenBy(c => c.Rank).Select(c => $"\t{c}")));
        Console.WriteLine("They can play:");
        Console.WriteLine(string.Join(Environment.NewLine, cardMap.Select(cm => $"\t{cm.Key} = {cm.Value}")));
        Console.WriteLine();
        Console.WriteLine("Please input card to play");

        var input = Console.ReadLine();

        int cardKey;

        while (!int.TryParse(input, out cardKey) || !cardMap.ContainsKey(cardKey))
        {
            Console.WriteLine("Please input a valid card to play");
            input = Console.ReadLine();
        }

        _game.PlayCard(args.Player, cardMap[cardKey]);
    }

    void GameOnTrickCompleted(object? sender, EventArgs e)
    {
        Console.WriteLine($"Winner: {_game.CurrentRound!.CurrentTrick!.Winner!.Name}");
        Console.WriteLine();
    }

    void GameCompleted(object? sender, EventArgs args)
    {
        Console.WriteLine($"Winner: {_game.CurrentRound!.CurrentTrick!.Winner!.Name}");
        Console.WriteLine();
        Console.WriteLine("Game Over");
        Console.WriteLine("Final Scores:");
        foreach (Player player in _game.Players.OrderBy(p => p.Score))
            Console.WriteLine($"{player.Name}: {player.Score}");
    }

    void OnRoundCompleted(object? sender, EventArgs eventArgs)
    {
        Console.WriteLine($"Winner: {_game.CurrentRound!.CurrentTrick!.Winner!.Name}");
        Console.WriteLine();
        Console.WriteLine("Round completed");
        Console.WriteLine("Scores:");
        foreach (Player player in _game.Players)
            Console.WriteLine($"{player.Name}: {player.Score}");
        Console.WriteLine();
    }
}