#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Commands.RemoveScheduledJob;
using Nemera.Contracts.Fixtures.Core.Requests.Scheduling;
using Nemera.Contracts.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="RemoveScheduledJobRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobRequestMappingTests
{
    private readonly RemoveScheduledJobRequestFixture _removeScheduledJobRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid scheduledJobId = Guid.NewGuid();
        RemoveScheduledJobRequest request = _removeScheduledJobRequestFixture.Create(scheduledJobId);

        // Act
        RemoveScheduledJobCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ScheduledJobId, result.ScheduledJobId);
    }
}
