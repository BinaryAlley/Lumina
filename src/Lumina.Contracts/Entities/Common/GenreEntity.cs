#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Entities.Common;

/// <summary>
/// Represents a request to get genre information.
/// </summary>
/// <param name="Name">Gets the name of the genre.</param>
[DebuggerDisplay("Name: {Name}")]
public record GenreEntity(
    string? Name
);
