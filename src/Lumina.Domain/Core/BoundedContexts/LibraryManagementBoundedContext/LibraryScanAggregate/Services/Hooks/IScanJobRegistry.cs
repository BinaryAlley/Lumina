#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Hooks;

/// <summary>
/// Registry of the media library scan jobs that are injected by plugins at the defined hook points of the media library scan job graph.
/// </summary>
public interface IScanJobRegistry
{
    /// <summary>
    /// Gets the media library scan jobs registered for the provided <paramref name="hookName"/>.
    /// </summary>
    /// <param name="hookName">The name of the hook point at which the media library scan jobs were injected.</param>
    /// <param name="libraryId">The unique identifier of the media library upon which the scan is performed.</param>
    /// <returns>The collection of media library scan jobs registered at the provided hook point.</returns>
    IEnumerable<IMediaLibraryScanJob> GetJobsForHook(string hookName, LibraryId libraryId);
}
