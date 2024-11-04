#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for a release information.
/// </summary>
/// <param name="OriginalReleaseDate">Gets the original release date of the content.</param>
/// <param name="OriginalReleaseYear">Gets the original release year of the content.</param>
/// <param name="ReReleaseDate">Gets the re-release date of the content, if applicable.</param>
/// <param name="ReReleaseYear">Gets the re-release year of the content, if applicable.</param>
/// <param name="ReleaseCountry">Gets the country where the content was released.</param>
/// <param name="ReleaseVersion">Gets the version or edition of the content's release.</param>
[DebuggerDisplay("OriginalReleaseDate: {OriginalReleaseDate}")]
public record ReleaseInfoDto(
    DateOnly? OriginalReleaseDate,
    int? OriginalReleaseYear,
    DateOnly? ReReleaseDate,
    int? ReReleaseYear,
    string? ReleaseCountry,
    string? ReleaseVersion
);
