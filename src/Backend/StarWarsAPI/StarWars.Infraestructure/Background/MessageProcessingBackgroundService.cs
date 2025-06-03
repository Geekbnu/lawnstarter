using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarWars.Infraestructure.Interfaces;
using StarWars.Infraestructure.Models;
using StarWarsApi.Metric;

namespace StarWars.Infraestructure.Background;

public class MessageProcessingBackgroundService : BackgroundService
{
    private readonly ILogger<MessageProcessingBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMessageQueue _messageQueue;
    private readonly TimeSpan _executionInterval = TimeSpan.FromMinutes(3);

    public MessageProcessingBackgroundService(
        ILogger<MessageProcessingBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IMessageQueue messageQueue)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _messageQueue = messageQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MessageProcessingBackgroundService started at {Time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishScheduledMessage();

                await ProcessPendingMessages();

                _logger.LogInformation("MessageProcessingBackgroundService cycle completed at {Time}",
                    DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in MessageProcessingBackgroundService at {Time}",
                    DateTimeOffset.Now);
            }

            await Task.Delay(_executionInterval, stoppingToken);
        }

        _logger.LogInformation("MessageProcessingBackgroundService stopped at {Time}", DateTimeOffset.Now);
    }

    private async Task PublishScheduledMessage()
    {
        try
        {
            var message = new QueueMessage
            {
                Id = Guid.NewGuid(),
                Type = "ScheduledTask",
                Payload = new
                {
                    ExecutedAt = DateTimeOffset.UtcNow,
                    Source = nameof(MessageProcessingBackgroundService),
                    Description = "Scheduled message published every 5 minutes",
                    TaskId = Guid.NewGuid()
                },
                CreatedAt = DateTimeOffset.UtcNow,
                Priority = MessagePriority.Normal
            };

            _messageQueue.Enqueue(message);

            _logger.LogInformation("Published scheduled message with ID: {MessageId} at {Time}",
                message.Id, message.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing scheduled message");
        }
    }

    private async Task ProcessPendingMessages()
    {
        try
        {
            var processedCount = 0;
            const int maxMessagesToProcess = 1;

            while (processedCount < maxMessagesToProcess && _messageQueue.TryDequeue(out var message))
            {
                var scope = _serviceScopeFactory.CreateScope();
                var metrics = scope.ServiceProvider.GetService<MovieMetricsService>();

                if (metrics != null)
                {
                    await metrics.DeleteAllMetricsAsync();
                }

                processedCount++;
            }

            if (processedCount > 0)
            {
                _logger.LogInformation("Processed {Count} messages in this cycle", processedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending messages");
        }
    }
}