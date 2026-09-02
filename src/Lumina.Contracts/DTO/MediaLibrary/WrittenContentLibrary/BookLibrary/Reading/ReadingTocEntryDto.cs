#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Data transfer object for an entry of the table of contents of a reading document.
/// </summary>
/// <param name="Label">The label of the table of contents entry.</param>
/// <param name="LocationRef">The opaque location reference of the reading section the entry points to.</param>
/// <param name="Children">The child entries of the table of contents entry.</param>
[DebuggerDisplay("Label: {Label}")]
public sealed record ReadingTocEntryDto(
    string Label,
    string LocationRef,
    IReadOnlyList<ReadingTocEntryDto> Children
);
