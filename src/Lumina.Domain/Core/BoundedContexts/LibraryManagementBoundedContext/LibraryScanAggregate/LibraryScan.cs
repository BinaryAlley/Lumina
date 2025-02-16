#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;

/// <summary>
/// Aggregate root for a media library scan.
/// </summary>
[DebuggerDisplay("Id: {Id}")]
public class LibraryScan : AggregateRoot<ScanId>
{
    private readonly List<LibraryScan> _pastScans = [];

    /// <summary>
    /// Gets the object representing the unique identifier of the media library that is scanned.
    /// </summary>
    public LibraryId LibraryId { get; private set; }

    /// <summary>
    /// Gets the object representing the unique identifier of the user initiating this media library scan.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets or sets the status of the current media library scan job.
    /// </summary>
    public LibraryScanJobStatus Status { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScan"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the media library scan.</param>
    /// <param name="libraryId">The object representing the unique identifier of the media library to be scanned.</param>
    /// <param name="userId">The object representing the unique identifier of the user initiating the media library scan.</param>
    /// <param name="status">The status of the media library scan.</param>
    /// <param name="pastScans">The list of past media library scans the library identified by <paramref name="libraryId"/> had.</param>
    private LibraryScan(ScanId id, LibraryId libraryId, UserId userId, LibraryScanJobStatus status, List<LibraryScan> pastScans) : base(id)
    {
        LibraryId = libraryId;
        UserId = userId;
        Status = status;
        _pastScans = pastScans;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LibraryScan"/> class.
    /// </summary>
    /// <param name="libraryId">The object representing the unique identifier of the media library to be scanned.</param>
    /// <param name="userId">The object representing the unique identifier of the user initiating the media library scan.</param>
    /// <param name="pastScans">The list of past media library scans the library identified by <paramref name="libraryId"/> had.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="LibraryScan"/>, or an error message.
    /// </returns>
    public static ErrorOr<LibraryScan> Create(LibraryId libraryId, UserId userId, List<LibraryScan> pastScans)
    {
        return new LibraryScan(ScanId.CreateUnique(), libraryId, userId, LibraryScanJobStatus.Pending, pastScans);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LibraryScan"/>, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the media library scan.</param>
    /// <param name="libraryId">The object representing the unique identifier of the media library to be scanned.</param>
    /// <param name="userId">The object representing the unique identifier of the user initiating the media library scan.</param>
    /// <param name="status">The status of the media library scan.</param>
    /// <param name="pastScans">The list of past media library scans the library identified by <paramref name="libraryId"/> had.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="LibraryScan"/>, or an error message.
    /// </returns>
    public static ErrorOr<LibraryScan> Create(ScanId id, LibraryId libraryId, UserId userId, LibraryScanJobStatus status, List<LibraryScan> pastScans)
    {
        return new LibraryScan(id, libraryId, userId, status, pastScans);
    }

    /// <summary>
    /// Queues a media library scan.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Success> QueueScan()
    {        
        // there must be no active scan for this library in the past 30 days
        if (_pastScans.Any(libraryScan => libraryScan.Status == LibraryScanJobStatus.Running || libraryScan.Status == LibraryScanJobStatus.Pending))
            return Errors.LibraryScanning.LibraryAlreadyBeingScanned;

        _domainEvents.Add(new LibraryScanQueuedDomainEvent(Guid.NewGuid(), Id, LibraryId, DateTime.UtcNow));

        return Result.Success;
    }

    /// <summary>
    /// Starts the media library scan.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Success> StartScan()
    {
        if (Status == LibraryScanJobStatus.Pending)
            Status = LibraryScanJobStatus.Running;
        else
            return Errors.LibraryScanning.CanOnlyStartPendingScans;

        // there must be no active scan for this library in the past 30 days
        if (_pastScans.Any(libraryScan => libraryScan.Status == LibraryScanJobStatus.Running || libraryScan.Status == LibraryScanJobStatus.Pending))
            return Errors.LibraryScanning.LibraryAlreadyBeingScanned;

        _domainEvents.Add(new LibraryScanStartedDomainEvent(Guid.NewGuid(), this, DateTime.UtcNow));

        return Result.Success;
    }

    /// <summary>
    /// Cancels the media library scan.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Success> CancelScan()
    {
        if (Status == LibraryScanJobStatus.Running)
            Status = LibraryScanJobStatus.Canceled;
        else
            return Errors.LibraryScanning.CanOnlyCancelRunningScans;
        
        _domainEvents.Add(new LibraryScanCancelledDomainEvent(Guid.NewGuid(), Id, LibraryId, DateTime.UtcNow));
 
        return Result.Success;
    }

    /// <summary>
    /// Marks the media library scan as failed.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Success> FailScan()
    {
        if (Status == LibraryScanJobStatus.Running)
            Status = LibraryScanJobStatus.Failed;
        else
            return Errors.LibraryScanning.CanOnlyFailRunningScans;

        return Result.Success;
    }
}
