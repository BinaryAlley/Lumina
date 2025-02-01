#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Lumina.Contracts.Requests.MediaLibrary.Management;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="CancelLibraryScanRequest"/>.
/// </summary>
public static class CancelLibraryScanRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="CancelLibraryScanCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static CancelLibraryScanCommand ToCommand(this CancelLibraryScanRequest request)
    {
        return new CancelLibraryScanCommand(
            request.Id
        );
    }
}
