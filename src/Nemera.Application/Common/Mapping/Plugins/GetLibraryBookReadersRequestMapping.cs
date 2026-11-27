#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="GetLibraryBookReadersRequest"/>.
/// </summary>
public static class GetLibraryBookReadersRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetLibraryBookReadersQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetLibraryBookReadersQuery ToQuery(this GetLibraryBookReadersRequest request)
    {
        return new GetLibraryBookReadersQuery(
            request.LibraryId
        );
    }
}
