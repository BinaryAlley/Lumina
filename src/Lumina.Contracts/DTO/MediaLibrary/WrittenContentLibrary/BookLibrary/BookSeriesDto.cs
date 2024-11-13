#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object for book series.
/// </summary>
/// <param name="Title">The title of the book series.</param>
[DebuggerDisplay("Title: {Title}")]
public record BookSeriesDto(
    string? Title
);
