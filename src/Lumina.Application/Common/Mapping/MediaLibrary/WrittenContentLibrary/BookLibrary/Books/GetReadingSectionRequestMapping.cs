#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="GetReadingSectionRequest"/>.
/// </summary>
public static class GetReadingSectionRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetReadingSectionQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetReadingSectionQuery ToQuery(this GetReadingSectionRequest request)
    {
        return new GetReadingSectionQuery(
            request.BookId, 
            request.LocationRef
        );
    }
}
