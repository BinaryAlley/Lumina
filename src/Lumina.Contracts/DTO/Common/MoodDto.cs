#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for a mood of a media element.
/// </summary>
/// <param name="Name">The name of the mood.</param>
[DebuggerDisplay("Name: {Name}")]
public record MoodDto(
    string? Name
);
