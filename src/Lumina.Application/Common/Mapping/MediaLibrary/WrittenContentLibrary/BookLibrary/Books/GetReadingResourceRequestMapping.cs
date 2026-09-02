#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="GetReadingResourceRequest"/>.
/// </summary>
public static class GetReadingResourceRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetReadingResourceQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetReadingResourceQuery ToQuery(this GetReadingResourceRequest request)
    {
        return new GetReadingResourceQuery(
            request.BookId, 
            request.ResourceKey
        );
    }
}
