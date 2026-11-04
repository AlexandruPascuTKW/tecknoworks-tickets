using Accounts.Domain.Models.Organizers;

namespace Accounts.Domain.Factories;

/// <summary>ACC-R4 — an Organizer is built together with its first Owner.</summary>
internal class OrganizerFactory : IOrganizerFactory
{
    private string? organizerName;
    private Guid? firstOwner;

    public IOrganizerFactory WithName(string organizerName)
    {
        this.organizerName = organizerName;
        return this;
    }

    public IOrganizerFactory WithFirstOwner(Guid accountId)
    {
        firstOwner = accountId;
        return this;
    }

    public Organizer Build()
    {
        if (organizerName is null || firstOwner is null)
        {
            throw new InvalidOperationException("An Organizer needs a name and a first Owner.");
        }

        return new Organizer(organizerName, firstOwner.Value);
    }
}
