#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.DataAccess.Core.Repositories.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly ScheduledJobRepository _sut;
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobRepositoryTests"/> class.
    /// </summary>
    public ScheduledJobRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new ScheduledJobRepository(_mockContext);
    }

    [Fact]
    public async Task GetByIdAsync_WhenScheduledJobExists_ShouldReturnScheduledJob()
    {
        // Arrange
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create();
        _mockContext.ScheduledJobs.Add(scheduledJob);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ScheduledJobEntity?> result = await _sut.GetByIdAsync(scheduledJob.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(scheduledJob.Id, result.Value!.Id);
        Assert.Equal(scheduledJob.Name, result.Value.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenScheduledJobDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<ScheduledJobEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdWithoutTrackingAsync_WhenScheduledJobExists_ShouldReturnScheduledJob()
    {
        // Arrange
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create();
        _mockContext.ScheduledJobs.Add(scheduledJob);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ScheduledJobEntity?> result = await _sut.GetByIdWithoutTrackingAsync(scheduledJob.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(scheduledJob.Id, result.Value!.Id);
        Assert.Equal(scheduledJob.Name, result.Value.Name);
    }

    [Fact]
    public async Task GetByIdWithoutTrackingAsync_WhenScheduledJobDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<ScheduledJobEntity?> result = await _sut.GetByIdWithoutTrackingAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetAllAsync_WhenScheduledJobsExist_ShouldReturnAllScheduledJobs()
    {
        // Arrange
        ScheduledJobEntity scheduledJob1 = _scheduledJobEntityFixture.Create();
        ScheduledJobEntity scheduledJob2 = _scheduledJobEntityFixture.Create();
        _mockContext.ScheduledJobs.AddRange(scheduledJob1, scheduledJob2);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<ScheduledJobEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task GetActiveOrRunningAsync_WhenScheduledJobsAreActiveOrRunning_ShouldReturnOnlyThem()
    {
        // Arrange
        ScheduledJobEntity activeJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Active);
        ScheduledJobEntity runningJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Running);
        ScheduledJobEntity addedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added);
        ScheduledJobEntity completedJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Completed);
        _mockContext.ScheduledJobs.AddRange(activeJob, runningJob, addedJob, completedJob);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<ScheduledJobEntity>> result = await _sut.GetActiveOrRunningAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        List<ScheduledJobEntity> scheduledJobs = result.Value.ToList();
        Assert.Equal(2, scheduledJobs.Count);
        Assert.Contains(scheduledJobs, scheduledJob => scheduledJob.Id == activeJob.Id);
        Assert.Contains(scheduledJobs, scheduledJob => scheduledJob.Id == runningJob.Id);
    }

    [Fact]
    public async Task InsertAsync_WhenScheduledJobDoesNotExist_ShouldAddScheduledJobToContextAndReturnCreated()
    {
        // Arrange
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        EntityEntry<ScheduledJobEntity>? addedScheduledJob = _mockContext.ChangeTracker.Entries<ScheduledJobEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == scheduledJob.Id);
        Assert.NotNull(addedScheduledJob);
        Assert.Equal(scheduledJob.Name, addedScheduledJob!.Entity.Name);
    }

    [Fact]
    public async Task InsertAsync_WhenScheduledJobAlreadyExists_ShouldReturnError()
    {
        // Arrange
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create();
        _mockContext.ScheduledJobs.Add(scheduledJob);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Created> result = await _sut.InsertAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenScheduledJobExists_ShouldUpdateItsPropertiesAndReturnUpdated()
    {
        // Arrange
        ScheduledJobEntity existingScheduledJob = _scheduledJobEntityFixture.Create(status: ScheduledJobStatus.Added);
        _mockContext.ScheduledJobs.Add(existingScheduledJob);
        await _mockContext.SaveChangesAsync();

        ScheduledJobEntity updatedScheduledJob = _scheduledJobEntityFixture.Create(
            id: existingScheduledJob.Id,
            name: "Updated name",
            status: ScheduledJobStatus.Active);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedScheduledJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        ScheduledJobEntity? modifiedScheduledJob = await _mockContext.ScheduledJobs.FirstOrDefaultAsync(scheduledJob => scheduledJob.Id == existingScheduledJob.Id);
        Assert.NotNull(modifiedScheduledJob);
        Assert.Equal("Updated name", modifiedScheduledJob!.Name);
        Assert.Equal(ScheduledJobStatus.Active, modifiedScheduledJob.Status);
    }

    [Fact]
    public async Task UpdateAsync_WhenScheduledJobDoesNotExist_ShouldReturnError()
    {
        // Arrange
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobNotFound, result.FirstError);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenScheduledJobExists_ShouldRemoveItAndReturnDeleted()
    {
        // Arrange
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create();
        _mockContext.ScheduledJobs.Add(scheduledJob);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(scheduledJob.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        EntityEntry<ScheduledJobEntity>? deletedScheduledJob = _mockContext.ChangeTracker.Entries<ScheduledJobEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Deleted && entry.Entity.Id == scheduledJob.Id);
        Assert.NotNull(deletedScheduledJob);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenScheduledJobDoesNotExist_ShouldReturnError()
    {
        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobNotFound, result.FirstError);
    }
}
