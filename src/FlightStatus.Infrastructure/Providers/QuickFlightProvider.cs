using FlightStatus.Application.Interfaces;
using FlightStatus.Domain.Enums;
using FlightStatus.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FlightStatus.Infrastructure.Providers;

/// <summary>
/// QuickFlight provider stub implementation with deterministic responses.
/// </summary>
public class QuickFlightProvider : IFlightStatusProvider
{
    private readonly ILogger<QuickFlightProvider> _logger;

    public QuickFlightProvider(ILogger<QuickFlightProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets flight status from QuickFlight (hardcoded stub data).
    /// </summary>
    public async Task<FlightStatusResult?> GetFlightStatusAsync(
        string flightNumber,
        DateOnly flightDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("QuickFlightProvider: Querying flight {FlightNumber} on {FlightDate}", flightNumber, flightDate);

        // Simulate async operation
        await Task.Delay(30, cancellationToken);

        return flightNumber switch
        {
            "AI101" => new FlightStatusResult
            {
                FlightNumber = "AI101",
                FlightDate = flightDate,
                Status = FlightStatus.Delayed,
                LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-5),
                ProviderName = "QuickFlight",
                Message = "Flight running late"
            },
            "BA303" => new FlightStatusResult
            {
                FlightNumber = "BA303",
                FlightDate = flightDate,
                Status = FlightStatus.Cancelled,
                LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-20),
                ProviderName = "QuickFlight",
                Message = "Flight cancelled"
            },
            _ => null
        };
    }
}
