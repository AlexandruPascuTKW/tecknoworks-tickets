namespace Sales.Domain.Models.Orders;

/// <summary>
/// SAL-F1 — who placed the Order, as they were when checkout started. The Name and Email are kept so the
/// sales list (SAL-V3) and the Tickets (ADM-R2) never need to ask Accounts.
/// </summary>
public class Buyer : ValueObject
{
    private Buyer()
    {
        Name = default!;
        Email = default!;
    }

    public Buyer(Guid accountId, string name, string email)
    {
        AccountId = accountId;
        Name = name;
        Email = email;
    }

    public Guid AccountId { get; private set; }

    public string Name { get; private set; }

    public string Email { get; private set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AccountId;
        yield return Name;
        yield return Email;
    }
}
