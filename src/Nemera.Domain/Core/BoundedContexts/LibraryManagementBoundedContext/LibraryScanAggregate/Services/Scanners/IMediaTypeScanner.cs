#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
#endregion

namespace Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;

/// <summary>
/// Interface defining a media library scanner for a specific media library type.
/// </summary>
public interface IMediaTypeScanner
{
    /// <summary>
    /// The media library type that this media library scanner supports.
    /// </summary>
    LibraryType SupportedLibraryType { get; }

    /// <summary>
    /// Creates the media library scan job graph for the provided media library.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library for which to create the media library scan jobs.</param>
    /// <returns>The file system discovery job, which is the only top level job of the media library scan job graph.</returns>
    IMediaLibraryScanDiscoveryJob CreateScanJobsForLibrary(LibraryId libraryId);
}
