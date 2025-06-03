using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using StarWars.Services.Interfaces;

public class StartupService : IHostedService
{
    private readonly ILogger<StartupService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public StartupService(ILogger<StartupService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

            async Task<bool> HasDataAsync(string collectionName)
            {
                try
                {
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var count = await collection.CountDocumentsAsync(new BsonDocument(), new CountOptions { Limit = 1 });
                    return count > 0;
                }
                catch
                {
                    return false;
                }
            }

            if (!await HasDataAsync("people"))
            {
                var peopleService = scope.ServiceProvider.GetRequiredService<IPeopleService>();
                peopleService.GetAllWarmup();
            }

            if (!await HasDataAsync("movies"))
            {
                var movieService = scope.ServiceProvider.GetRequiredService<IMovieService>();
                movieService.GetAllMoviesWarmup();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o carregamento inicial de dados");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando serviço de inicialização...");
        return Task.CompletedTask;
    }
}