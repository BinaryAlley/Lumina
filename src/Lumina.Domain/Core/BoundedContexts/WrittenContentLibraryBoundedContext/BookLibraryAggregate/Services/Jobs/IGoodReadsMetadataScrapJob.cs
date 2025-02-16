#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for retrieving written content metadata from GoodReads.
/// </summary>
public interface IGoodReadsMetadataScrapJob : IMediaLibraryScanJob
{
}
