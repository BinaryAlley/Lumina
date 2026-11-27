#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for extracting the metadata of the discovered music file system items,
/// reading it from the embedded tags of the files and falling back to the structure of the media library on disk.
/// </summary>
public interface IMusicMetadataExtractionJob : IMediaLibraryScanJob
{
}
