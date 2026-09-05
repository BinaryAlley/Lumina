#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.DataAccess.Core.Repositories.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
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
/// Contains unit tests for the <see cref="ScheduledJobExecutionRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly ScheduledJobExecutionRepository _sut;
    private readonly ScheduledJobExecutionEntityFixture _scheduledJobExecutionEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobExecutionRepositoryTests"/> class.
    /// </summary>
    public ScheduledJobExecutionRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new ScheduledJobExecutionRepository(_mockContext);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExecutionExists_ShouldReturnExecution()
    {
        // Arrange
        ScheduledJobExecutionEntity execution = _scheduledJobExecutionEntityFixture.Create();
        _mockContext.ScheduledJobExecutions.Add(execution);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ScheduledJobExecutionEntity?> result = await _sut.GetByIdAsync(execution.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(execution.Id, result.Value!.Id);
        Assert.Equal(execution.ScheduledJobId, result.Value.ScheduledJobId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExecutionDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<ScheduledJobExecutionEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task InsertAsync_WhenExecutionDoesNotExist_ShouldAddExecutionToContextAndReturnCreated()
    {
        // Arrange
        ScheduledJobExecutionEntity execution = _scheduledJobExecutionEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(execution, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        EntityEntry<ScheduledJobExecutionEntity>? addedExecution = _mockContext.ChangeTracker.Entries<ScheduledJobExecutionEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == execution.Id);
        Assert.NotNull(addedExecution);
    }

    [Fact]
    public async Task InsertAsync_WhenExecutionAlreadyExists_ShouldReturnError()
    {
        // Arrange
        ScheduledJobExecutionEntity execution = _scheduledJobExecutionEntityFixture.Create();
        _mockContext.ScheduledJobExecutions.Add(execution);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Created> result = await _sut.InsertAsync(execution, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobExecutionAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenExecutionExists_ShouldUpdateItsPropertiesAndReturnUpdated()
    {
        // Arrange
        DateTime completedOnUtc = DateTime.UtcNow;
        ScheduledJobExecutionEntity existingExecution = _scheduledJobExecutionEntityFixture.Create(completedOnUtc: null);
        _mockContext.ScheduledJobExecutions.Add(existingExecution);
        await _mockContext.SaveChangesAsync();

        ScheduledJobExecutionEntity updatedExecution = _scheduledJobExecutionEntityFixture.Create(
            id: existingExecution.Id,
            scheduledJobId: existingExecution.ScheduledJobId,
            isCycleRun: existingExecution.IsCycleRun,
            startedOnUtc: existingExecution.StartedOnUtc,
            completedOnUtc: completedOnUtc);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedExecution, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        ScheduledJobExecutionEntity? modifiedExecution = await _mockContext.ScheduledJobExecutions.FirstOrDefaultAsync(execution => execution.Id == existingExecution.Id);
        Assert.NotNull(modifiedExecution);
        Assert.Equal(completedOnUtc, modifiedExecution!.CompletedOnUtc);
    }

    [Fact]
    public async Task UpdateAsync_WhenExecutionDoesNotExist_ShouldReturnError()
    {
        // Arrange
        ScheduledJobExecutionEntity execution = _scheduledJobExecutionEntityFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(execution, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobExecutionNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetOpenByScheduledJobIdAsync_WhenAnOpenExecutionExists_ShouldReturnTheMostRecentOne()
    {
        // Arrange
        Guid scheduledJobId = Guid.NewGuid();
        DateTime mostRecentStart = DateTime.UtcNow.AddMinutes(-1);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: scheduledJobId, startedOnUtc: mostRecentStart, completedOnUtc: null);
        ScheduledJobExecutionEntity olderOpenExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: scheduledJobId, startedOnUtc: mostRecentStart.AddMinutes(-10), completedOnUtc: null);
        ScheduledJobExecutionEntity closedExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: scheduledJobId, startedOnUtc: mostRecentStart.AddMinutes(-5), completedOnUtc: DateTime.UtcNow);
        _mockContext.ScheduledJobExecutions.AddRange(openExecution, olderOpenExecution, closedExecution);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ScheduledJobExecutionEntity?> result = await _sut.GetOpenByScheduledJobIdAsync(scheduledJobId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(openExecution.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetOpenByScheduledJobIdAsync_WhenNoOpenExecutionExists_ShouldReturnNull()
    {
        // Arrange
        Guid scheduledJobId = Guid.NewGuid();
        ScheduledJobExecutionEntity closedExecution = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: scheduledJobId, completedOnUtc: DateTime.UtcNow);
        _mockContext.ScheduledJobExecutions.Add(closedExecution);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ScheduledJobExecutionEntity?> result = await _sut.GetOpenByScheduledJobIdAsync(scheduledJobId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByTimeRangeAsync_WhenExecutionsStartedInTheRange_ShouldReturnThemOrderedByStartTime()
    {
        // Arrange
        DateTime fromUtc = DateTime.UtcNow.AddHours(-2);
        DateTime toUtc = DateTime.UtcNow;
        ScheduledJobExecutionEntity inRange1 = _scheduledJobExecutionEntityFixture.Create(startedOnUtc: fromUtc.AddMinutes(1));
        ScheduledJobExecutionEntity inRange2 = _scheduledJobExecutionEntityFixture.Create(startedOnUtc: fromUtc.AddMinutes(30));
        ScheduledJobExecutionEntity beforeRange = _scheduledJobExecutionEntityFixture.Create(startedOnUtc: fromUtc.AddMinutes(-10));
        ScheduledJobExecutionEntity afterRange = _scheduledJobExecutionEntityFixture.Create(startedOnUtc: toUtc.AddMinutes(10));
        _mockContext.ScheduledJobExecutions.AddRange(inRange1, inRange2, beforeRange, afterRange);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<ScheduledJobExecutionEntity>> result = await _sut.GetByTimeRangeAsync(fromUtc, toUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        List<ScheduledJobExecutionEntity> executions = [.. result.Value];
        Assert.Equal(2, executions.Count);
        Assert.Equal(inRange1.Id, executions[0].Id);
        Assert.Equal(inRange2.Id, executions[1].Id);
    }

    [Fact]
    public async Task DeleteByScheduledJobIdAsync_WhenExecutionsExist_ShouldRemoveThemAllAndReturnSuccess()
    {
        // Arrange
        Guid scheduledJobId = Guid.NewGuid();
        ScheduledJobExecutionEntity execution1 = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: scheduledJobId);
        ScheduledJobExecutionEntity execution2 = _scheduledJobExecutionEntityFixture.Create(scheduledJobId: scheduledJobId);
        ScheduledJobExecutionEntity otherExecution = _scheduledJobExecutionEntityFixture.Create();
        _mockContext.ScheduledJobExecutions.AddRange(execution1, execution2, otherExecution);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Success> result = await _sut.DeleteByScheduledJobIdAsync(scheduledJobId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(2, _mockContext.ChangeTracker.Entries<ScheduledJobExecutionEntity>().Count(entry => entry.State == EntityState.Deleted));
        Assert.Contains(_mockContext.ChangeTracker.Entries<ScheduledJobExecutionEntity>(), entry => entry.State == EntityState.Deleted && entry.Entity.Id == execution1.Id);
        Assert.Contains(_mockContext.ChangeTracker.Entries<ScheduledJobExecutionEntity>(), entry => entry.State == EntityState.Deleted && entry.Entity.Id == execution2.Id);
    }

    [Fact]
    public async Task DeleteByScheduledJobIdAsync_WhenNoExecutionsExist_ShouldReturnSuccess()
    {
        // Act
        Result<Success> result = await _sut.DeleteByScheduledJobIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
    }
}
