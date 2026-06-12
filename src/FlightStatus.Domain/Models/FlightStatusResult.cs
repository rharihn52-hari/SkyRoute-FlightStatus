global using FlightStatusEnum = FlightStatus.Domain.Enums.FlightStatus;

namespace FlightStatus.Domain.Models;

public record FlightStatusResult
{
    public string FlightNumber { get; init; } = string.Empty;

    public DateOnly FlightDate { get; init; }

    public FlightStatusEnum Status { get; init; }

    public DateTime LastUpdatedUtc { get; init; }

    public string ProviderName { get; init; } = string.Empty;

    public string? Gate { get; init; }

    public string? Terminal { get; init; }

    public string? DelayReason { get; init; }

    public string? Message { get; init; }
}
