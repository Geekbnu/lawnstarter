using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Prometheus;
using StarWars.Infraestructure.Background;
using StarWars.Infraestructure.Interfaces;
using StarWars.Infraestructure.Repository;
using StarWars.Infraestructure.Services;
using StarWars.Infraestucture.Interfaces;
using StarWars.Services.Interfaces;
using StarWarsApi.Data;
using StarWarsApi.Metric;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ConfigureCors(builder);
        ConfigureMongoDB(builder);
        ConfigureServices(builder);
        ConfigureSwagger(builder);
        ConfigureLogging(builder);

        var app = builder.Build();

        ConfigureMiddleware(app);
        ConfigurePipeline(app);

        app.Run();
    }

    private static void ConfigureCors(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options => options
            .AddPolicy("AllowAll", policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
    }

    private static void ConfigureMongoDB(WebApplicationBuilder builder)
    {
        string mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB")
            ?? "mongodb://lawnstarter:lawnstarter@localhost:27017/admin";

        string mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "admin";

        builder.Services.AddSingleton<IMongoClient>(provider =>
        {
            try
            {
                var settings = CreateMongoClientSettings(builder, mongoConnectionString);
                var mongoClient = new MongoClient(settings);

                return mongoClient;
            }
            catch (Exception ex)
            {
                throw;
            }
        });

        builder.Services.AddScoped(provider =>
            provider.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));
    }

    private static MongoClientSettings CreateMongoClientSettings(WebApplicationBuilder builder, string connectionString)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);

        string username = builder.Configuration["MongoDB:Username"];
        string password = builder.Configuration["MongoDB:Password"];
        string databaseName = builder.Configuration["MongoDB:AuthDatabase"] ?? "admin";
        string authMechanism = builder.Configuration["MongoDB:AuthMechanism"];

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            MongoCredential credential = authMechanism?.ToUpper() == "PLAIN"
                ? MongoCredential.CreatePlainCredential(databaseName, username, password)
                : MongoCredential.CreateCredential(databaseName, username, password);

            settings.Credential = credential;
        }

        settings.MaxConnectionPoolSize = 100;
        settings.ConnectTimeout = TimeSpan.FromSeconds(30.0);
        settings.SocketTimeout = TimeSpan.FromSeconds(30.0);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30.0);

        return settings;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<MovieMetricsService>();
        builder.Services.AddScoped<IMongoContext, MongoContext>();
        builder.Services.AddScoped<IPeopleRepository, PeopleRepository>();
        builder.Services.AddScoped<IMovieRepository, MovieRepository>();
        builder.Services.AddSingleton<IMessageQueue, ConcurrentMessageQueue>();
        builder.Services.AddSingleton<MessageQueueService>();
        builder.Services.AddHostedService<MessageProcessingBackgroundService>();
        builder.Services.AddSingleton<IMessageQueue, ConcurrentMessageQueue>();
        builder.Services.AddSingleton<MessageQueueService>();
        ConfigureHttpClients(builder);

        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddHostedService<StartupService>();
    }

    private static void ConfigureHttpClients(WebApplicationBuilder builder)
    {
        // People Service HTTP Client
        builder.Services.AddHttpClient<IPeopleService, PeopleService>(client =>
        {
            client.BaseAddress = new Uri("https://swapi.tech/api/");
            client.Timeout = TimeSpan.FromSeconds(30.0);
            client.DefaultRequestHeaders.Add("User-Agent", "StarWarsAPI-Consumer/1.0");
        }).AddStandardResilienceHandler();

        // Movie Service HTTP Client
        builder.Services.AddHttpClient<IMovieService, MovieService>(client =>
        {
            client.BaseAddress = new Uri("https://swapi.tech/api/");
            client.Timeout = TimeSpan.FromSeconds(30.0);
            client.DefaultRequestHeaders.Add("User-Agent", "StarWarsAPI-Consumer/1.0");
        }).AddStandardResilienceHandler();
    }

    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(c =>
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Star Wars API",
                Version = "v1",
                Description = "API para gerenciamento de dados do Star Wars"
            }));
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Logging.AddConsole();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/metrics"))
            {
                await next();
            }
            else
            {
                await next();
            }
        });

        app.UseCors("AllowAll");
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        // Swagger apenas em desenvolvimento
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Star Wars API v1"));
        }

        // Pipeline de requisição
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseHttpMetrics();
        app.MapControllers();
        app.MapMetrics();
    }
}