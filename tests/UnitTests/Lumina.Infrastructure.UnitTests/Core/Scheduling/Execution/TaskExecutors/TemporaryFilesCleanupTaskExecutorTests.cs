#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Contains unit tests for the <see cref="TemporaryFilesCleanupTaskExecutor"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TemporaryFilesCleanupTaskExecutorTests
{
    private readonly TemporaryFilesCleanupTaskExecutor _sut;
    private readonly ScheduledJobFixture _scheduledJobFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryFilesCleanupTaskExecutorTests"/> class.
    /// </summary>
    public TemporaryFilesCleanupTaskExecutorTests()
    {
        ILogger<TemporaryFilesCleanupTaskExecutor> logger = Substitute.For<ILogger<TemporaryFilesCleanupTaskExecutor>>();
        _sut = new TemporaryFilesCleanupTaskExecutor(logger);
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenTemporaryDirectoryExists_ShouldEmptyItAndReturnSuccess()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        string temporaryDirectory = Path.Combine(AppContext.BaseDirectory, "reading-cache");
        Directory.CreateDirectory(Path.Combine(temporaryDirectory, "subdirectory"));
        File.WriteAllText(Path.Combine(temporaryDirectory, "subdirectory", "file.txt"), "content");

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.True(Directory.Exists(temporaryDirectory));
        Assert.Empty(Directory.GetFileSystemEntries(temporaryDirectory));
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenTemporaryDirectoryDoesNotExist_ShouldRecreateItAndReturnSuccess()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        string temporaryDirectory = Path.Combine(AppContext.BaseDirectory, "reading-cache");
        if (Directory.Exists(temporaryDirectory))
            Directory.Delete(temporaryDirectory, recursive: true);

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.True(Directory.Exists(temporaryDirectory));
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenDeletingTheDirectoryFails_ShouldReturnError()
    {
        // On non Windows platforms deleting a file that is locked open succeeds, so the failure path can only be forced on Windows.
        if (!OperatingSystem.IsWindows())
            return;

        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create();
        string temporaryDirectory = Path.Combine(AppContext.BaseDirectory, "reading-cache");
        Directory.CreateDirectory(temporaryDirectory);
        string lockedFile = Path.Combine(temporaryDirectory, "locked.txt");
        File.WriteAllText(lockedFile, "content");
        // Lock the file with an exclusive handle so the recursive delete fails.
        using FileStream lockStream = new(lockedFile, FileMode.Open, FileAccess.Read, FileShare.None);

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }
}
