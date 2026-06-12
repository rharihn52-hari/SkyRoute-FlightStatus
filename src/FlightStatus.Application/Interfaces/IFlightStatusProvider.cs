using FlightStatus.Domain.Models;

namespace FlightStatus.Application.Interfaces;

/// <summary>
/// Defines the contract for flight status provider implementations.
/// </summary>
public interface IFlightStatusProvider
{
    /// <summary>
    /// Retrieves the flight status for a given flight number and date.
    /// </summary>
    /// <param name="flightNumber">The flight number to query.</param>
    /// <param name="flightDate">The date of the flight.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Flight status result or null if not found.</returns>
    Task<FlightStatusResult?> GetFlightStatusAsync(
        string flightNumber,
        DateOnly flightDate,
        CancellationToken cancellationToken);
}
