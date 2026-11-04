using Admission.Domain.Models.Performances;
using Admission.Domain.Repositories;
using Admission.Infrastructure.Persistence;

namespace Admission.Infrastructure.Repositories;

internal class PerformanceRepository : DataRepository<AdmissionDbContext, Performance>, IPerformanceDomainRepository
{
    public PerformanceRepository(AdmissionDbContext db) : base(db)
    {
    }

    public async Task<List<Performance>> ForEvent(Guid eventId, CancellationToken cancellationToken = default)
        => await All().Where(p => p.EventId == eventId).ToListAsync(cancellationToken);

    public async Task Delete(Performance performance, CancellationToken cancellationToken = default)
    {
        Data.Remove(performance);
        await Data.SaveChangesAsync(cancellationToken);
    }
}
