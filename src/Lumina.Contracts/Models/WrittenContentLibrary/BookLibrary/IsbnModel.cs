#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.BookLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Represents a request to get ISBN information.
/// </summary>
/// <param name="Value">Gets the ISBN value.</param>
/// <param name="Format">Gets the format of the ISBN (e.g., ISBN-10 or ISBN-13).</param>
[DebuggerDisplay("{Value} ({Format})")]
public record IsbnModel(
    string? Value,
    IsbnFormat? Format
);