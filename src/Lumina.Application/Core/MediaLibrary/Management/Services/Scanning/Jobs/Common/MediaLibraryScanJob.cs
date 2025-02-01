#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;

/// <summary>
/// Abstract class defining a media library scan job.
/// </summary>
internal abstract class MediaLibraryScanJob
{
    internal int parentsPayloadsExecuted;
    
    /// <summary>
    /// Gets or sets the Id of the media library scan that this job is part of.
    /// </summary>
    public Guid ScanId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that initiated the media library scan that this job is part of.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the list of child media scan jobs that will be triggered when this job's payload completes execution.
    /// </summary>
    public List<MediaLibraryScanJob> Children { get; protected set; } = [];

    /// <summary>
    /// Gets or sets the list of parent media scan jobs that will trigger this job when their payload will complete execution.
    /// </summary>
    public List<MediaLibraryScanJob> Parents { get; protected set; } = [];

    /// <summary>
    /// Gets or sets the media library upon which the scan is performed.
    /// </summary>
    public required Library Library { get; set; }

    /// <summary>
    /// Gets or sets the status of the current media library scan job.
    /// </summary>
    public LibraryScanJobStatus Status { get; protected set; } = LibraryScanJobStatus.Pending;

    /// <summary>
    /// Adds a media library scan job to the list of child media library scan jobs that will be triggered when this job's payload completes execution.
    /// </summary>
    /// <param name="job">The child media library scan job to be added.</param>
    public void AddChild(MediaLibraryScanJob job)
    {
        if (!Children.Contains(job) && !Parents.Contains(job))
            Children.Add(job);
    }

    /// <summary>
    /// Adds a media library scan job to the list of parent media library scan jobs that will trigger this job when their payload will complete execution.
    /// </summary>
    /// <param name="job">The child media library scan job to be added.</param>
    public void AddParent(MediaLibraryScanJob job)
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
    public abstract Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken);
}
