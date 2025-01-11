#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibrary;
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
    /// <returns>The converted query.</returns>
    public static GetLibraryQuery ToQuery(this GetLibraryRequest request)
    {
        return new GetLibraryQuery(
            request.Id
        );
    }
}
