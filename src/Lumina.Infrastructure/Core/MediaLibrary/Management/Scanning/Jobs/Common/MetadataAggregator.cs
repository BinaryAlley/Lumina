#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Merges the book metadata returned by multiple metadata providers into a single book metadata, giving priority to the first provider that provides a value for each field.
/// </summary>
internal static class MetadataAggregator
{
    /// <summary>
    /// Merges <paramref name="first"/> with <paramref name="second"/>, keeping the first non-empty value of each scalar field, and the union of the collection fields.
    /// </summary>
    /// <param name="first">The first metadata to merge, whose values have priority.</param>
    /// <param name="second">The second metadata to merge, filling the fields that the first metadata lacks.</param>
    /// <returns>The merged metadata.</returns>
    public static BookMetadataDto Merge(BookMetadataDto first, BookMetadataDto second)
    {
        return new BookMetadataDto(
            FirstOrDefaultReference(first.Title, second.Title),
            FirstOrDefaultReference(first.OriginalTitle, second.OriginalTitle),
            FirstOrDefaultReference(first.Description, second.Description),
            MergeReleaseInfo(first.ReleaseInfo, second.ReleaseInfo),
            Union(first.Genres, second.Genres, genre => genre.Name),
            Union(first.Tags, second.Tags, tag => tag.Name),
            FirstOrDefaultReference(first.Language, second.Language),
            FirstOrDefaultReference(first.OriginalLanguage, second.OriginalLanguage),
            FirstOrDefaultReference(first.Publisher, second.Publisher),
            FirstOrDefaultValue(first.PageCount, second.PageCount),
            FirstOrDefaultValue(first.Format, second.Format),
            FirstOrDefaultReference(first.Edition, second.Edition),
            FirstOrDefaultValue(first.VolumeNumber, second.VolumeNumber),
            FirstOrDefaultReference(first.Series, second.Series),
            FirstOrDefaultReference(first.ASIN, second.ASIN),
            FirstOrDefaultReference(first.GoodreadsId, second.GoodreadsId),
            FirstOrDefaultReference(first.LCCN, second.LCCN),
            FirstOrDefaultReference(first.OCLCNumber, second.OCLCNumber),
            FirstOrDefaultReference(first.OpenLibraryId, second.OpenLibraryId),
            FirstOrDefaultReference(first.LibraryThingId, second.LibraryThingId),
            FirstOrDefaultReference(first.GoogleBooksId, second.GoogleBooksId),
            FirstOrDefaultReference(first.BarnesAndNobleId, second.BarnesAndNobleId),
            FirstOrDefaultReference(first.AppleBooksId, second.AppleBooksId),
            Union(first.Isbns, second.Isbns, isbn => isbn.Value),
            Union(first.Contributors, second.Contributors, contributor => $"{contributor.Name?.DisplayName}|{contributor.Role?.Name}"),
            Union(first.Ratings, second.Ratings, rating => rating.Source?.ToString()),
            FirstOrDefaultReference(first.CoverImagePath, second.CoverImagePath));
    }

    /// <summary>
    /// Merges the release information of the two metadata, keeping the first non-null value of each field.
    /// </summary>
    /// <param name="first">The first release information, whose values have priority.</param>
    /// <param name="second">The second release information, filling the fields that the first one lacks.</param>
    /// <returns>The merged release information, or <see langword="null"/> when both are <see langword="null"/>.</returns>
    private static ReleaseInfoDto? MergeReleaseInfo(ReleaseInfoDto? first, ReleaseInfoDto? second)
    {
        if (first is null && second is null)
            return null;

        return new ReleaseInfoDto(
            FirstOrDefaultValue(first?.OriginalReleaseDate, second?.OriginalReleaseDate),
            FirstOrDefaultValue(first?.OriginalReleaseYear, second?.OriginalReleaseYear),
            FirstOrDefaultValue(first?.ReReleaseDate, second?.ReReleaseDate),
            FirstOrDefaultValue(first?.ReReleaseYear, second?.ReReleaseYear),
            FirstOrDefaultReference(first?.ReleaseCountry, second?.ReleaseCountry),
            FirstOrDefaultReference(first?.ReleaseVersion, second?.ReleaseVersion));
    }

    /// <summary>
    /// Gets the first non-null value of the two reference values.
    /// </summary>
    /// <typeparam name="T">The type of the reference values.</typeparam>
    /// <param name="first">The value with priority.</param>
    /// <param name="second">The fallback value.</param>
    /// <returns>The first non-null value, or <see langword="null"/> when both are <see langword="null"/>.</returns>
    private static T? FirstOrDefaultReference<T>(T? first, T? second)
        where T : class
    {
        return first ?? second;
    }

    /// <summary>
    /// Gets the first non-null value of the two nullable value types.
    /// </summary>
    /// <typeparam name="T">The underlying type of the nullable values.</typeparam>
    /// <param name="first">The value with priority.</param>
    /// <param name="second">The fallback value.</param>
    /// <returns>The first non-null value, or <see langword="null"/> when both are <see langword="null"/>.</returns>
    private static T? FirstOrDefaultValue<T>(T? first, T? second)
        where T : struct
    {
        return first ?? second;
    }

    /// <summary>
    /// Returns the union of the two collections, de-duplicated by the <paramref name="keySelector"/> key, keeping the items of the first collection on key conflicts.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    /// <param name="first">The first collection, whose items have priority.</param>
    /// <param name="second">The second collection.</param>
    /// <param name="keySelector">The function used to extract the de-duplication key of an item.</param>
    /// <returns>The union of the two collections, or <see langword="null"/> when both are <see langword="null"/>.</returns>
    private static List<T>? Union<T>(List<T>? first, List<T>? second, Func<T, string?> keySelector)
        where T : class
    {
        if (first is null && second is null)
            return null;

        Dictionary<string, T> seen = new(StringComparer.OrdinalIgnoreCase);
        List<T> result = [];

        void AddRange(List<T>? items)
        {
            foreach (T item in items ?? [])
                if (seen.TryAdd(keySelector(item) ?? string.Empty, item))
                    result.Add(item);
        }

        AddRange(first);
        AddRange(second);
        return result;
    }
}
