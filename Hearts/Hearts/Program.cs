// See https://aka.ms/new-console-template for more information

using Hearts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host
   .CreateDefaultBuilder(args)
   .ConfigureServices((_, services) =>
    {
        services.AddScoped<IDeck, Deck>();
        services.AddScoped<ITrickFactory, TrickFactory>();
        services.AddScoped<IRoundFactory, RoundFactory>();
        services.AddScoped<Game>(provider =>
        {
            int pointsToEndGame = 25;
            var roundFactory = provider.GetRequiredService<IRoundFactory>();
            var deck = provider.GetRequiredService<IDeck>();
            return new Game(pointsToEndGame, roundFactory, deck);
        });
        services.AddHostedService<GameService>();
    });

builder.Build().Run();