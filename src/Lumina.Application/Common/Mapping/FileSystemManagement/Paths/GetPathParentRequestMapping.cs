#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Extension methods for converting <see cref="GetPathParentRequest"/>.
/// </summary>
public static class GetPathParentRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetPathParentQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetPathParentQuery ToQuery(this GetPathParentRequest request)
    {
        return new GetPathParentQuery(
            request.Path
        );
    }
}
