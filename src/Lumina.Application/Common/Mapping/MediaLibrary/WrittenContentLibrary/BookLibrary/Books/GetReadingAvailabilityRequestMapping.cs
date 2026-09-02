#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="GetReadingAvailabilityRequest"/>.
/// </summary>
public static class GetReadingAvailabilityRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetReadingAvailabilityQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetReadingAvailabilityQuery ToQuery(this GetReadingAvailabilityRequest request)
    {
        return new GetReadingAvailabilityQuery(
            request.BookId
        );
    }
}
