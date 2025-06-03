using StarWars.Infraestructure.Interfaces;
using StarWars.Infraestructure.Models;
using System.Collections.Concurrent;

namespace StarWars.Infraestructure.Services;

public class ConcurrentMessageQueue : IMessageQueue
{
    private readonly ConcurrentQueue<QueueMessage> _queue = new();
    private readonly object _lockObject = new();

    public int Count => _queue.Count;

    public bool IsEmpty => _queue.IsEmpty;

    public void Enqueue(QueueMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        if (message.Priority == MessagePriority.Critical)
        {
            _queue.Enqueue(message);
        }
        else
        {
            _queue.Enqueue(message);
        }
    }

    public bool TryDequeue(out QueueMessage? message)
    {
        return _queue.TryDequeue(out message);
    }

    public bool TryPeek(out QueueMessage? message)
    {
        return _queue.TryPeek(out message);
    }
}