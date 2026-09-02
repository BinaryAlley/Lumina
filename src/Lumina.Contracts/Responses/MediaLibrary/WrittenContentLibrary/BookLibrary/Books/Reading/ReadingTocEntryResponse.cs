#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Represents an entry of the table of contents of the reading manifest of a book.
/// </summary>
/// <param name="Label">The label of the table of contents entry.</param>
/// <param name="LocationRef">The opaque location reference of the reading section the entry points to.</param>
/// <param name="Children">The child entries of the table of contents entry.</param>
[DebuggerDisplay("Label: {Label}")]
public sealed record ReadingTocEntryResponse(
    string Label,
    string LocationRef,
    IReadOnlyList<ReadingTocEntryResponse> Children
);
