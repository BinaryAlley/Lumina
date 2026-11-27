#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for a tag.
/// </summary>
/// <param name="Name">The name of the tag.</param>
[DebuggerDisplay("Name: {Name}")]
public record TagDto(
    string? Name
);
