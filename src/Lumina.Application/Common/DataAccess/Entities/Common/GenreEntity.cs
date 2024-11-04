#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Common;

/// <summary>
/// Represents a genre.
/// </summary>
/// <param name="Name">Gets the name of the genre.</param>
[DebuggerDisplay("Name: {Name}")]
public record GenreEntity(
    string? Name
);
