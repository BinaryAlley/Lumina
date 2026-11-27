#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Commands.StopScheduledJob;
using Nemera.Contracts.Fixtures.Core.Requests.Scheduling;
using Nemera.Contracts.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="StopScheduledJobRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopScheduledJobRequestMappingTests
{
    private readonly StopScheduledJobRequestFixture _stopScheduledJobRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid scheduledJobId = Guid.NewGuid();
        StopScheduledJobRequest request = _stopScheduledJobRequestFixture.Create(scheduledJobId);

        // Act
        StopScheduledJobCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ScheduledJobId, result.ScheduledJobId);
    }
}
