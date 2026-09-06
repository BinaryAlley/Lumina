#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="GetBookRequest"/>.
/// </summary>
public static class GetBookRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetBookQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetBookQuery ToQuery(this GetBookRequest request)
    {
        return new GetBookQuery(
            Guid.TryParse(request.Id, out Guid bookId) ? bookId : Guid.Empty
        );
    }
}
