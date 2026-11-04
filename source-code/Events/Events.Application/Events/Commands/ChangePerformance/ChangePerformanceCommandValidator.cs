namespace Events.Application.Events.Commands.ChangePerformance;

public class ChangePerformanceCommandValidator : AbstractValidator<ChangePerformanceCommand>
{
    public ChangePerformanceCommandValidator()
    {
        RuleFor(c => c.EventId).NotEmpty();
        RuleFor(c => c.PerformanceId).NotEmpty();
    }
}
