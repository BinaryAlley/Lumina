#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Commands.StartScheduledJob;
using Nemera.Contracts.Fixtures.Core.Requests.Scheduling;
using Nemera.Contracts.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="StartScheduledJobRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobRequestMappingTests
{
    private readonly StartScheduledJobRequestFixture _startScheduledJobRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid scheduledJobId = Guid.NewGuid();
        StartScheduledJobRequest request = _startScheduledJobRequestFixture.Create(scheduledJobId);

        // Act
        StartScheduledJobCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ScheduledJobId, result.ScheduledJobId);
    }
}
