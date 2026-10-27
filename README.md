# Support Desk Platform

A portfolio project for demonstrating ASP.NET Core API design through a customer support ticket workflow.

## Current milestone

- ASP.NET Core 10 Minimal API
- Create, list, retrieve, and update the status of support tickets
- Request validation with clear HTTP error responses
- Health endpoint and development OpenAPI document
- Thread-safe in-memory store for a quick local demo

> **Demo limitation:** Tickets are stored in memory and disappear when the API restarts. Authentication, authorization, a database, and production deployment are planned milestones, not implemented features.

## Run locally

Requires the .NET 10 SDK. The first restore requires access to configured NuGet feeds.

```powershell
dotnet restore
dotnet run --project src/SupportDesk.Api
```

Use the sample requests in `src/SupportDesk.Api/SupportDesk.Api.http` to explore the API.

## Example request

```http
POST /api/tickets
Content-Type: application/json

{
  "title": "Unable to access my account",
  "description": "The password reset link expires before I can use it.",
  "customerEmail": "customer@example.com"
}
```

The API responds with `201 Created` and a link to the new ticket. Use `GET /api/tickets` to list tickets, or `PATCH /api/tickets/{id}/status` with `{ "status": "InProgress" }` to change a ticket's status.

## Planned portfolio milestones

1. Persist tickets with EF Core and SQL Server, including migrations.
2. Add user sign-in and role-based access for customers and support agents.
3. Add comments, assignment, filtering, pagination, and audit history.
4. Add integration tests, containerized local dependencies, and a CI workflow.
5. Deploy a public demo and document the architecture and API contract.

Each milestone will be marked complete here only after it is implemented.

