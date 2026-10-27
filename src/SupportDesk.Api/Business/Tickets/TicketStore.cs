using System.Collections.Concurrent;
using SupportDesk.Api.Enum;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.Business
{
    public sealed class TicketStore
    {
        private readonly ConcurrentDictionary<Guid, Ticket> _tickets = new();

        public IReadOnlyCollection<Ticket> GetAll() => _tickets.Values
            .OrderByDescending(ticket => ticket.CreatedAt)
            .ToArray();

        public Ticket? Get(Guid id) => _tickets.GetValueOrDefault(id);

        public Ticket Add(CreateTicketRequest request)
        {
            var ticket = new Ticket(
                Guid.NewGuid(),
                request.Title.Trim(),
                request.Description.Trim(),
                request.CustomerEmail.Trim(),
                TicketStatus.Open,
                DateTimeOffset.UtcNow);

            _tickets[ticket.Id] = ticket;
            return ticket;
        }

        public Ticket? UpdateStatus(Guid id, TicketStatus status)
        {
            while (_tickets.TryGetValue(id, out var current))
            {
                var updated = current with { Status = status };
                if (_tickets.TryUpdate(id, updated, current))
                {
                    return updated;
                }
            }

            return null;
        }
    }

}
