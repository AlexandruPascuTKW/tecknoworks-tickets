using Events.Domain.Models.Venues;

namespace Events.Application.Venues.Commands.Add;

public class AddVenueCommandValidator : AbstractValidator<AddVenueCommand>
{
    public AddVenueCommandValidator()
    {
        RuleFor(c => c.OrganizerId).NotEmpty();

        RuleFor(c => c.VenueName)
            .NotEmpty().WithMessage("Venue name is required.")
            .MaximumLength(VenueModelConstants.Venue.MaxNameLength);

        RuleFor(c => c.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(VenueModelConstants.Venue.MaxAddressLength);

        // EVT-F3
        RuleFor(c => c.Capacity).GreaterThan(0).WithMessage("Capacity must be a whole number above zero.");
    }
}
