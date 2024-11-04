#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Common;

/// <summary>
/// Represents a tag.
/// </summary>
/// <param name="Name">Gets the name of the tag.</param>
[DebuggerDisplay("Name: {Name}")]
public record TagEntity(
    string? Name
);
