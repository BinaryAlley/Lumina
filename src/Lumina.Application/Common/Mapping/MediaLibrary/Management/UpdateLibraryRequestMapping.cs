#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="UpdateLibraryRequest"/>.
/// </summary>
public static class UpdateLibraryRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateLibraryCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static UpdateLibraryCommand ToCommand(this UpdateLibraryRequest request)
    {
        return new UpdateLibraryCommand(
            request.Id,
            request.UserId,
            request.Title,
            request.LibraryType,
            request.ContentLocations,
            request.CoverImage,
            request.IsEnabled,
            request.IsLocked,
            request.DownloadMedatadaFromWeb,
            request.SaveMetadataInMediaDirectories
        );
    }
}
