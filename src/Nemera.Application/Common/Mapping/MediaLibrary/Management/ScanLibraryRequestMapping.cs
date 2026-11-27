#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="ScanLibraryRequest"/>.
/// </summary>
public static class ScanLibraryRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="ScanLibraryCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static ScanLibraryCommand ToCommand(this ScanLibraryRequest request)
    {
        return new ScanLibraryCommand(
            request.Id
        );
    }
}
