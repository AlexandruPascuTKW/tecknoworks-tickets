namespace Events.Domain.Exceptions;

/// <summary>A refusal from the Events domain; the message is shown to the person who tried.</summary>
public class InvalidEventException : BaseDomainException
{
    public InvalidEventException(string error) => Error = error;
}
