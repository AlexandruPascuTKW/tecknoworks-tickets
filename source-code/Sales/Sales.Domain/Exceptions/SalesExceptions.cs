namespace Sales.Domain.Exceptions;

/// <summary>SAL-R3 — a checkout that is refused; the message is shown to the Attendee.</summary>
public class CheckoutRefusedException : BaseDomainException
{
    public CheckoutRefusedException(string error) => Error = error;
}

/// <summary>A refusal about an Order's life: paying one that is not payable, a bad Payment, and so on.</summary>
public class InvalidOrderException : BaseDomainException
{
    public InvalidOrderException(string error) => Error = error;
}

/// <summary>
/// Two changes to the same Performance raced and this one lost (ADR-002). Not a refusal: the caller
/// reloads and tries again.
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException() : base("The Performance changed while this was being saved.")
    {
    }
}
