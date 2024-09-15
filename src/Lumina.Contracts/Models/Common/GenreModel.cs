#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.Common;

/// <summary>
/// Represents a request to get genre information.
/// </summary>
/// <param name="Name">Gets the name of the genre.</param>
[DebuggerDisplay("{Name}")]
public record GenreModel(
    string? Name
);