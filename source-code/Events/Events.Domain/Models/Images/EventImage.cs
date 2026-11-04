using Events.Domain.Exceptions;

namespace Events.Domain.Models.Images;

/// <summary>
/// EVT-F6 / ADR-008 — an Event's picture, kept apart from the Event so loading an Event never loads its
/// picture. The format and size are checked in the Application layer, where the settings are.
/// </summary>
public class EventImage : Entity, IAggregateRoot
{
    internal EventImage(string contentType, byte[] content)
    {
        if (string.IsNullOrWhiteSpace(contentType) || content.Length == 0)
        {
            throw new InvalidEventException("Image is required.");
        }

        ContentType = contentType;
        Content = content;
    }

    private EventImage()
    {
        ContentType = default!;
        Content = default!;
    }

    public string ContentType { get; private set; }

    public byte[] Content { get; private set; }
}
