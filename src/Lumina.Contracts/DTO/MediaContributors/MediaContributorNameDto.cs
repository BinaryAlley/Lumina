#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaContributors;

/// <summary>
/// Repository entity for a media contributor name.
/// </summary>
/// <param name="DisplayName">The display name of the media contributor.</param>
/// <param name="LegalName">The legal name of the media contributor.</param>
[DebuggerDisplay("DisplayName: {DisplayName}")]
public record MediaContributorNameDto(
    string? DisplayName,
    string? LegalName
);
