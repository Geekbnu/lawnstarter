using StarWars.Infraestructure.Models;

namespace StarWars.Infraestructure.Interfaces;

public interface IMessageQueue
{
   
    void Enqueue(QueueMessage message);
    bool TryDequeue(out QueueMessage? message);

    bool TryPeek(out QueueMessage? message);

    int Count { get; }

    bool IsEmpty { get; }
}