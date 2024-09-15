#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.MediaContributors;

/// <summary>
/// Represents a media contributor name.
/// </summary>
/// <param name="DisplayName">Gets the display name of the media contributor.</param>
/// <param name="LegalName">Gets the legal name of the media contributor.</param>
[DebuggerDisplay("{DisplayName}")]
public record MediaContributorNameModel(
    string? DisplayName,
    string? LegalName
);