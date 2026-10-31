#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Controllable test double for the <see cref="IMediaLibraryScanDiscoveryJob"/> interface.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class FakeMediaLibraryScanDiscoveryJob : IMediaLibraryScanDiscoveryJob
{
    private readonly TaskCompletionSource _executionStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _executionCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Gets or sets the Id of the media library scan that this job is part of.
    /// </summary>
    public required ScanId ScanId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that initiated the media library scan that this job is part of.
    /// </summary>
    public required UserId UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the media library upon which the scan is performed.
    /// </summary>
    public required LibraryId LibraryId { get; set; }

    /// <summary>
    /// Gets the list of child media scan jobs that will be triggered when this job's payload completes execution.
    /// </summary>
    public List<IMediaLibraryScanJob> Children { get; } = [];

    /// <summary>
    /// Gets the list of parent media scan jobs that will trigger this job when their payload will complete execution.
    /// </summary>
    public List<IMediaLibraryScanJob> Parents { get; } = [];

    /// <summary>
    /// Gets the status of the current media library scan job.
    /// </summary>
    public LibraryScanJobStatus Status { get; private set; } = LibraryScanJobStatus.Pending;

    /// <summary>
    /// Gets the number of times this job was executed.
    /// </summary>
    public int ExecutionCount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this job is currently executing.
    /// </summary>
    public bool IsExecuting { get; private set; }

    /// <summary>
    /// Gets the Id this job was last executed with.
    /// </summary>
    public Guid? LastExecutionId { get; private set; }

    /// <summary>
    /// Gets the cancellation token this job was last executed with.
    /// </summary>
    public CancellationToken LastCancellationToken { get; private set; }

    /// <summary>
    /// Gets or sets the optional callback invoked while the job executes, allowing a test to block or throw.
    /// </summary>
    public Func<CancellationToken, Task>? OnExecute { get; set; }

    /// <summary>
    /// Gets a task that completes as soon as the job starts executing.
    /// </summary>
    public Task ExecutionStarted => _executionStarted.Task;

    /// <summary>
    /// Gets a task that completes as soon as the job finishes executing successfully.
    /// </summary>
    public Task ExecutionCompleted => _executionCompleted.Task;

    /// <summary>
    /// Adds a media library scan job to the list of child media library scan jobs that will be triggered when this job's payload completes execution.
    /// </summary>
    /// <param name="job">The child media library scan job to be added.</param>
    public void AddChild(IMediaLibraryScanJob job)
    {
        if (!Children.Contains(job) && !Parents.Contains(job))
            Children.Add(job);
    }

    /// <summary>
    /// Adds a media library scan job to the list of parent media library scan jobs that will trigger this job when their payload will complete execution.
    /// </summary>
    /// <param name="job">The child media library scan job to be added.</param>
    public void AddParent(IMediaLibraryScanJob job)
    {
        if (!Parents.Contains(job) && !Children.Contains(job))
            Parents.Add(job);
    }

    /// <summary>
    /// Executes the payload of the media library scan job.
    /// </summary>
    /// <typeparam name="TInput">The type of the input parameter representing the data to be processed by this payload.</typeparam>
    /// <param name="id">The id of the media library scan job.</param>
    /// <param name="input">The input data to be processed by this payload.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
    {
        ExecutionCount++;
        LastExecutionId = id;
        LastCancellationToken = cancellationToken;
        IsExecuting = true;
        Status = LibraryScanJobStatus.Running;
        _executionStarted.TrySetResult();

        if (OnExecute is not null)
            await OnExecute(cancellationToken).ConfigureAwait(false);

        Status = LibraryScanJobStatus.Completed;
        IsExecuting = false;
        _executionCompleted.TrySetResult();
    }
}
