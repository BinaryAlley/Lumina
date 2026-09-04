#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.StartScheduledJob;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="StartScheduledJobRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobRequestMappingTests
{
    private readonly StartScheduledJobRequestFixture _requestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid scheduledJobId = Guid.NewGuid();
        StartScheduledJobRequest request = _requestFixture.Create(scheduledJobId);

        // Act
        StartScheduledJobCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ScheduledJobId, result.ScheduledJobId);
    }
}
