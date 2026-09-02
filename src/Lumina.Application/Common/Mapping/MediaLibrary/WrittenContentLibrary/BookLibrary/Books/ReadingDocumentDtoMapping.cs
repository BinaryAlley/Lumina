#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="ReadingDocumentDto"/>.
/// </summary>
public static class ReadingDocumentDtoMapping
{
    /// <summary>
    /// Converts <paramref name="document"/> to <see cref="ReadingManifestResponse"/>.
    /// </summary>
    /// <param name="document">The reading document to be converted.</param>
    /// <returns>The converted reading manifest response.</returns>
    public static ReadingManifestResponse ToResponse(this ReadingDocumentDto document)
    {
        return new ReadingManifestResponse(
            document.Title,
            document.Author,
            document.CoverResourceKey,
            [.. document.TableOfContents.Select(entry => entry.ToResponse())],
            [.. document.Spine.Select(spineItem => new ReadingSpineItemResponse(spineItem.LocationRef, spineItem.Title))],
            [.. document.Resources.Keys],
            document.HasTextContent
        );
    }
}
