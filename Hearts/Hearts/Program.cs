// See https://aka.ms/new-console-template for more information

using Hearts;

ManualResetEvent actionRequested = new(false);
ManualResetEvent gameEnded = new(false);

Game game = new(100);

game.AddPlayer(new Player("Alice"));
game.AddPlayer(new Player("Bob"));
game.AddPlayer(new Player("Charlie"));
game.AddPlayer(new Player("David"));

game.ActionRequested += GameActionRequested;
game.GameCompleted += (_, _) => gameEnded.Set();


game.StartGame();

while (!gameEnded.WaitOne(0))
{
    actionRequested.WaitOne();
}

return;

void GameActionRequested(object source, ActionRequestArgs args)
{
    Dictionary<int, Card> cardMap =
        args.ValidCards.Select((card, i) => new { card, idx = i }).ToDictionary(x => x.idx, x => x.card);

    Console.WriteLine();
    Console.WriteLine($"{args.Player.Name} is being asked for a card.");
    Console.WriteLine($"They have:");
    Console.WriteLine(string.Join(Environment.NewLine,
        args.Player.Hand!.OrderBy(c => c.Suit).ThenBy(c => c.Rank).Select(c => $"\t{c}")));
    Console.WriteLine($"They can play:");
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