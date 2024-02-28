// See https://aka.ms/new-console-template for more information

using Hearts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHostBuilder builder = Host
   .CreateDefaultBuilder(args)
   .ConfigureServices((_, services) =>
    {
        services.AddScoped<IDeckFactory, DeckFactory>();
        services.AddScoped<ITrickFactory, TrickFactory>();
        services.AddScoped<IRoundFactory, RoundFactory>();
        services.AddScoped<Game>(provider =>
        {
            var pointsToEndGame = 25;
            var roundFactory = provider.GetRequiredService<IRoundFactory>();
            return new Game(pointsToEndGame, roundFactory);
        });
        services.AddHostedService<GameService>();
    });

builder.Build().Run();