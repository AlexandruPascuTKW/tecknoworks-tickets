namespace Events.Application.Events.Commands.AddPerformance;

public class AddPerformanceCommandValidator : AbstractValidator<AddPerformanceCommand>
{
    public AddPerformanceCommandValidator()
    {
        RuleFor(c => c.EventId).NotEmpty();
        RuleFor(c => c.Date).NotEqual(default(DateOnly)).WithMessage("Date is required.");
    }
}
