using Admission.Domain.Models.Performances;

namespace Admission.Domain.Factories;

public interface IPerformanceFactory : IFactory<Performance>
{
    IPerformanceFactory ForPerformance(
        Guid performanceId,
        Guid eventId,
        Guid organizerId,
        string eventName,
        string venueName,
        DateOnly date,
        TimeOnly doorsTime,
        TimeOnly startTime);
}
