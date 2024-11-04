#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for a tag.
/// </summary>
/// <param name="Name">Gets the name of the tag.</param>
[DebuggerDisplay("Name: {Name}")]
public record TagDto(
    string? Name
);
