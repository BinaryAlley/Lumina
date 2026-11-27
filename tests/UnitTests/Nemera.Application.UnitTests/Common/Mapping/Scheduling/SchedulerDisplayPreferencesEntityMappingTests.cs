#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="SchedulerDisplayPreferencesEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SchedulerDisplayPreferencesEntityMappingTests
{
    private readonly SchedulerDisplayPreferencesEntityFixture _schedulerDisplayPreferencesEntityFixture = new();

    [Fact]
    public void ToResponse_WhenRepositoryEntityExists_ShouldMapToResponse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        SchedulerDisplayPreferencesEntity repositoryEntity = _schedulerDisplayPreferencesEntityFixture.Create(
            userId: userId,
            jobTypeFilter: ScheduledTaskType.CleanTemporaryFiles,
            displayTimeSpan: 30,
            displayTimeUnit: SchedulerDisplayTimeUnit.Minutes);

        // Act
        SchedulerDisplayPreferencesResponse result = repositoryEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(ScheduledTaskType.CleanTemporaryFiles, result.JobTypeFilter);
        Assert.Equal(30, result.DisplayTimeSpan);
        Assert.Equal(SchedulerDisplayTimeUnit.Minutes, result.DisplayTimeUnit);
    }

    [Fact]
    public void ToDefaultResponse_WhenRepositoryEntityIsNull_ShouldReturnDefaultResponseForTheUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        SchedulerDisplayPreferencesResponse result = ((SchedulerDisplayPreferencesEntity?)null).ToDefaultResponse(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Null(result.JobTypeFilter);
        Assert.Equal(10, result.DisplayTimeSpan);
        Assert.Equal(SchedulerDisplayTimeUnit.Minutes, result.DisplayTimeUnit);
    }
}
