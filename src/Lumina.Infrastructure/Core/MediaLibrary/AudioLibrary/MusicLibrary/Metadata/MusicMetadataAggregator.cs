#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Metadata;

/// <summary>
/// Merges the music metadata returned by multiple metadata providers into a single metadata, giving priority to the first provider that provides a value for each field.
/// </summary>
internal static class MusicMetadataAggregator
{
    /// <summary>
    /// Merges <paramref name="first"/> with <paramref name="second"/>, keeping the first non-empty value of each scalar field, and the union of the collection fields.
    /// </summary>
    /// <param name="first">The first artist metadata to merge, whose values have priority.</param>
    /// <param name="second">The second artist metadata, filling the fields that the first metadata lacks.</param>
    /// <returns>The merged artist metadata.</returns>
    public static ArtistMetadataDto Merge(ArtistMetadataDto first, ArtistMetadataDto second)
    {
        return new ArtistMetadataDto(
            First(first.Name, second.Name),
            First(first.SortName, second.SortName),
            First(first.Disambiguation, second.Disambiguation),
            First(first.Type, second.Type),
            First(first.Gender, second.Gender),
            First(first.Country, second.Country),
            First(first.Area, second.Area),
            First(first.BeginArea, second.BeginArea),
            First(first.EndArea, second.EndArea),
            First(first.LifeSpanBegin, second.LifeSpanBegin),
            First(first.LifeSpanEnd, second.LifeSpanEnd),
            first.IsEnded || second.IsEnded,
            First(first.Website, second.Website),
            First(first.MusicBrainzArtistId, second.MusicBrainzArtistId),
            Union(first.Ipis, second.Ipis, ipi => ipi),
            Union(first.Isnis, second.Isnis, isni => isni),
            Union(first.Aliases, second.Aliases, alias => alias.Name),
            Union(first.Genres, second.Genres, genre => genre.Name),
            Union(first.Tags, second.Tags, tag => tag.Name),
            Union(first.Ratings, second.Ratings, rating => rating.Source?.ToString()),
            Union(first.Contributors, second.Contributors, contributor => $"{contributor.Name?.DisplayName}|{contributor.Role}"));
    }

    /// <summary>
    /// Merges <paramref name="first"/> with <paramref name="second"/>, keeping the first non-empty value of each scalar field, and the union of the collection fields.
    /// </summary>
    /// <param name="first">The first album metadata to merge, whose values have priority.</param>
    /// <param name="second">The second album metadata, filling the fields that the first metadata lacks.</param>
    /// <returns>The merged album metadata.</returns>
    public static AlbumMetadataDto Merge(AlbumMetadataDto first, AlbumMetadataDto second)
    {
        return new AlbumMetadataDto(
            First(first.Title, second.Title),
            First(first.OriginalTitle, second.OriginalTitle),
            First(first.Description, second.Description),
            First(first.Disambiguation, second.Disambiguation),
            MergeReleaseInfo(first.ReleaseInfo, second.ReleaseInfo),
            First(first.Language, second.Language),
            First(first.OriginalLanguage, second.OriginalLanguage),
            Union(first.Tags, second.Tags, tag => tag.Name),
            Union(first.Genres, second.Genres, genre => genre.Name),
            First(first.Script, second.Script),
            first.ReleaseTypes is { Count: > 0 } ? first.ReleaseTypes : second.ReleaseTypes,
            First(first.ReleaseStatus, second.ReleaseStatus),
            First(first.MediaFormat, second.MediaFormat),
            First(first.Packaging, second.Packaging),
            First(first.TotalDiscs, second.TotalDiscs),
            First(first.TotalTracks, second.TotalTracks),
            First(first.Barcode, second.Barcode),
            Union(first.CatalogNumbers, second.CatalogNumbers, catalogNumber => catalogNumber),
            First(first.Label, second.Label),
            First(first.ASIN, second.ASIN),
            First(first.MusicBrainzReleaseId, second.MusicBrainzReleaseId),
            First(first.MusicBrainzReleaseGroupId, second.MusicBrainzReleaseGroupId),
            First(first.MusicBrainzReleaseArtistId, second.MusicBrainzReleaseArtistId),
            Union(first.Contributors, second.Contributors, contributor => $"{contributor.Name?.DisplayName}|{contributor.Role}"),
            Union(first.Ratings, second.Ratings, rating => rating.Source?.ToString()),
            First(first.ReleaseTitle, second.ReleaseTitle));
    }

