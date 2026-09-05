#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.StopScheduledJob;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="StopScheduledJobRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopScheduledJobRequestMappingTests
{
    private readonly StopScheduledJobRequestFixture _requestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid scheduledJobId = Guid.NewGuid();
        StopScheduledJobRequest request = _requestFixture.Create(scheduledJobId);

        // Act
        StopScheduledJobCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ScheduledJobId, result.ScheduledJobId);
    }
}
