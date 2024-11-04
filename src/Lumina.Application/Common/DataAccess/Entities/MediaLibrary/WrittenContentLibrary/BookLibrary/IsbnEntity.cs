#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.BookLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Repository entity for ISBN information.
/// </summary>
/// <param name="Value">The ISBN value.</param>
/// <param name="Format">The format of the ISBN (e.g., ISBN-10 or ISBN-13).</param>
[DebuggerDisplay("ISBN: {Value} ({Format})")]
public record IsbnEntity(
    string? Value,
    IsbnFormat? Format
);
