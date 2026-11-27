#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Scheduling;
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Nemera.Contracts.Responses.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobExecutionEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionEntityMappingTests
{
    private readonly ScheduledJobExecutionEntityFixture _scheduledJobExecutionEntityFixture = new();

    [Fact]
    public void ToResponse_WhenRepositoryEntityIsCompleted_ShouldMapToResponse()
    {
        // Arrange
        DateTime startedOnUtc = DateTime.UtcNow.AddMinutes(-10);
        DateTime completedOnUtc = DateTime.UtcNow;
        ScheduledJobExecutionEntity repositoryEntity = _scheduledJobExecutionEntityFixture.Create(
            scheduledJobId: Guid.NewGuid(),
            isCycleRun: true,
            startedOnUtc: startedOnUtc,
            completedOnUtc: completedOnUtc);

        // Act
        ScheduledJobExecutionResponse result = repositoryEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(repositoryEntity.Id, result.Id);
        Assert.Equal(repositoryEntity.ScheduledJobId, result.ScheduledJobId);
        Assert.Equal(repositoryEntity.TaskType, result.TaskType);
        Assert.True(result.IsCycleRun);
        Assert.Equal(startedOnUtc, result.StartedOnUtc);
        Assert.Equal(completedOnUtc, result.CompletedOnUtc);
    }

    [Fact]
    public void ToResponse_WhenRepositoryEntityIsStillRunning_ShouldMapToResponseWithNullCompletionTime()
    {
        // Arrange
        ScheduledJobExecutionEntity repositoryEntity = _scheduledJobExecutionEntityFixture.Create(
            isCycleRun: false,
            completedOnUtc: null);

        // Act
        ScheduledJobExecutionResponse result = repositoryEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(repositoryEntity.Id, result.Id);
        Assert.False(result.IsCycleRun);
        Assert.Null(result.CompletedOnUtc);
    }
}
