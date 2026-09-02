#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="ReadingTocEntryDto"/>.
/// </summary>
public static class ReadingTocEntryDtoMapping
{
    /// <summary>
    /// Converts <paramref name="entry"/> to <see cref="ReadingTocEntryResponse"/>.
    /// </summary>
    /// <param name="entry">The table of contents entry to be converted.</param>
    /// <returns>The converted table of contents entry response.</returns>
    public static ReadingTocEntryResponse ToResponse(this ReadingTocEntryDto entry)
    {
        return new ReadingTocEntryResponse(
            entry.Label,
            entry.LocationRef,
            [.. entry.Children.Select(child => child.ToResponse())]
        );
    }
}
