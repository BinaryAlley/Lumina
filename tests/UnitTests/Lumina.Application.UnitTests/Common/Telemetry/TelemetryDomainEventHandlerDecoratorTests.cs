#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Telemetry;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Common.Telemetry;

/// <summary>
/// Contains unit tests for the <see cref="TelemetryDomainEventHandlerDecorator{TDomainEvent}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TelemetryDomainEventHandlerDecoratorTests
{
    private readonly IDomainEventHandler<LibrarySavedDomainEvent> _mockInnerHandler;
    private readonly TelemetryDomainEventHandlerDecorator<LibrarySavedDomainEvent> _sut;
    private readonly LibrarySavedDomainEventFixture _librarySavedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryDomainEventHandlerDecoratorTests"/> class.
    /// </summary>
    public TelemetryDomainEventHandlerDecoratorTests()
    {
        _mockInnerHandler = Substitute.For<IDomainEventHandler<LibrarySavedDomainEvent>>();
        ILogger<TelemetryDomainEventHandlerDecorator<LibrarySavedDomainEvent>> mockLogger =
            Substitute.For<ILogger<TelemetryDomainEventHandlerDecorator<LibrarySavedDomainEvent>>>();
        _sut = new TelemetryDomainEventHandlerDecorator<LibrarySavedDomainEvent>(
            _mockInnerHandler,
            mockLogger);
    }

    [Fact]
    public async Task HandleAsync_WhenInnerHandlerSucceeds_ShouldDelegateAndEmitSuccessfulTraceAndMetric()
    {
        // Arrange
        List<Activity> capturedActivities = [];
        using ActivityListener activityListener = TelemetryTestHelpers.CreateActivityListener(capturedActivities);
        List<(string InstrumentName, double Value, string Outcome)> metricMeasurements = [];
        using MeterListener meterListener = TelemetryTestHelpers.CreateMeterListener(metricMeasurements);
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create();
        _mockInnerHandler.HandleAsync(Arg.Any<LibrarySavedDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockInnerHandler.Received(1).HandleAsync(domainEvent, CancellationToken.None);
        Activity activity = Assert.Single(capturedActivities);
        Assert.Equal($"Handle {_mockInnerHandler.GetType().Name}", activity.OperationName);
        Assert.Equal("domain-event", activity.GetTagItem("lumina.handler.type"));
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Contains(metricMeasurements, measurement => measurement.InstrumentName == "lumina.application.handler.duration" && measurement.Outcome == "success");
        Assert.Contains(metricMeasurements, measurement => measurement.InstrumentName == "lumina.application.handler.invocations" && measurement.Outcome == "success");
    }

    [Fact]
    public async Task HandleAsync_WhenInnerHandlerThrows_ShouldRethrowAndEmitFailureTraceAndMetric()
    {
        // Arrange
        List<Activity> capturedActivities = [];
        using ActivityListener activityListener = TelemetryTestHelpers.CreateActivityListener(capturedActivities);
        List<(string InstrumentName, double Value, string Outcome)> metricMeasurements = [];
        using MeterListener meterListener = TelemetryTestHelpers.CreateMeterListener(metricMeasurements);
        LibrarySavedDomainEvent domainEvent = _librarySavedDomainEventFixture.Create();
        InvalidOperationException exception = new("boom");
        _mockInnerHandler.HandleAsync(Arg.Any<LibrarySavedDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException(exception));

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Activity activity = Assert.Single(capturedActivities);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Contains(metricMeasurements, measurement => measurement.InstrumentName == "lumina.application.handler.duration" && measurement.Outcome == "failure");
    }
}
