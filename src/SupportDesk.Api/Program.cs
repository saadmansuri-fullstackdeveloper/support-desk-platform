using SupportDesk.Api.Business;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TicketStore>();

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("GetHealth");
app.MapTicketEndpoints();

app.Run();

public partial class Program { }
