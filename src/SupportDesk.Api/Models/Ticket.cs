using System.ComponentModel.DataAnnotations;
using SupportDesk.Api.Enum;

namespace SupportDesk.Api.Models
{
    public sealed record Ticket(
       Guid Id,
       string Title,
       string Description,
       string CustomerEmail,
       TicketStatus Status,
       DateTimeOffset CreatedAt);

    public sealed record CreateTicketRequest(
        [property: Required, StringLength(120, MinimumLength = 4)] string Title,
        [property: Required, StringLength(4000, MinimumLength = 10)] string Description,
        [property: Required, EmailAddress] string CustomerEmail);

    public sealed record UpdateTicketStatusRequest(TicketStatus Status);

}
