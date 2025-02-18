#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;

/// <summary>
/// Interface defining a media library scanner for a specific media library type.
/// </summary>
public interface IMediaTypeScanner
{
    /// <summary>
    /// The media library type that this media library scanner supports.
    /// </summary>
    LibraryType SupportedType { get; }

    /// <summary>
    /// Creates the media library scan jobs for the provided media library.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library for which to create the media library scan jobs.</param>
    /// <param name="downloadMedatadaFromWeb">Whether the library permits downloading data from the web, or not.</param>
    /// <returns>A collection of media library scan jobs.</returns>
    IEnumerable<IMediaLibraryScanJob> CreateScanJobsForLibrary(LibraryId libraryId, bool downloadMedatadaFromWeb);
}
