using FlightStatus.Application.Interfaces;
using FlightStatus.Domain.Enums;
using FlightStatus.Domain.Models;
using Microsoft.Extensions.Logging;
using FlightStatusEnum = FlightStatus.Domain.Enums.FlightStatus;

namespace FlightStatus.Infrastructure.Providers;

/// <summary>
/// AeroTrack provider stub implementation with deterministic responses.
/// </summary>
public class AeroTrackProvider : IFlightStatusProvider
{
    private readonly ILogger<AeroTrackProvider> _logger;

    public AeroTrackProvider(ILogger<AeroTrackProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets flight status from AeroTrack (hardcoded stub data).
    /// </summary>
    public async Task<FlightStatusResult?> GetFlightStatusAsync(
        string flightNumber,
        DateOnly flightDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("AeroTrackProvider: Querying flight {FlightNumber} on {FlightDate}", flightNumber, flightDate);

        // Simulate async operation
        await Task.Delay(50, cancellationToken);

        return flightNumber switch
        {
            "AI101" => new FlightStatusResult
            {
                FlightNumber = "AI101",
                FlightDate = flightDate,
                Status = FlightStatusEnum.Delayed,
                LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-10),
                ProviderName = "AeroTrack",
                Gate = "A12",
                Terminal = "T1",
                DelayReason = "Weather",
                Message = "Flight delayed due to adverse weather conditions"
            },
            "AI202" => new FlightStatusResult
            {
                FlightNumber = "AI202",
                FlightDate = flightDate,
                Status = FlightStatusEnum.OnTime,
                LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-15),
                ProviderName = "AeroTrack",
                Message = "Flight on schedule"
            },
            "AI303" => new FlightStatusResult
            {
                FlightNumber = "AI303",
                FlightDate = flightDate,
                Status = FlightStatusEnum.Cancelled,
                LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-20),
                ProviderName = "AeroTrack",
                Message = "Flight cancelled"
            },
            "AI404" => new FlightStatusResult
            {
                FlightNumber = "AI404",
                FlightDate = flightDate,
                Status = FlightStatusEnum.Diverted,
                LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-25),
                ProviderName = "AeroTrack",
                Message = "Flight diverted"
            },
            _ => null
        };
    }
}
