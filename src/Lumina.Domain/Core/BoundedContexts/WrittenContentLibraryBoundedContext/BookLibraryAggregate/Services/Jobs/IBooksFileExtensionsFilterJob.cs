#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;

/// <summary>
/// Interface for the media library scan job for filtering only files that have known book formats extensions.
/// </summary>
public interface IBooksFileExtensionsFilterJob : IMediaLibraryScanJob
{
}
