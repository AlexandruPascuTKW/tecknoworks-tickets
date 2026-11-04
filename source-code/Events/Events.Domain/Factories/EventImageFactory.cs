using Events.Domain.Models.Images;

namespace Events.Domain.Factories;

internal class EventImageFactory : IEventImageFactory
{
    private string? contentType;
    private byte[]? content;

    public IEventImageFactory WithContent(string contentType, byte[] content)
    {
        this.contentType = contentType;
        this.content = content;
        return this;
    }

    public EventImage Build()
    {
        if (contentType is null || content is null)
        {
            throw new InvalidOperationException("An image needs a content type and content.");
        }

        return new EventImage(contentType, content);
    }
}
