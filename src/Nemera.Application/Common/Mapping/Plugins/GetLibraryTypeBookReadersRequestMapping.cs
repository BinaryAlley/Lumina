#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Plugins.Queries.GetLibraryTypeBookReaders;
using Nemera.Contracts.Requests.Plugins;
#endregion

namespace Nemera.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="GetLibraryTypeBookReadersRequest"/>.
/// </summary>
public static class GetLibraryTypeBookReadersRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetLibraryTypeBookReadersQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetLibraryTypeBookReadersQuery ToQuery(this GetLibraryTypeBookReadersRequest request)
    {
        return new GetLibraryTypeBookReadersQuery(
            request.LibraryType
        );
    }
}
