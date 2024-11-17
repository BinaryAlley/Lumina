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
    /// <param name="userId">The Id of the user making the request to update the media library.</param>
    /// <returns>The converted command.</returns>
    public static UpdateLibraryCommand ToCommand(this UpdateLibraryRequest request, Guid userId)
    {
        return new UpdateLibraryCommand(
            request.Id,
            request.UserId,
            userId,
            request.Title,
            request.LibraryType,
            request.ContentLocations
        );
    }
}
