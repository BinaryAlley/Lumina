#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;

/// <summary>
/// Interface for the media library scan jobs factory.
/// </summary>
internal interface IMediaScanJobFactory
{
    /// <summary>
    /// Creates a new media library scan job of type <typeparamref name="TJob"/>.
    /// </summary>
    /// <typeparam name="TJob">The type of media library scan job to create.</typeparam>
    /// <returns>A media library scan job of type <typeparamref name="TJob"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested media library scan job has not been registered.</exception>
    TJob CreateJob<TJob>(Library library) where TJob : MediaScanJob;
}
