#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Value Object for the release info of a media element.
/// </summary>
[DebuggerDisplay("{ReleaseYear}")]
public class ReleaseInfo : ValueObject
{
    /// <summary>
    /// Gets the optional original release date of the media item.
    /// </summary>
    public Optional<DateOnly> OriginalReleaseDate { get; }

    /// <summary>
    /// Gets the optional original release year of the media item.
    /// This is useful when only the year of original release is known.
    /// </summary>
    public Optional<int> OriginalReleaseYear { get; }

    /// <summary>
    /// Gets the optional re-release or reissue date of the media item.
    /// </summary>
    public Optional<DateOnly> ReReleaseDate { get; }

    /// <summary>
    /// Gets the optional re-release or reissue year of the media item.
    /// </summary>
    public Optional<int> ReReleaseYear { get; }

    /// <summary>
    /// Gets the optional country or region of release.
    /// </summary>
    public Optional<string> ReleaseCountry { get; }

    /// <summary>
    /// Gets the optional release version or edition. (e.g. "Original", "Director's Cut", "2.0")
    /// </summary>
    public Optional<string> ReleaseVersion { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReleaseInfo"/> class.
    /// </summary>
    /// <param name="originalReleaseDate">The optional original release date of the media item.</param>
    /// <param name="originalReleaseYear">The optional original release year of the media item.</param>
    /// <param name="reReleaseDate">The optional re-release date of the media item.</param>
    /// <param name="reReleaseYear">The optional re-release year of the media item.</param>
    /// <param name="releaseCountry">The optional country or region of release.</param>
    /// <param name="releaseVersion">The optional release version or edition.</param>
    private ReleaseInfo(
        Optional<DateOnly> originalReleaseDate,
        Optional<int> originalReleaseYear,
        Optional<DateOnly> reReleaseDate,
        Optional<int> reReleaseYear,
        Optional<string> releaseCountry,
        Optional<string> releaseVersion)
    {
        OriginalReleaseDate = originalReleaseDate;
        OriginalReleaseYear = originalReleaseYear;
        ReReleaseDate = reReleaseDate;
        ReReleaseYear = reReleaseYear;
        ReleaseCountry = releaseCountry;
        ReleaseVersion = releaseVersion;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ReleaseInfo"/> class.
    /// </summary>
    /// <param name="originalReleaseDate">The optional original release date of the media item.</param>
    /// <param name="originalReleaseYear">The optional original release year of the media item.</param>
    /// <param name="reReleaseDate">The optional re-release date of the media item.</param>
    /// <param name="reReleaseYear">The optional re-release year of the media item.</param>
    /// <param name="releaseCountry">The optional country or region of release.</param>
    /// <param name="releaseVersion">The optional release version or edition.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="ReleaseInfo"/>, or an error message.
    /// </returns>
    public static ErrorOr<ReleaseInfo> Create(
        Optional<DateOnly> originalReleaseDate,
        Optional<int> originalReleaseYear,
        Optional<DateOnly> reReleaseDate,
        Optional<int> reReleaseYear,
        Optional<string> releaseCountry,
        Optional<string> releaseVersion)
    {
        if (originalReleaseDate.HasValue && originalReleaseYear.HasValue && originalReleaseDate.Value.Year != originalReleaseYear.Value)
            return Errors.Errors.Metadata.OriginalReleaseDateAndYearMustMatch;
        if (reReleaseDate.HasValue && reReleaseYear.HasValue && reReleaseDate.Value.Year != reReleaseYear.Value)
            return Errors.Errors.Metadata.ReReleaseDateAndYearMustMatch;
        if (originalReleaseDate.HasValue && reReleaseDate.HasValue && originalReleaseDate.Value > reReleaseDate.Value)
            return Errors.Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate;
        if (originalReleaseYear.HasValue && reReleaseYear.HasValue && originalReleaseYear.Value > reReleaseYear.Value)
            return Errors.Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear;
        return new ReleaseInfo(originalReleaseDate, originalReleaseYear, reReleaseDate, reReleaseYear, releaseCountry, releaseVersion);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return OriginalReleaseDate;
        yield return OriginalReleaseYear;
        yield return ReReleaseDate;
        yield return ReReleaseYear;
        yield return ReleaseCountry;
        yield return ReleaseVersion;
    }
}
