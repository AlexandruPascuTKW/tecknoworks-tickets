namespace Accounts.Domain.Models.Accounts;

/// <summary>Platform roles held by TecknoworksTickets staff (ACC glossary: Platform admin, Finance manager).</summary>
public class PlatformRole : Enumeration
{
    public static readonly PlatformRole PlatformAdmin = new(1, nameof(PlatformAdmin));
    public static readonly PlatformRole FinanceManager = new(2, nameof(FinanceManager));

    private PlatformRole(int value, string name) : base(value, name)
    {
    }
}
