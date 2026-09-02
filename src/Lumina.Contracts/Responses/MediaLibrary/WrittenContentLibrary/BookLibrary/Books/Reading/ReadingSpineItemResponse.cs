#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents an item of the spine of the reading manifest of a book.
/// </summary>
/// <param name="LocationRef">The opaque location reference of the reading section.</param>
/// <param name="Title">The title of the reading section, if known.</param>
[DebuggerDisplay("LocationRef: {LocationRef}")]
public sealed record ReadingSpineItemResponse(
    string LocationRef,
    string? Title
);
