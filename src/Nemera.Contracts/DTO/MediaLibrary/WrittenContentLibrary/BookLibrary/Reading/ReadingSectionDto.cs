#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Data transfer object for the content of a reading section.
/// </summary>
/// <param name="LocationRef">The opaque location reference of the reading section.</param>
/// <param name="Title">The title of the reading section, if known.</param>
/// <param name="ContentHtml">The sanitized HTML content of the reading section, ready to be rendered by the client.</param>
[DebuggerDisplay("LocationRef: {LocationRef}")]
public sealed record ReadingSectionDto(
    string LocationRef,
    string? Title,
    string ContentHtml
);
