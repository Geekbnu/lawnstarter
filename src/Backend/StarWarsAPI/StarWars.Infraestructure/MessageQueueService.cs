using Microsoft.Extensions.Logging;
using StarWars.Infraestructure.Interfaces;
using StarWars.Infraestructure.Models;

namespace StarWars.Infraestructure.Services;

public class MessageQueueService
{
    private readonly IMessageQueue _messageQueue;
    private readonly ILogger<MessageQueueService> _logger;

    public MessageQueueService(IMessageQueue messageQueue, ILogger<MessageQueueService> logger)
    {
        _messageQueue = messageQueue;
        _logger = logger;
    }

    public void PublishNotificationMessage(string recipient, string title, string content)
    {
        var message = new QueueMessage
        {
            Type = "Notification",
            Payload = new
            {
                Recipient = recipient,
                Title = title,
                Content = content,
                SentAt = DateTimeOffset.UtcNow
            },
            Priority = MessagePriority.Normal
        };

        _messageQueue.Enqueue(message);
        _logger.LogDebug("Published notification message for {Recipient}", recipient);
    }
}