#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for a genre.
/// </summary>
/// <param name="Name">Gets the name of the genre.</param>
[DebuggerDisplay("Name: {Name}")]
public record GenreDto(
    string? Name
);
