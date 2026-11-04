/// <summary>
/// The caller is in scope but their role does not allow the action — a Manager publishing an Event
/// (EVT-A10), say. Someone out of scope gets <see cref="NotFoundException"/> instead.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
