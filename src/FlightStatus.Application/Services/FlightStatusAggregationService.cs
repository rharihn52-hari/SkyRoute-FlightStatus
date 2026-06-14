using FlightStatus.Application.Interfaces;
using FlightStatus.Domain.Enums;
using FlightStatus.Domain.Models;
using Microsoft.Extensions.Logging;
using FlightStatusEnum = FlightStatus.Domain.Enums.FlightStatus;

namespace FlightStatus.Application.Services;

/// <summary>
/// Aggregates flight status from multiple providers with failure resilience.
/// </summary>
public class FlightStatusAggregationService
{
    private readonly IEnumerable<IFlightStatusProvider> _providers;
    private readonly ILogger<FlightStatusAggregationService> _logger;

    public FlightStatusAggregationService(
        IEnumerable<IFlightStatusProvider> providers,
        ILogger<FlightStatusAggregationService> logger)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets aggregated flight status from all providers.
    /// Queries all providers in parallel and returns the most recent result.
    /// </summary>
    /// <remarks>
    /// Selection rules (in order):
    ///   1. Most recent <see cref="FlightStatusResult.LastUpdatedUtc"/> wins.
    ///   2. On timestamp tie, the result with more populated detail fields
    ///      (Gate, Terminal, DelayReason, Message) wins — "richer data" rule.
    ///   3. On both ties, alphabetical provider name wins for full determinism.
    /// </remarks>
    public async Task<FlightStatusResult> GetAggregatedFlightStatusAsync(
        string flightNumber,
        DateOnly flightDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
        {
            _logger.LogWarning("Flight number is null or empty");
            return CreateUnknownResult(flightNumber, flightDate);
        }

        _logger.LogInformation("Aggregating flight status for {FlightNumber} on {FlightDate}", flightNumber, flightDate);

        // Call all providers in parallel
        var tasks = _providers.Select(provider => CallProviderSafelyAsync(provider, flightNumber, flightDate, cancellationToken));
        var results = await Task.WhenAll(tasks);

        // Filter out null results
        var validResults = results.Where(r => r != null).Cast<FlightStatusResult>().ToList();

        if (validResults.Count == 0)
        {
            _logger.LogWarning("No providers returned data for flight {FlightNumber}", flightNumber);
            return CreateUnknownResult(flightNumber, flightDate);
        }

        if (validResults.Count == 1)
        {
            _logger.LogInformation("Single result found for flight {FlightNumber} from provider {Provider}", flightNumber, validResults[0].ProviderName);
            return validResults[0];
        }

        // Multiple results: apply deterministic selection rules
        var winner = validResults
            .OrderByDescending(r => r.LastUpdatedUtc)
            .ThenByDescending(r => CountPopulatedDetailFields(r))
            .ThenBy(r => r.ProviderName, StringComparer.Ordinal)
            .First();

        _logger.LogInformation(
            "Multiple results found for flight {FlightNumber}, selected provider {Provider} (timestamp {Timestamp:o})",
            flightNumber,
            winner.ProviderName,
            winner.LastUpdatedUtc);

        return winner;
    }

    /// <summary>
    /// Counts how many optional detail fields are populated.
    /// Used as a deterministic tie-breaker when two providers report identical timestamps.
    /// </summary>
    private static int CountPopulatedDetailFields(FlightStatusResult result)
    {
        var count = 0;
        if (!string.IsNullOrWhiteSpace(result.Gate)) count++;
        if (!string.IsNullOrWhiteSpace(result.Terminal)) count++;
        if (!string.IsNullOrWhiteSpace(result.DelayReason)) count++;
        if (!string.IsNullOrWhiteSpace(result.Message)) count++;
        return count;
    }

    /// <summary>
    /// Safely calls a provider, catching and logging any exceptions.
    /// </summary>
    private async Task<FlightStatusResult?> CallProviderSafelyAsync(
        IFlightStatusProvider provider,
        string flightNumber,
        DateOnly flightDate,
        CancellationToken cancellationToken)
    {
        try
        {
            return await provider.GetFlightStatusAsync(flightNumber, flightDate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider {ProviderType} failed for flight {FlightNumber}", provider.GetType().Name, flightNumber);
            // Return null to allow aggregation to continue with other providers
            return null;
        }
    }

    /// <summary>
    /// Creates an Unknown status result for a flight.
    /// </summary>
    private static FlightStatusResult CreateUnknownResult(string flightNumber, DateOnly flightDate)
    {
        return new FlightStatusResult
        {
            FlightNumber = flightNumber,
            FlightDate = flightDate,
            Status = FlightStatusEnum.Unknown,
            LastUpdatedUtc = DateTime.UtcNow,
            ProviderName = "None",
            Message = "Flight status unknown - no provider data available"
        };
    }
}