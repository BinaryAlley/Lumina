#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Repositories.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Scheduling;

/// <summary>
/// Contains integration tests for the <see cref="ScheduledJobExecutionRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionRepositoryTests
{
    private readonly ScheduledJobExecutionEntityFixture _scheduledJobExecutionEntityFixture = new();

    [Fact]
    public async Task DeleteOlderThanAsync_WhenExecutionsStartedBeforeTheCutoff_ShouldDeleteOnlyThem()
    {
        // Arrange
        // The deletion uses ExecuteDeleteAsync, which is not supported by the in-memory provider, so a real SQLite database is used.
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-scheduledjobexecutionrepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ScheduledJobExecutionRepository sut = new(context);

        DateTime cutoffUtc = DateTime.UtcNow.AddMonths(-1);
        ScheduledJobExecutionEntity olderExecution = _scheduledJobExecutionEntityFixture.Create(startedOnUtc: cutoffUtc.AddDays(-30));
        ScheduledJobExecutionEntity recentExecution = _scheduledJobExecutionEntityFixture.Create(startedOnUtc: cutoffUtc.AddDays(1));
        context.ScheduledJobExecutions.AddRange(olderExecution, recentExecution);
        await context.SaveChangesAsync();

        // Act
        Result<Success> result = await sut.DeleteOlderThanAsync(cutoffUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        int remainingCount = await context.ScheduledJobExecutions.CountAsync();
        Assert.Equal(1, remainingCount);
        Assert.Null(await context.ScheduledJobExecutions.FirstOrDefaultAsync(execution => execution.Id == olderExecution.Id));
        Assert.NotNull(await context.ScheduledJobExecutions.FirstOrDefaultAsync(execution => execution.Id == recentExecution.Id));
    }

    [Fact]
    public async Task DeleteOlderThanAsync_WhenNoExecutionStartedBeforeTheCutoff_ShouldNotDeleteAnything()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-scheduledjobexecutionrepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ScheduledJobExecutionRepository sut = new(context);

        DateTime cutoffUtc = DateTime.UtcNow.AddMonths(-1);
        ScheduledJobExecutionEntity recentExecution = _scheduledJobExecutionEntityFixture.Create(startedOnUtc: cutoffUtc.AddDays(1));
        context.ScheduledJobExecutions.Add(recentExecution);
        await context.SaveChangesAsync();

        // Act
        Result<Success> result = await sut.DeleteOlderThanAsync(cutoffUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1, await context.ScheduledJobExecutions.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_WhenAnEditableValueChanges_ShouldPreserveTheCreationColumnsAndStampTheModificationColumns()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-scheduledjobexecutionrepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
        Guid actingUserId = Guid.NewGuid();
        currentUserService.UserId.Returns(actingUserId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        DateTime insertTimeUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        dateTimeProvider.UtcNow.Returns(insertTimeUtc);
        // The context is built with the auditing interceptor attached, so an interceptor regression is caught instead of being masked by its absence.
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .AddInterceptors(new UpdateAuditableEntitiesInterceptor(currentUserService, dateTimeProvider))
            .Options);
        context.Database.EnsureCreated();
        ScheduledJobExecutionRepository sut = new(context);

        ScheduledJobExecutionEntity storedExecution = _scheduledJobExecutionEntityFixture.Create(isCycleRun: false);
        context.ScheduledJobExecutions.Add(storedExecution);
        await context.SaveChangesAsync();

        // The interceptor stamps the creation columns on insert.
        Assert.Equal(insertTimeUtc, storedExecution.CreatedOnUtc);
        Assert.Equal(actingUserId, storedExecution.CreatedBy);

        DateTime updateTimeUtc = insertTimeUtc.AddDays(1);
        dateTimeProvider.UtcNow.Returns(updateTimeUtc);

        ScheduledJobExecutionEntity updatedExecution = _scheduledJobExecutionEntityFixture.Create(
            id: storedExecution.Id,
            scheduledJobId: storedExecution.ScheduledJobId,
            taskType: storedExecution.TaskType,
            isCycleRun: true,
            wasCycleActive: storedExecution.WasCycleActive,
            startedOnUtc: storedExecution.StartedOnUtc,
            completedOnUtc: DateTime.UtcNow);
        // The incoming entity carries hostile audit values that must never win over the ones owned by the persistence medium.
        updatedExecution.CreatedOnUtc = insertTimeUtc.AddYears(-10);
        updatedExecution.CreatedBy = Guid.NewGuid();
        updatedExecution.UpdatedOnUtc = insertTimeUtc.AddYears(-5);
        updatedExecution.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await sut.UpdateAsync(updatedExecution, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        ScheduledJobExecutionEntity reloadedExecution = await context.ScheduledJobExecutions.AsNoTracking().FirstAsync(execution => execution.Id == storedExecution.Id);
        Assert.True(reloadedExecution.IsCycleRun);
        // The interceptor owns the modification columns and stamped them with the acting user and time, not with the incoming values.
        Assert.Equal(updateTimeUtc, reloadedExecution.UpdatedOnUtc);
        Assert.Equal(actingUserId, reloadedExecution.UpdatedBy);
        // The creation columns are still the ones stamped on insert.
        Assert.Equal(insertTimeUtc, reloadedExecution.CreatedOnUtc);
        Assert.Equal(actingUserId, reloadedExecution.CreatedBy);
    }
}
