using Microsoft.Extensions.Hosting;

namespace Hearts;

public class GameService : IHostedService
{
    private readonly Game _game;

    public GameService(Game game)
    {
        _game = game;

        _game.ActionRequested += GameActionRequested_Automated;
        _game.TrickCompleted += GameOnTrickCompleted;
        _game.RoundCompleted += OnRoundCompleted;
        _game.GameCompleted += GameCompleted;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(PromptInitialization, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task PromptInitialization()
    {
        await Task.Delay(100);

        Console.WriteLine("Welcome to Hearts!");
        Console.WriteLine("Please enter the names of the players. Type -Start to begin");

        while (true)
        {
            var input = Console.ReadLine();

            if (input == "-Start")
                break;

            if (string.IsNullOrWhiteSpace(input))
                continue;

            _game.AddPlayer(new Player(input));
            Console.WriteLine($"Player {input} added");
        }

        _game.StartGame();
    }

    private void GameActionRequested_Automated(object? source, ActionRequestArgs args)
    {
        Card cardToPlay = args.ValidCards.Skip(Random.Shared.Next(0, args.ValidCards.Count - 1)).First();

        Console.WriteLine($"{args.Player.Name}: {cardToPlay}");

        _game.PlayCard(args.Player, cardToPlay);
    }

    private void GameActionRequested(object? source, ActionRequestArgs args)
    {
        var cardMap = args.ValidCards.Select((card, i) => new { card, idx = i }).ToDictionary(x => x.idx, x => x.card);

        Console.WriteLine();
        Console.WriteLine($"{args.Player.Name} is being asked for a card.");
        Console.WriteLine("They have:");
        Console.WriteLine(string.Join("\n",
            args.Player.Hand.OrderBy(c => c.Suit).ThenBy(c => c.Rank).Select(c => $"\t{c}")));
        Console.WriteLine("They can play:");
        Console.WriteLine(string.Join(string.Empty, cardMap.Select(cm => $"\t{cm.Key} = {cm.Value}\n")));
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

    private void GameOnTrickCompleted(object? sender, EventArgs e)
    {
        Console.WriteLine($"Winner: {_game.CurrentRound!.CurrentTrick!.Winner!.Name}\n");
    }

    private void GameCompleted(object? sender, EventArgs args)
    {
        Console.WriteLine($"Winner: {_game.CurrentRound!.CurrentTrick!.Winner!.Name}\n");
        Console.WriteLine("Game Over");
        Console.WriteLine("Final Scores:");
        foreach (Player player in _game.Players.OrderBy(p => p.Score))
            Console.WriteLine($"{player.Name}: {player.Score}");
    }

    private void OnRoundCompleted(object? sender, EventArgs eventArgs)
    {
        Console.WriteLine($"Winner: {_game.CurrentRound!.CurrentTrick!.Winner!.Name}\n");
        Console.WriteLine("Round completed");
        Console.WriteLine("Scores:");
        foreach (Player player in _game.Players)
            Console.WriteLine($"{player.Name}: {player.Score}");
        Console.WriteLine();
    }
}