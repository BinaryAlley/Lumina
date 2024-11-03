#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Repository entity for book series.
/// </summary>
/// <param name="Title">The title of the book series.</param>
[DebuggerDisplay("Title: {Title}")]
public record BookSeriesEntity(
    string? Title
);
