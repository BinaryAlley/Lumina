#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Task executor that empties the temporary directory into which the reading plugins extract the contents of the books.
/// </summary>
public class TemporaryFilesCleanupTaskExecutor : IScheduledTaskExecutor
{
    private readonly ILogger<TemporaryFilesCleanupTaskExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryFilesCleanupTaskExecutor"/> class.
    /// </summary>
    /// <param name="logger">Injected service for logging.</param>
    public TemporaryFilesCleanupTaskExecutor(ILogger<TemporaryFilesCleanupTaskExecutor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Empties the temporary directory into which the reading plugins extract the contents of the books.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job whose task is executed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> ExecutePayloadAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken)
    {
        string temporaryDirectoryRoot = Path.GetFullPath(ReadingCachePaths.GetRootDirectory());
        string applicationBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        // Fail closed when the temporary directory could not be resolved inside the application base directory, so that a misconfiguration can never delete arbitrary directories.
        if (string.IsNullOrWhiteSpace(temporaryDirectoryRoot) ||
            string.Equals(temporaryDirectoryRoot, Path.GetPathRoot(temporaryDirectoryRoot), comparison) ||
            !temporaryDirectoryRoot.StartsWith(applicationBaseDirectory, comparison) ||
            string.Equals(temporaryDirectoryRoot, applicationBaseDirectory, comparison))
        {
            _logger.LogWarning("The temporary directory to clean could not be resolved safely, so the scheduled job '{ScheduledJobName}' was skipped.", scheduledJob.Name);
            return Result.Success;
        }

        try
        {
            if (Directory.Exists(temporaryDirectoryRoot))
                Directory.Delete(temporaryDirectoryRoot, recursive: true);
            Directory.CreateDirectory(temporaryDirectoryRoot);
            _logger.LogInformation("Emptied the temporary directory '{TemporaryDirectory}'.", temporaryDirectoryRoot);
            return Result.Success;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(exception, "The temporary directory '{TemporaryDirectory}' could not be emptied.", temporaryDirectoryRoot);
            return await Task.FromResult(Error.Failure(description: nameof(TemporaryFilesCleanupTaskExecutor)));
        }
    }
}
