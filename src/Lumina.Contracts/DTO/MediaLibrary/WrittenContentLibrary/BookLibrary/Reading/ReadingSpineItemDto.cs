#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Data transfer object for an item of the spine of a reading document.
/// </summary>
/// <param name="LocationRef">The opaque location reference of the reading section.</param>
/// <param name="Title">The title of the reading section, if known.</param>
/// <param name="RelativeSectionFilePath">The path of the section content file, relative to the extraction directory of the book.</param>
[DebuggerDisplay("LocationRef: {LocationRef}")]
public sealed record ReadingSpineItemDto(
    string LocationRef,
    string? Title,
    string RelativeSectionFilePath
);
