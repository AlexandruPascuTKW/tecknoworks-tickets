using Events.Application.Common;
using Events.Application.Events.Commands.Common;
using Events.Domain.Factories;
using Events.Domain.Repositories;

namespace Events.Application.Events.Commands.ChangeDetails;

/// <summary>EVT-A3 — Change an Event's details. Name, description, image any time; Venue only while Draft (EVT-R8).</summary>
public class ChangeEventDetailsCommand : IRequest<Result>
{
    public Guid EventId { get; set; }

    public string? EventName { get; set; }

    public string? Description { get; set; }

    public ImageUpload? Image { get; set; }

    public Guid? VenueId { get; set; }

    public class ChangeEventDetailsCommandHandler : IRequestHandler<ChangeEventDetailsCommand, Result>
    {
        private readonly IEventDomainRepository events;
        private readonly IVenueDomainRepository venues;
        private readonly IEventImageDomainRepository images;
        private readonly IEventImageFactory imageFactory;
        private readonly EventAccess access;

        public ChangeEventDetailsCommandHandler(
            IEventDomainRepository events,
            IVenueDomainRepository venues,
            IEventImageDomainRepository images,
            IEventImageFactory imageFactory,
            EventAccess access)
        {
            this.events = events;
            this.venues = venues;
            this.images = images;
            this.imageFactory = imageFactory;
            this.access = access;
        }

        public async Task<Result> Handle(ChangeEventDetailsCommand request, CancellationToken cancellationToken)
        {
            var @event = await access.LoadEvent(request.EventId, cancellationToken);

            var venue = request.VenueId is { } venueId
                ? await venues.Find(venueId, cancellationToken)
                : null;

            if (request.VenueId is not null && (venue is null || venue.OrganizerId != @event.OrganizerId))
            {
                throw new NotFoundException("Venue", request.VenueId);
            }

            var previousImage = @event.ImageId;
            Guid? newImage = null;

            if (request.Image is not null)
            {
                var image = imageFactory
                    .WithContent(ImageRules.Detect(request.Image.Content)!, request.Image.Content)
                    .Build();

                await images.Save(image, cancellationToken);
                newImage = image.Id;
            }

            @event.ChangeDetails(request.EventName, request.Description, newImage, venue);

            await events.Save(@event, cancellationToken);

            if (newImage is not null && previousImage is { } old)
            {
                await images.Delete(old, cancellationToken);
            }

            return Result.Success;
        }
    }
}
