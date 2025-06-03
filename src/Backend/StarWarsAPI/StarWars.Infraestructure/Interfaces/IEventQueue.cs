using StarWars.Infraestucture.Models;

namespace StarWars.Infraestructure.Interfaces;

public interface IEventQueue
{
    Task PublishAsync<T>(T @event) where T : DomainEvent;
    void Subscribe<T>(Func<T, Task> handler) where T : DomainEvent;
}