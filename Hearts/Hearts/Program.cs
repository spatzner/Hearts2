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
game.GameEnded += (_, _) => gameEnded.Set();

game.StartGame();

while (!gameEnded.WaitOne(0))
{
    actionRequested.WaitOne();
}

return;

void GameActionRequested(object source, ActionRequestArgs args)
{
    Dictionary<int, Card> cardMap = args.ValidActions.Select((card, i) => new { card, idx = i }).ToDictionary(x => x.idx, x => x.card);

    Console.WriteLine($"{args.Player.Name} is being asked for a card.");
    Console.WriteLine($"They have: {string.Join(", ", args.Player.Hand!)}");
    Console.WriteLine($"They can play: {string.Join(", ", cardMap.Select(cm => $"{cm.Key} = {cm.Value}"))}");
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