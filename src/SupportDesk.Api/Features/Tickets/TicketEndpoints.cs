using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

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

public static class TicketEndpoints
{
    public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tickets").WithTags("Tickets");

        group.MapGet("/", (TicketStore store) => Results.Ok(store.GetAll()))
            .WithName("ListTickets");

        group.MapGet("/{id:guid}", (Guid id, TicketStore store) =>
        {
            var ticket = store.Get(id);
            return ticket is null ? Results.NotFound() : Results.Ok(ticket);
        }).WithName("GetTicket");

        group.MapPost("/", (CreateTicketRequest request, TicketStore store) =>
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = validationResults
                    .SelectMany(result => result.MemberNames.Select(member => (member, result.ErrorMessage ?? "Invalid value.")))
                    .GroupBy(item => item.member)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(item => item.Item2).ToArray());

                return Results.ValidationProblem(errors);
            }

            var created = store.Add(request);
            return Results.Created($"/api/tickets/{created.Id}", created);
        }).WithName("CreateTicket");

        group.MapPatch("/{id:guid}/status", (Guid id, UpdateTicketStatusRequest request, TicketStore store) =>
        {
            if (!Enum.IsDefined(request.Status))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.Status)] = ["Choose a supported ticket status."]
                });
            }

            var updated = store.UpdateStatus(id, request.Status);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateTicketStatus");

        return endpoints;
    }
}
