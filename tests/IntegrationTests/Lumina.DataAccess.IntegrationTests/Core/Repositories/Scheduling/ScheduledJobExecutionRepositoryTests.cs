#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.DataAccess.Core.Repositories.Scheduling;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
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
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
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
}
