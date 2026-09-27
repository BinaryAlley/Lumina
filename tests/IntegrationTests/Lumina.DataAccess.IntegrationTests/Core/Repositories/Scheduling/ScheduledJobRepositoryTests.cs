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
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Scheduling;

/// <summary>
/// Contains integration tests for the <see cref="ScheduledJobRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobRepositoryTests
{
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();

    [Fact]
    public async Task UpdateAsync_WhenAnEditableValueChanges_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-scheduledjobrepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ScheduledJobRepository sut = new(context);

        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        ScheduledJobEntity storedJob = _scheduledJobEntityFixture.Create(name: "Original Job");
        storedJob.CreatedOnUtc = storedCreatedOnUtc;
        storedJob.CreatedBy = storedCreatedBy;
        context.ScheduledJobs.Add(storedJob);
        await context.SaveChangesAsync();

        ScheduledJobEntity updatedJob = _scheduledJobEntityFixture.Create(
            id: storedJob.Id,
            name: "Updated Job",
            taskType: storedJob.TaskType,
            scheduleType: storedJob.ScheduleType,
            intervalMinutes: storedJob.IntervalMinutes,
            hour: storedJob.Hour,
            minute: storedJob.Minute,
            status: storedJob.Status,
            ownerUserId: storedJob.OwnerUserId,
            lastStartedOnUtc: storedJob.LastStartedOnUtc,
            lastCompletedOnUtc: storedJob.LastCompletedOnUtc);
        updatedJob.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        updatedJob.CreatedBy = Guid.NewGuid();
        updatedJob.UpdatedOnUtc = DateTime.UtcNow.AddYears(-5);
        updatedJob.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await sut.UpdateAsync(updatedJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        ScheduledJobEntity reloadedJob = await context.ScheduledJobs.AsNoTracking().FirstAsync(job => job.Id == storedJob.Id);
        Assert.Equal("Updated Job", reloadedJob.Name);
        Assert.Equal(storedCreatedOnUtc, reloadedJob.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, reloadedJob.CreatedBy);
        Assert.Null(reloadedJob.UpdatedOnUtc);
        Assert.Null(reloadedJob.UpdatedBy);
    }
}
