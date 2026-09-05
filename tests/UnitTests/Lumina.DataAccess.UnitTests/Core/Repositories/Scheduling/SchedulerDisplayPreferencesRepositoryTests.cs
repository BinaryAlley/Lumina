#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.DataAccess.Core.Repositories.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="SchedulerDisplayPreferencesRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SchedulerDisplayPreferencesRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly SchedulerDisplayPreferencesRepository _sut;
    private readonly SchedulerDisplayPreferencesEntityFixture _schedulerDisplayPreferencesEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerDisplayPreferencesRepositoryTests"/> class.
    /// </summary>
    public SchedulerDisplayPreferencesRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new SchedulerDisplayPreferencesRepository(_mockContext);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenDisplayPreferencesExist_ShouldReturnThem()
    {
        // Arrange
        SchedulerDisplayPreferencesEntity displayPreferences = _schedulerDisplayPreferencesEntityFixture.Create();
        _mockContext.SchedulerDisplayPreferences.Add(displayPreferences);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<SchedulerDisplayPreferencesEntity?> result = await _sut.GetByUserIdAsync(displayPreferences.UserId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(displayPreferences.Id, result.Value!.Id);
        Assert.Equal(displayPreferences.UserId, result.Value.UserId);
        Assert.Equal(displayPreferences.DisplayTimeSpan, result.Value.DisplayTimeSpan);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenDisplayPreferencesDoNotExist_ShouldReturnNull()
    {
        // Act
        Result<SchedulerDisplayPreferencesEntity?> result = await _sut.GetByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpsertAsync_WhenDisplayPreferencesDoNotExist_ShouldAddThemToContextAndReturnUpdated()
    {
        // Arrange
        SchedulerDisplayPreferencesEntity displayPreferences = _schedulerDisplayPreferencesEntityFixture.Create(
            displayTimeSpan: 30,
            displayTimeUnit: SchedulerDisplayTimeUnit.Minutes);

        // Act
        Result<Updated> result = await _sut.UpsertAsync(displayPreferences, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        EntityEntry<SchedulerDisplayPreferencesEntity>? addedPreferences = _mockContext.ChangeTracker.Entries<SchedulerDisplayPreferencesEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.UserId == displayPreferences.UserId);
        Assert.NotNull(addedPreferences);
    }

    [Fact]
    public async Task UpsertAsync_WhenDisplayPreferencesAlreadyExist_ShouldUpdateTheMutableFieldsAndReturnUpdated()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        SchedulerDisplayPreferencesEntity existingPreferences = _schedulerDisplayPreferencesEntityFixture.Create(
            userId: userId,
            jobTypeFilter: ScheduledTaskType.ScanMediaLibraries,
            displayTimeSpan: 10,
            displayTimeUnit: SchedulerDisplayTimeUnit.Hours);
        _mockContext.SchedulerDisplayPreferences.Add(existingPreferences);
        await _mockContext.SaveChangesAsync();

        SchedulerDisplayPreferencesEntity updatedPreferences = _schedulerDisplayPreferencesEntityFixture.Create(
            userId: userId,
            jobTypeFilter: ScheduledTaskType.CleanTemporaryFiles,
            displayTimeSpan: 60,
            displayTimeUnit: SchedulerDisplayTimeUnit.Days);

        // Act
        Result<Updated> result = await _sut.UpsertAsync(updatedPreferences, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        SchedulerDisplayPreferencesEntity? modifiedPreferences = await _mockContext.SchedulerDisplayPreferences
            .FirstOrDefaultAsync(preferences => preferences.UserId == userId);
        Assert.NotNull(modifiedPreferences);
        // The Id and the ownership of the existing preferences are preserved.
        Assert.Equal(existingPreferences.Id, modifiedPreferences!.Id);
        Assert.Equal(ScheduledTaskType.CleanTemporaryFiles, modifiedPreferences.JobTypeFilter);
        Assert.Equal(60, modifiedPreferences.DisplayTimeSpan);
        Assert.Equal(SchedulerDisplayTimeUnit.Days, modifiedPreferences.DisplayTimeUnit);
        Assert.Single(_mockContext.ChangeTracker.Entries<SchedulerDisplayPreferencesEntity>());
    }
}
