using Events.Domain.Models.Images;

namespace Events.Domain.Factories;

public interface IEventImageFactory : IFactory<EventImage>
{
    IEventImageFactory WithContent(string contentType, byte[] content);
}
