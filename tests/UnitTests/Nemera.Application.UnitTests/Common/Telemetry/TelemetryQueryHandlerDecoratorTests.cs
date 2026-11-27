#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Telemetry;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Lumina.Application.Fixtures.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Lumina.Contracts.Fixtures.Core.Responses.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
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
/// Contains unit tests for the <see cref="TelemetryQueryHandlerDecorator{TQuery,TResult}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TelemetryQueryHandlerDecoratorTests
{
    private readonly IQueryHandler<GetRolePermissionsQuery, Result<RolePermissionsResponse>> _mockInnerHandler;
    private readonly TelemetryQueryHandlerDecorator<GetRolePermissionsQuery, Result<RolePermissionsResponse>> _sut;
    private readonly GetRolePermissionsQueryFixture _getRolePermissionsQueryFixture = new();
    private readonly RolePermissionsResponseFixture _rolePermissionsResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryQueryHandlerDecoratorTests"/> class.
    /// </summary>
    public TelemetryQueryHandlerDecoratorTests()
    {
        _mockInnerHandler = Substitute.For<IQueryHandler<GetRolePermissionsQuery, Result<RolePermissionsResponse>>>();
        ILogger<TelemetryQueryHandlerDecorator<GetRolePermissionsQuery, Result<RolePermissionsResponse>>> mockLogger =
            Substitute.For<ILogger<TelemetryQueryHandlerDecorator<GetRolePermissionsQuery, Result<RolePermissionsResponse>>>>();
        _sut = new TelemetryQueryHandlerDecorator<GetRolePermissionsQuery, Result<RolePermissionsResponse>>(
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
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.Create();
        Result<RolePermissionsResponse> expectedResult = Result<RolePermissionsResponse>.Success(_rolePermissionsResponseFixture.Create());
        _mockInnerHandler.HandleAsync(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        await _mockInnerHandler.Received(1).HandleAsync(query, CancellationToken.None);
        Activity activity = Assert.Single(capturedActivities);
        Assert.Equal($"Handle {_mockInnerHandler.GetType().Name}", activity.OperationName);
        Assert.Equal("query", activity.GetTagItem("lumina.handler.type"));
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Contains(metricMeasurements, measurement => measurement.InstrumentName == "lumina.application.handler.duration" && measurement.Outcome == "success");
        Assert.Contains(metricMeasurements, measurement => measurement.InstrumentName == "lumina.application.handler.invocations" && measurement.Outcome == "success");
    }

    [Fact]
    public async Task HandleAsync_WhenInnerHandlerReturnsFailedResult_ShouldReturnResultAndEmitFailureTraceAndMetric()
    {
        // Arrange
        List<Activity> capturedActivities = [];
        using ActivityListener activityListener = TelemetryTestHelpers.CreateActivityListener(capturedActivities);
        List<(string InstrumentName, double Value, string Outcome)> metricMeasurements = [];
        using MeterListener meterListener = TelemetryTestHelpers.CreateMeterListener(metricMeasurements);
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.Create();
        Result<RolePermissionsResponse> failedResult = Error.Failure("Test.Failed", "Test failure description");
        _mockInnerHandler.HandleAsync(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(failedResult);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(failedResult, result);
        Activity activity = Assert.Single(capturedActivities);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Contains(metricMeasurements, measurement => measurement.InstrumentName == "lumina.application.handler.duration" && measurement.Outcome == "failure");
        Assert.Contains(metricMeasurements, measurement => measurement.InstrumentName == "lumina.application.handler.invocations" && measurement.Outcome == "failure");
    }

    [Fact]
    public async Task HandleAsync_WhenInnerHandlerThrows_ShouldRethrowAndEmitFailureTraceAndMetric()
    {
        // Arrange
        List<Activity> capturedActivities = [];
        using ActivityListener activityListener = TelemetryTestHelpers.CreateActivityListener(capturedActivities);
        List<(string InstrumentName, double Value, string Outcome)> metricMeasurements = [];
        using MeterListener meterListener = TelemetryTestHelpers.CreateMeterListener(metricMeasurements);
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.Create();
        InvalidOperationException exception = new("boom");
        _mockInnerHandler.HandleAsync(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<RolePermissionsResponse>>(exception));

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.HandleAsync(query, CancellationToken.None));

        // Assert
        Activity activity = Assert.Single(capturedActivities);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Contains(metricMeasurements, measurement => measurement.InstrumentName == "lumina.application.handler.duration" && measurement.Outcome == "failure");
    }
}
