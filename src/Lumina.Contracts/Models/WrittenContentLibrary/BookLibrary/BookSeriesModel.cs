#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Represents a request to get book series information.
/// </summary>
/// <param name="Title">Gets the title of the book series.</param>
[DebuggerDisplay("{Title}")]
public record BookSeriesModel(
    string? Title
);