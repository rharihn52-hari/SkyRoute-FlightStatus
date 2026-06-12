using FlightStatus.Application.Interfaces;
using FlightStatus.Application.Services;
using FlightStatus.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register flight status providers
builder.Services.AddScoped<IFlightStatusProvider, AeroTrackProvider>();
builder.Services.AddScoped<IFlightStatusProvider, QuickFlightProvider>();

// Register aggregation service
builder.Services.AddScoped<FlightStatusAggregationService>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Flight Status Endpoint
app.MapGet("/flights/status", GetFlightStatus)
    .WithName("GetFlightStatus")
    .Produces(200)
    .Produces(400);

async Task<IResult> GetFlightStatus(
    string? flightNumber,
    DateOnly? date,
    FlightStatusAggregationService service,
    ILogger<Program> logger,
    CancellationToken cancellationToken)
{
    // Validation
    if (string.IsNullOrWhiteSpace(flightNumber))
    {
        logger.LogWarning("Flight status request missing flight number");
        return Results.BadRequest(new { error = "Flight number is required" });
    }

    if (!date.HasValue)
    {
        logger.LogWarning("Flight status request missing date");
        return Results.BadRequest(new { error = "Date is required in yyyy-MM-dd format" });
    }

    logger.LogInformation("Fetching flight status for {FlightNumber} on {Date}", flightNumber, date);

    try
    {
        var result = await service.GetAggregatedFlightStatusAsync(
            flightNumber,
            date.Value,
            cancellationToken);

        logger.LogInformation("Flight status retrieved: {FlightNumber} status {Status}", flightNumber, result.Status);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving flight status for {FlightNumber}", flightNumber);
        return Results.StatusCode(500);
    }
}

app.Run();
