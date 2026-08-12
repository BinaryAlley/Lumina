#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for a release information.
/// </summary>
/// <param name="OriginalReleaseDate">The original release date of the content. Optional.</param>
/// <param name="OriginalReleaseYear">The original release year of the content. Optional.</param>
/// <param name="ReReleaseDate">The re-release date of the content. Optional.</param>
/// <param name="ReReleaseYear">The re-release year of the content. Optional.</param>
/// <param name="ReleaseCountry">The country where the content was released. Optional.</param>
/// <param name="ReleaseVersion">The version or edition of the content's release. Optional.</param>
[DebuggerDisplay("OriginalReleaseDate: {OriginalReleaseDate}")]
public record ReleaseInfoDto(
    DateOnly? OriginalReleaseDate,
    int? OriginalReleaseYear,
    DateOnly? ReReleaseDate,
    int? ReReleaseYear,
    string? ReleaseCountry,
    string? ReleaseVersion
);
