using Events.Application.Common;
using Events.Application.Events.Commands.Common;
using Events.Domain.Factories;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.Create;

/// <summary>EVT-A2 — Create an Event at one of the Organizer's Venues. It starts as Draft (EVT-R3).</summary>
public class CreateEventCommand : IRequest<CreateEventResponse>
{
    public Guid OrganizerId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public Guid VenueId { get; set; }

    public string? Description { get; set; }

    public ImageUpload? Image { get; set; }

    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, CreateEventResponse>
    {
        private readonly IEventDomainRepository events;
        private readonly IVenueDomainRepository venues;
        private readonly IEventImageDomainRepository images;
        private readonly IEventFactory eventFactory;
        private readonly IEventImageFactory imageFactory;
        private readonly EventAccess access;

        public CreateEventCommandHandler(
            IEventDomainRepository events,
            IVenueDomainRepository venues,
            IEventImageDomainRepository images,
            IEventFactory eventFactory,
            IEventImageFactory imageFactory,
            EventAccess access)
        {
            this.events = events;
            this.venues = venues;
            this.images = images;
            this.eventFactory = eventFactory;
            this.imageFactory = imageFactory;
            this.access = access;
        }

        public async Task<CreateEventResponse> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            await access.Require(request.OrganizerId, "Organizer", request.OrganizerId, cancellationToken);

            // EVT-R2 / EVT-R4: only the Organizer's own Venues; another's is simply not there.
            var venue = await venues.Find(request.VenueId, cancellationToken);
            if (venue is null || venue.OrganizerId != request.OrganizerId)
            {
                throw new NotFoundException("Venue", request.VenueId);
            }

            var factory = eventFactory
                .ForOrganizer(request.OrganizerId)
                .WithName(request.EventName)
                .WithDescription(request.Description)
                .AtVenue(venue);

            if (request.Image is not null)
            {
                var image = imageFactory
                    .WithContent(ImageRules.Detect(request.Image.Content)!, request.Image.Content)
                    .Build();

                await images.Save(image, cancellationToken);
                factory.WithImage(image.Id);
            }

            var @event = factory.Build();

            await events.Save(@event, cancellationToken);

            return new CreateEventResponse(@event.Id);
        }
    }
}

public record CreateEventResponse(Guid Id);
