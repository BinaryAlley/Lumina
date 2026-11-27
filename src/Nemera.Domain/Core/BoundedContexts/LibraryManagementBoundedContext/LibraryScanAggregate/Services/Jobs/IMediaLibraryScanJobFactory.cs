#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using System;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan jobs factory.
/// </summary>
internal interface IMediaLibraryScanJobFactory
{
    /// <summary>
    /// Creates a new media library scan job of type <typeparamref name="TJob"/>.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library upon which the scan is performed.</param>
    /// <typeparam name="TJob">The type of media library scan job to create.</typeparam>
    /// <returns>A media library scan job of type <typeparamref name="TJob"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested media library scan job has not been registered.</exception>
    TJob CreateJob<TJob>(LibraryId libraryId) where TJob : IMediaLibraryScanJob;
}
