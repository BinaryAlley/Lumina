#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.RemoveScheduledJob;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="RemoveScheduledJobRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobRequestMappingTests
{
    private readonly RemoveScheduledJobRequestFixture _requestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid scheduledJobId = Guid.NewGuid();
        RemoveScheduledJobRequest request = _requestFixture.Create(scheduledJobId);

        // Act
        RemoveScheduledJobCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ScheduledJobId, result.ScheduledJobId);
    }
}