    /// <summary>
    /// Merges <paramref name="first"/> with <paramref name="second"/>, keeping the first non-empty value of each scalar field, and the union of the collection fields.
    /// </summary>
    /// <param name="first">The first audio metadata to merge, whose values have priority.</param>
    /// <param name="second">The second audio metadata, filling the fields that the first metadata lacks.</param>
    /// <returns>The merged audio metadata.</returns>
    public static AudioMetadataDto Merge(AudioMetadataDto first, AudioMetadataDto second)
    {
        return new AudioMetadataDto(
            First(first.Title, second.Title),
            First(first.OriginalTitle, second.OriginalTitle),
            First(first.Description, second.Description),
            First(first.Disambiguation, second.Disambiguation),
            MergeReleaseInfo(first.ReleaseInfo, second.ReleaseInfo),
            First(first.Language, second.Language),
            First(first.OriginalLanguage, second.OriginalLanguage),
            Union(first.Tags, second.Tags, tag => tag.Name),
            Union(first.Genres, second.Genres, genre => genre.Name),
            First(first.Script, second.Script),
            First(first.Key, second.Key),
            First(first.Bpm, second.Bpm),
            first.IsVideo || second.IsVideo,
            First(first.Work, second.Work),
            Union(first.Isrcs, second.Isrcs, isrc => isrc.Value),
            Union(first.Moods, second.Moods, mood => mood.Name),
            First(first.DurationInSeconds, second.DurationInSeconds),
            First(first.SampleRate, second.SampleRate),
            First(first.Channels, second.Channels),
            First(first.BitDepth, second.BitDepth),
            First(first.AudioCodec, second.AudioCodec),
            First(first.Bitrate, second.Bitrate),
            First(first.MusicBrainzRecordingId, second.MusicBrainzRecordingId),
            First(first.MusicBrainzTrackId, second.MusicBrainzTrackId),
            Union(first.Contributors, second.Contributors, contributor => $"{contributor.Name?.DisplayName}|{contributor.Role}"),
            Union(first.Ratings, second.Ratings, rating => rating.Source?.ToString()));
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

        DateOnly? originalReleaseDate = First(first?.OriginalReleaseDate, second?.OriginalReleaseDate);
        int? originalReleaseYear = First(first?.OriginalReleaseYear, second?.OriginalReleaseYear);
        DateOnly? reReleaseDate = First(first?.ReReleaseDate, second?.ReReleaseDate);
        int? reReleaseYear = First(first?.ReReleaseYear, second?.ReReleaseYear);

        // A release date and its year describe the same point in time, so the year is derived from the date whenever a date is known. Without this,
        // a year from one provider could be combined with a date from another (for example a release year from the tags with the original release
        // date from MusicBrainz), and the resulting inconsistent pair would be rejected when the metadata is applied.
        if (originalReleaseDate is not null)
            originalReleaseYear = originalReleaseDate.Value.Year;
        if (reReleaseDate is not null)
            reReleaseYear = reReleaseDate.Value.Year;

        return new ReleaseInfoDto(
            originalReleaseDate,
            originalReleaseYear,
            reReleaseDate,
            reReleaseYear,
            First(first?.ReleaseCountry, second?.ReleaseCountry),
            First(first?.ReleaseVersion, second?.ReleaseVersion));
    }

    /// <summary>
    /// Gets the first non-null value of the two values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="first">The value with priority.</param>
    /// <param name="second">The fallback value.</param>
    /// <returns>The first non-null value, or <see langword="null"/> when both are <see langword="null"/>.</returns>
    private static T? First<T>(T? first, T? second)
    {
        return first is not null && !IsEmpty(first) ? first : second;
    }

    /// <summary>
    /// Determines whether the provided value is an empty string.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> when the value is an empty string, otherwise <see langword="false"/>.</returns>
    private static bool IsEmpty<T>(T? value)
    {
        return value is string text && string.IsNullOrWhiteSpace(text);
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
