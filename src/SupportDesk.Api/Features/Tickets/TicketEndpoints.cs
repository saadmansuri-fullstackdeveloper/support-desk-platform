using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using SupportDesk.Api.Business;
using SupportDesk.Api.Enum;
using SupportDesk.Api.Models;


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
