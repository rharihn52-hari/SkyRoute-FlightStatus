using FlightStatus.Application.Interfaces;
using FlightStatus.Domain.Enums;
using FlightStatus.Domain.Models;
using Microsoft.Extensions.Logging;
using FlightStatusEnum = FlightStatus.Domain.Enums.FlightStatus;

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
                Status = FlightStatusEnum.Delayed,
                LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-5),
                ProviderName = "QuickFlight",
                Gate = "A12",
                Terminal = "T1",
                DelayReason = "Weather",
                Message = "Flight delayed due to adverse weather conditions"
            },
            "BA303" => new FlightStatusResult
            {
                FlightNumber = "BA303",
                FlightDate = flightDate,
                Status = FlightStatusEnum.Cancelled,
                LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-20),
                ProviderName = "QuickFlight",
                Message = "Flight cancelled"
            },
            "QS101" => new FlightStatusResult
            {
                FlightNumber = "QS101",
                FlightDate = flightDate,
                Status = FlightStatusEnum.OnTime,
                LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-2),
                ProviderName = "QuickFlight",
                Message = "Flight on schedule"
            },
            _ => null
        };
    }
}