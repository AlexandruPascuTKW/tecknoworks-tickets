using Events.Domain.Models.Images;
using Events.Domain.Repositories;
using Events.Infrastructure.Persistence;

namespace Events.Infrastructure.Repositories;

internal class EventImageRepository : DataRepository<EventsDbContext, EventImage>, IEventImageDomainRepository
{
    public EventImageRepository(EventsDbContext db) : base(db)
    {
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var image = await All().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (image is not null)
        {
            Data.Remove(image);
            await Data.SaveChangesAsync(cancellationToken);
        }
    }
}
