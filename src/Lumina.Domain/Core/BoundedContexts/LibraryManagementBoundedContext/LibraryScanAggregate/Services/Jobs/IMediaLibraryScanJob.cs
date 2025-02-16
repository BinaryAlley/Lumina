#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Interface for the abstract base class defining a media library scan job.
/// </summary>
public interface IMediaLibraryScanJob
{
    /// <summary>
    /// Gets or sets the Id of the media library scan that this job is part of.
    /// </summary>
    ScanId ScanId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that initiated the media library scan that this job is part of.
    /// </summary>
    UserId UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the media library upon which the scan is performed.
    /// </summary>
    LibraryId LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the list of child media scan jobs that will be triggered when this job's payload completes execution.
    /// </summary>
    List<IMediaLibraryScanJob> Children { get; }

    /// <summary>
    /// Gets or sets the list of parent media scan jobs that will trigger this job when their payload will complete execution.
    /// </summary>
    List<IMediaLibraryScanJob> Parents { get; }

    /// <summary>
    /// Gets or sets the status of the current media library scan job.
    /// </summary>
    LibraryScanJobStatus Status { get; }

    /// <summary>
    /// Adds a media library scan job to the list of child media library scan jobs that will be triggered when this job's payload completes execution.
    /// </summary>
    /// <param name="job">The child media library scan job to be added.</param>
    void AddChild(IMediaLibraryScanJob job);

    /// <summary>
    /// Adds a media library scan job to the list of parent media library scan jobs that will trigger this job when their payload will complete execution.
    /// </summary>
    /// <param name="job">The child media library scan job to be added.</param>
    void AddParent(IMediaLibraryScanJob job);

    /// <summary>
    /// Executes the payload of the media library scan job.
    /// </summary>
    /// <typeparam name="TInput">The type of the input parameter representing the data to be processed by this payload.</typeparam>
    /// <param name="id">The id of the media library scan job.</param>
    /// <param name="input">The input data to be processed by this payload.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken);
}
