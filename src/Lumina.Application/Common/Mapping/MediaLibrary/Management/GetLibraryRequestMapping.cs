#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="GetLibraryRequest"/>.
/// </summary>
public static class GetLibraryRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetLibraryQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="userId">The Id of the user requesting the media library.</param>
    /// <returns>The converted query.</returns>
    public static GetLibraryQuery ToQuery(this GetLibraryRequest request, Guid userId)
    {
        return new GetLibraryQuery(
            request.Id,
            userId
        );
    }
}
