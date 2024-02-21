// See https://aka.ms/new-console-template for more information

using Hearts;

ManualResetEvent actionRequested = new(false);
ManualResetEvent gameEnded = new(false);

//TODO: Dependency injection
Game game = new(25, new RoundFactory(new TrickFactory()));

game.AddPlayer(new Player("Alice"));
game.AddPlayer(new Player("Bob"));
game.AddPlayer(new Player("Charlie"));
game.AddPlayer(new Player("David"));

//game.ActionRequested += GameActionRequested;
game.ActionRequested += GameActionReuquested_Automated;
game.TrickCompleted += GameOnTrickCompleted;
game.RoundCompleted += OnRoundCompleted;
game.GameCompleted += GameCompleted;

void GameOnTrickCompleted(object? sender, EventArgs e)
{
    Console.WriteLine();
}

void GameCompleted(object? sender, EventArgs args)
{
    Console.WriteLine();
    Console.WriteLine("Game Over");
    Console.WriteLine("Final Scores:");
    foreach (Player player in game.Players)
        Console.WriteLine($"{player.Name}: {player.Score}");
}

void OnRoundCompleted(object? sender, EventArgs eventArgs)
{
    Console.WriteLine();
    Console.WriteLine("Round completed");
    Console.WriteLine("Scores:");
    foreach (Player player in game.Players)
        Console.WriteLine($"{player.Name}: {player.Score}");
    Console.WriteLine();
}

game.GameCompleted += (_, _) => gameEnded.Set();

game.StartGame();

while (!gameEnded.WaitOne(0)) { }

Console.ReadKey();

return;

void GameActionReuquested_Automated(object? source, ActionRequestArgs args)
{
    Card cardToPlay = args.ValidCards.Skip(Random.Shared.Next(0, args.ValidCards.Count - 1)).First();

    Console.WriteLine($"{args.Player.Name}: {cardToPlay}");

    game.PlayCard(args.Player, cardToPlay);
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

    game.PlayCard(args.Player, cardMap[cardKey]);
}