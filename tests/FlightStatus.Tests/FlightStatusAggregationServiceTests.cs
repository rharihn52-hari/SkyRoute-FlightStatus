using FluentAssertions;
using FlightStatus.Application.Interfaces;
using FlightStatus.Application.Services;
using FlightStatus.Domain.Models;
using FlightStatus.Infrastructure.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using FlightStatusEnum = FlightStatus.Domain.Enums.FlightStatus;

namespace FlightStatus.Tests;

public class FlightStatusAggregationServiceTests
{
    private static FlightStatusAggregationService BuildService(params IFlightStatusProvider[] providers)
    {
        return new FlightStatusAggregationService(
            providers,
            Mock.Of<ILogger<FlightStatusAggregationService>>());
    }

    private static Mock<IFlightStatusProvider> MockProvider(
        string flightNumber,
        DateOnly flightDate,
        FlightStatusEnum status,
        DateTime lastUpdatedUtc,
        string providerName,
        string? gate = null,
        string? terminal = null,
        string? delayReason = null,
        string? message = null)
    {
        var mock = new Mock<IFlightStatusProvider>();
        mock.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FlightStatusResult
            {
                FlightNumber = flightNumber,
                FlightDate = flightDate,
                Status = status,
                LastUpdatedUtc = lastUpdatedUtc,
                ProviderName = providerName,
                Gate = gate,
                Terminal = terminal,
                DelayReason = delayReason,
                Message = message
            });
        return mock;
    }

    [Fact]
    public async Task GetAggregatedFlightStatusAsync_MostRecentProviderWins_ReturnsLatestResult()
    {
        var flightNumber = "TEST123";
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var providerA = MockProvider(flightNumber, flightDate, FlightStatusEnum.OnTime, now.AddHours(-1), "ProviderA");
        var providerB = MockProvider(flightNumber, flightDate, FlightStatusEnum.Delayed, now, "ProviderB");

        var service = BuildService(providerA.Object, providerB.Object);

        var result = await service.GetAggregatedFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        result.ProviderName.Should().Be("ProviderB");
        result.Status.Should().Be(FlightStatusEnum.Delayed);
    }

    [Fact]
    public async Task GetAggregatedFlightStatusAsync_NoProviderResults_ReturnsUnknown()
    {
        var flightNumber = "TEST456";
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var providerA = new Mock<IFlightStatusProvider>();
        providerA.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightStatusResult?)null);

        var providerB = new Mock<IFlightStatusProvider>();
        providerB.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightStatusResult?)null);

        var service = BuildService(providerA.Object, providerB.Object);

        var result = await service.GetAggregatedFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        result.Status.Should().Be(FlightStatusEnum.Unknown);
        result.ProviderName.Should().Be("None");
    }

    [Fact]
    public async Task GetAggregatedFlightStatusAsync_ProviderFails_ReturnsOtherProviderResult()
    {
        var flightNumber = "TEST789";
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var failingProvider = new Mock<IFlightStatusProvider>();
        failingProvider.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Provider failure"));

        var healthyProvider = MockProvider(flightNumber, flightDate, FlightStatusEnum.OnTime, DateTime.UtcNow, "HealthyProvider");

        var service = BuildService(failingProvider.Object, healthyProvider.Object);

        var result = await service.GetAggregatedFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        result.ProviderName.Should().Be("HealthyProvider");
        result.Status.Should().Be(FlightStatusEnum.OnTime);
    }

    [Fact]
    public async Task GetAggregatedFlightStatusAsync_BothProvidersFail_ReturnsUnknown()
    {
        var flightNumber = "TEST999";
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var failingA = new Mock<IFlightStatusProvider>();
        failingA.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Provider A down"));

        var failingB = new Mock<IFlightStatusProvider>();
        failingB.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Provider B timed out"));

        var service = BuildService(failingA.Object, failingB.Object);

        var result = await service.GetAggregatedFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        result.Status.Should().Be(FlightStatusEnum.Unknown);
        result.ProviderName.Should().Be("None");
    }

    [Fact]
    public async Task GetAggregatedFlightStatusAsync_IdenticalTimestamps_RicherDataWins()
    {
        // Tie-breaker rule: when LastUpdatedUtc is identical, the provider with
        // more populated detail fields (Gate/Terminal/DelayReason/Message) wins.
        var flightNumber = "TIE100";
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var sameTimestamp = DateTime.UtcNow;

        // Sparse provider: only Message
        var sparseProvider = MockProvider(
            flightNumber, flightDate, FlightStatusEnum.Delayed, sameTimestamp,
            "SparseProvider", message: "Delayed");

        // Rich provider: Gate, Terminal, DelayReason, Message
        var richProvider = MockProvider(
            flightNumber, flightDate, FlightStatusEnum.Delayed, sameTimestamp,
            "RichProvider", gate: "A12", terminal: "T1", delayReason: "Weather", message: "Delayed");

        var service = BuildService(sparseProvider.Object, richProvider.Object);

        var result = await service.GetAggregatedFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        result.ProviderName.Should().Be("RichProvider");
        result.Gate.Should().Be("A12");
    }

    [Fact]
    public async Task GetAggregatedFlightStatusAsync_IdenticalTimestampsAndDetailCount_AlphabeticalProviderWins()
    {
        // Final tie-breaker: when timestamps AND populated-field counts are equal,
        // alphabetical provider name (ordinal) wins to guarantee determinism.
        var flightNumber = "TIE200";
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var sameTimestamp = DateTime.UtcNow;

        var providerZ = MockProvider(flightNumber, flightDate, FlightStatusEnum.OnTime, sameTimestamp, "ZetaProvider", message: "OK");
        var providerA = MockProvider(flightNumber, flightDate, FlightStatusEnum.OnTime, sameTimestamp, "AlphaProvider", message: "OK");

        var service = BuildService(providerZ.Object, providerA.Object);

        var result = await service.GetAggregatedFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        result.ProviderName.Should().Be("AlphaProvider");
    }

    // ─── Data-driven provider mapping tests ───

    [Theory]
    [InlineData("AI202", FlightStatusEnum.OnTime)]
    [InlineData("AI101", FlightStatusEnum.Delayed)]
    [InlineData("AI303", FlightStatusEnum.Cancelled)]
    [InlineData("AI404", FlightStatusEnum.Diverted)]
    public async Task AeroTrackProvider_MapsFlightNumberToExpectedStatus(string flightNumber, FlightStatusEnum expectedStatus)
    {
        var provider = new AeroTrackProvider(Mock.Of<ILogger<AeroTrackProvider>>());
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await provider.GetFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData("QS101", FlightStatusEnum.OnTime)]
    [InlineData("AI101", FlightStatusEnum.Delayed)]
    [InlineData("BA303", FlightStatusEnum.Cancelled)]
    public async Task QuickFlightProvider_MapsFlightNumberToExpectedStatus(string flightNumber, FlightStatusEnum expectedStatus)
    {
        var provider = new QuickFlightProvider(Mock.Of<ILogger<QuickFlightProvider>>());
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await provider.GetFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAggregatedFlightStatusAsync_InvalidFlightNumber_ReturnsUnknown(string? flightNumber)
    {
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var dummyProvider = new Mock<IFlightStatusProvider>();
        var service = BuildService(dummyProvider.Object);

        var result = await service.GetAggregatedFlightStatusAsync(flightNumber!, flightDate, CancellationToken.None);

        result.Status.Should().Be(FlightStatusEnum.Unknown);
        result.ProviderName.Should().Be("None");
    }
}