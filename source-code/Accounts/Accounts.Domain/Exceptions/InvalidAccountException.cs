namespace Accounts.Domain.Exceptions;

public class InvalidAccountException : BaseDomainException
{
    public InvalidAccountException(string error) => Error = error;
}
