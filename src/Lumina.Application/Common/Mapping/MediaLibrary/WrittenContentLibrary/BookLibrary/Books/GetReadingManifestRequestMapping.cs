#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="GetReadingManifestRequest"/>.
/// </summary>
public static class GetReadingManifestRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetReadingManifestQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetReadingManifestQuery ToQuery(this GetReadingManifestRequest request)
    {
        return new GetReadingManifestQuery(
            request.BookId
        );
    }
}
