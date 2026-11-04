namespace Admission.Application.Tickets.Queries.Mine;

/// <summary>ADM-V1 — See my Tickets. Only the Tickets the Attendee bought (ADM-R1).</summary>
public class MyTicketsQuery : IRequest<List<MyTicketItem>>
{
    public class MyTicketsQueryHandler : IRequestHandler<MyTicketsQuery, List<MyTicketItem>>
    {
        private readonly ITicketQueryRepository tickets;
        private readonly ICurrentUser user;

        public MyTicketsQueryHandler(ITicketQueryRepository tickets, ICurrentUser user)
        {
            this.tickets = tickets;
            this.user = user;
        }

        public async Task<List<MyTicketItem>> Handle(MyTicketsQuery request, CancellationToken cancellationToken)
        {
            var buyerId = user.UserId ?? throw new ForbiddenException("Sign in first.");

            return await tickets.Mine(buyerId, cancellationToken);
        }
    }
}
