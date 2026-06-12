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
    [Fact]
    public async Task GetAggregatedFlightStatusAsync_MostRecentProviderWins_ReturnsLatestResult()
    {
        // Arrange
        var flightNumber = "TEST123";
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var providerA = new Mock<IFlightStatusProvider>();
        providerA.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FlightStatusResult
            {
                FlightNumber = flightNumber,
                FlightDate = flightDate,
                Status = FlightStatusEnum.OnTime,
                LastUpdatedUtc = DateTime.UtcNow.AddHours(-1),
                ProviderName = "ProviderA"
            });

        var providerB = new Mock<IFlightStatusProvider>();
        providerB.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FlightStatusResult
            {
                FlightNumber = flightNumber,
                FlightDate = flightDate,
                Status = FlightStatusEnum.Delayed,
                LastUpdatedUtc = DateTime.UtcNow,
                ProviderName = "ProviderB"
            });

        var logger = Mock.Of<ILogger<FlightStatusAggregationService>>();
        var service = new FlightStatusAggregationService(new[] { providerA.Object, providerB.Object }, logger);

        // Act
        var result = await service.GetAggregatedFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProviderName.Should().Be("ProviderB");
        result.Status.Should().Be(FlightStatusEnum.Delayed);
    }

    [Fact]
    public async Task GetAggregatedFlightStatusAsync_NoProviderResults_ReturnsUnknown()
    {
        // Arrange
        var flightNumber = "TEST456";
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var providerA = new Mock<IFlightStatusProvider>();
        providerA.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightStatusResult?)null);

        var providerB = new Mock<IFlightStatusProvider>();
        providerB.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightStatusResult?)null);

        var logger = Mock.Of<ILogger<FlightStatusAggregationService>>();
        var service = new FlightStatusAggregationService(new[] { providerA.Object, providerB.Object }, logger);

        // Act
        var result = await service.GetAggregatedFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(FlightStatusEnum.Unknown);
        result.ProviderName.Should().Be("None");
    }

    [Fact]
    public async Task GetAggregatedFlightStatusAsync_ProviderFails_ReturnsOtherProviderResult()
    {
        // Arrange
        var flightNumber = "TEST789";
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var failingProvider = new Mock<IFlightStatusProvider>();
        failingProvider.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Provider failure"));

        var healthyProvider = new Mock<IFlightStatusProvider>();
        healthyProvider.Setup(x => x.GetFlightStatusAsync(flightNumber, flightDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FlightStatusResult
            {
                FlightNumber = flightNumber,
                FlightDate = flightDate,
                Status = FlightStatusEnum.OnTime,
                LastUpdatedUtc = DateTime.UtcNow,
                ProviderName = "HealthyProvider"
            });

        var logger = Mock.Of<ILogger<FlightStatusAggregationService>>();
        var service = new FlightStatusAggregationService(new[] { failingProvider.Object, healthyProvider.Object }, logger);

        // Act
        var result = await service.GetAggregatedFlightStatusAsync(flightNumber, flightDate, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProviderName.Should().Be("HealthyProvider");
        result.Status.Should().Be(FlightStatusEnum.OnTime);
    }

    [Fact]
    public async Task AeroTrackProvider_ReturnsExpectedStatusMapping()
    {
        // Arrange
        var provider = new AeroTrackProvider(Mock.Of<ILogger<AeroTrackProvider>>());
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var resultOnTime = await provider.GetFlightStatusAsync("AI202", flightDate, CancellationToken.None);
        var resultDelayed = await provider.GetFlightStatusAsync("AI101", flightDate, CancellationToken.None);
        var resultCancelled = await provider.GetFlightStatusAsync("AI303", flightDate, CancellationToken.None);
        var resultDiverted = await provider.GetFlightStatusAsync("AI404", flightDate, CancellationToken.None);

        // Assert
        resultOnTime.Should().NotBeNull();
        resultOnTime!.Status.Should().Be(FlightStatusEnum.OnTime);

        resultDelayed.Should().NotBeNull();
        resultDelayed!.Status.Should().Be(FlightStatusEnum.Delayed);

        resultCancelled.Should().NotBeNull();
        resultCancelled!.Status.Should().Be(FlightStatusEnum.Cancelled);

        resultDiverted.Should().NotBeNull();
        resultDiverted!.Status.Should().Be(FlightStatusEnum.Diverted);
    }

    [Fact]
    public async Task QuickFlightProvider_ReturnsExpectedStatusMapping()
    {
        // Arrange
        var provider = new QuickFlightProvider(Mock.Of<ILogger<QuickFlightProvider>>());
        var flightDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var resultOnTime = await provider.GetFlightStatusAsync("QS101", flightDate, CancellationToken.None);
        var resultDelayed = await provider.GetFlightStatusAsync("AI101", flightDate, CancellationToken.None);
        var resultCancelled = await provider.GetFlightStatusAsync("BA303", flightDate, CancellationToken.None);

        // Assert
        resultOnTime.Should().NotBeNull();
        resultOnTime!.Status.Should().Be(FlightStatusEnum.OnTime);

        resultDelayed.Should().NotBeNull();
        resultDelayed!.Status.Should().Be(FlightStatusEnum.Delayed);

        resultCancelled.Should().NotBeNull();
        resultCancelled!.Status.Should().Be(FlightStatusEnum.Cancelled);
    }
}
