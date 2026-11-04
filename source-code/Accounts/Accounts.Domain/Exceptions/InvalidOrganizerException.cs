namespace Accounts.Domain.Exceptions;

public class InvalidOrganizerException : BaseDomainException
{
    public InvalidOrganizerException(string error) => Error = error;
}
