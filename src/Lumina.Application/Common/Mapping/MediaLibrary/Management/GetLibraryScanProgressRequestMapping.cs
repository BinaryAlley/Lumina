#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using Lumina.Contracts.Requests.MediaLibrary.Management;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="GetLibraryScanProgressRequest"/>.
/// </summary>
public static class GetLibraryScanProgressRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetLibraryScanProgressQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetLibraryScanProgressQuery ToQuery(this GetLibraryScanProgressRequest request)
    {
        return new GetLibraryScanProgressQuery(
            request.LibraryId,
            request.ScanId
        );
    }
}
