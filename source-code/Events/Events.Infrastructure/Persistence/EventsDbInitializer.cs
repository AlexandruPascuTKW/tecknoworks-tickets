namespace Events.Infrastructure.Persistence;

internal class EventsDbInitializer : DbInitializer
{
    public EventsDbInitializer(EventsDbContext db) : base(db)
    {
    }
}
