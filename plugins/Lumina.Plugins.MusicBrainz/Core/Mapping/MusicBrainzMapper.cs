#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Plugins.MusicBrainz.Core.Mapping;

/// <summary>
/// Maps MusicBrainz web service responses into the application's media metadata DTOs.
/// </summary>
internal static partial class MusicBrainzMapper
{
    /// <summary>
    /// The neutral cultures of the process, indexed by both their two letter and three letter ISO 639 codes.
    /// Built once, because the available cultures do not change during the lifetime of the process.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, CultureInfo> s_neutralCulturesByCode = BuildNeutralCultureLookup();

    /// <summary>
    /// Matches a four digit year.
    /// </summary>
    [GeneratedRegex(@"\b(?:1[0-9]{3}|20[0-9]{2}|2100)\b")]
    private static partial Regex YearPattern();

    /// <summary>
    /// Maps a MusicBrainz artist response into artist metadata.
    /// </summary>
    /// <param name="artist">The artist response to map.</param>
    /// <returns>The mapped artist metadata.</returns>
    public static ArtistMetadataDto MapArtist(MusicBrainzArtistResponse artist)
    {
        (List<MediaContributorDto> contributors, string? website) = MapArtistRelations(artist.Relations);
        return new ArtistMetadataDto(
            Name: NullIfWhiteSpace(artist.Name),
            SortName: NullIfWhiteSpace(artist.SortName),
            Disambiguation: NullIfWhiteSpace(artist.Disambiguation),
            Type: MapArtistType(artist.Type),
            Gender: MapArtistGender(artist.Gender),
            Country: NullIfWhiteSpace(artist.Country),
            Area: MapArea(artist.Area),
            BeginArea: MapArea(artist.BeginArea),
            EndArea: MapArea(artist.EndArea),
            LifeSpanBegin: ParseDate(artist.LifeSpan?.Begin).Date,
            LifeSpanEnd: ParseDate(artist.LifeSpan?.End).Date,
            IsEnded: artist.LifeSpan?.IsEnded ?? false,
            Website: website,
            MusicBrainzArtistId: ParseGuid(artist.Id),
            Ipis: [.. artist.Ipis.Where(ipi => !string.IsNullOrWhiteSpace(ipi))],
            Isnis: [.. artist.Isnis.Where(isni => !string.IsNullOrWhiteSpace(isni))],
            Aliases: [.. artist.Aliases.Select(MapAlias)],
            Genres: MapGenres(artist.Genres),
            Tags: MapTags(artist.Tags),
            Ratings: MapRating(artist.Rating),
            Contributors: contributors);
    }

    /// <summary>
    /// Maps a MusicBrainz release group, and optionally one of its releases, into album metadata.
    /// </summary>
    /// <param name="releaseGroup">The release group response to map.</param>
    /// <param name="release">The selected release of the release group, or <see langword="null"/> when no release was selected.</param>
    /// <returns>The mapped album metadata.</returns>
    public static AlbumMetadataDto MapAlbum(MusicBrainzReleaseGroupResponse releaseGroup, MusicBrainzReleaseResponse? release)
    {
        (DateOnly? originalDate, int? originalYear) = ParseDate(releaseGroup.FirstReleaseDate);
        (DateOnly? reReleaseDate, int? reReleaseYear) = ParseDate(release?.Date);
        if (reReleaseDate is not null && originalDate is not null && reReleaseDate.Value.Year == originalDate.Value.Year)
        {
            reReleaseDate = null;
            reReleaseYear = null;
        }

        string? releaseArtistId = releaseGroup.ArtistCredit.Select(credit => credit.Artist?.Id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id))
            ?? release?.ArtistCredit.Select(credit => credit.Artist?.Id).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        List<MediaContributorDto> contributors = MapArtistCredit(releaseGroup.ArtistCredit);
        if (contributors.Count == 0 && release is not null)
            contributors = MapArtistCredit(release.ArtistCredit);
        if (release is not null)
            AddContributors(contributors, MapContributorsFromRelations(release.Relations));

        List<MusicBrainzMediumResponse> media = release?.Media ?? [];
        int totalTracks = media.Sum(medium => medium.TrackCount);
        // A MusicBrainz release can carry several labels and catalog numbers: the label stores the first one, while every catalog number is kept.
        string? label = release?.LabelInfo.FirstOrDefault()?.Label?.Name;
        List<string> catalogNumbers = [.. (release?.LabelInfo ?? [])
            .Select(labelInfo => NullIfWhiteSpace(labelInfo.CatalogNumber))
            .Where(catalogNumber => catalogNumber is not null)
            .Select(catalogNumber => catalogNumber!)
            .Distinct(StringComparer.OrdinalIgnoreCase)];

        return new AlbumMetadataDto(
            Title: NullIfWhiteSpace(releaseGroup.Title) ?? NullIfWhiteSpace(release?.Title),
            OriginalTitle: null,
            Description: null,
            Disambiguation: NullIfWhiteSpace(releaseGroup.Disambiguation),
            ReleaseInfo: new ReleaseInfoDto(
                OriginalReleaseDate: originalDate,
                OriginalReleaseYear: originalYear,
                ReReleaseDate: reReleaseDate,
                ReReleaseYear: reReleaseYear,
                ReleaseCountry: MapReleaseCountry(release?.Country),
                ReleaseVersion: NullIfWhiteSpace(release?.Disambiguation)),
            Language: MapLanguage(release?.TextRepresentation?.Language),
            OriginalLanguage: null,
            Tags: MapTags(releaseGroup.Tags),
            Genres: MapGenres(releaseGroup.Genres),
            Script: NullIfWhiteSpace(release?.TextRepresentation?.Script),
            ReleaseTypes: MapReleaseTypes(releaseGroup.PrimaryType, releaseGroup.SecondaryTypes),
            ReleaseStatus: MapReleaseStatus(release?.Status),
            MediaFormat: MapMediaFormat(media.FirstOrDefault()?.Format),
            Packaging: MapPackaging(release?.Packaging),
            TotalDiscs: media.Count > 0 ? media.Count : null,
            TotalTracks: totalTracks > 0 ? totalTracks : null,
            Barcode: NullIfWhiteSpace(release?.Barcode),
            CatalogNumbers: catalogNumbers,
            Label: NullIfWhiteSpace(label),
            ASIN: NullIfWhiteSpace(release?.Asin),
            MusicBrainzReleaseId: ParseGuid(release?.Id),
            MusicBrainzReleaseGroupId: ParseGuid(releaseGroup.Id),
            MusicBrainzReleaseArtistId: ParseGuid(releaseArtistId),
            Contributors: contributors,
            Ratings: MapRating(releaseGroup.Rating),
            ReleaseTitle: NullIfWhiteSpace(release?.Title));
    }

    /// <summary>
    /// Maps a MusicBrainz recording, and optionally its work and release, into track metadata.
    /// </summary>
    /// <param name="recording">The recording response to map.</param>
    /// <param name="work">The work the recording is an instance of, or <see langword="null"/> when the recording has no work.</param>
    /// <param name="release">The release the recording appears on, or <see langword="null"/> when it is not known.</param>
    /// <returns>The mapped track metadata.</returns>
    public static AudioMetadataDto MapTrack(MusicBrainzRecordingResponse recording, MusicBrainzWorkResponse? work, MusicBrainzReleaseResponse? release)
    {
        (DateOnly? releaseDate, int? releaseYear) = ParseDate(recording.FirstReleaseDate);
        // A recording embedded in a release lookup does not carry its own first release date, so the date of the release the track appears on is used
        // instead, rather than leaving the track without any release date at all.
        if (releaseYear is null)
            (releaseDate, releaseYear) = ParseDate(release?.Date);

        List<MediaContributorDto> contributors = MapArtistCredit(recording.ArtistCredit);
        AddContributors(contributors, MapContributorsFromRelations(recording.Relations));
        if (work is not null)
            AddContributors(contributors, MapContributorsFromRelations(work.Relations));

        return new AudioMetadataDto(
            Title: NullIfWhiteSpace(recording.Title),
            OriginalTitle: null,
            Description: null,
            Disambiguation: NullIfWhiteSpace(recording.Disambiguation),
            ReleaseInfo: new ReleaseInfoDto(
                OriginalReleaseDate: releaseDate,
                OriginalReleaseYear: releaseYear,
                ReReleaseDate: null,
                ReReleaseYear: null,
                ReleaseCountry: null,
                ReleaseVersion: null),
            Language: MapLanguage(release?.TextRepresentation?.Language),
            OriginalLanguage: null,
            Tags: MapTags(recording.Tags),
            Genres: MapGenres(recording.Genres),
            Script: NullIfWhiteSpace(release?.TextRepresentation?.Script),
            Key: null,
            Bpm: null,
            IsVideo: recording.IsVideo ?? false,
            Work: work is null ? null : MapWork(work),
            Isrcs: [.. recording.Isrcs.Where(isrc => !string.IsNullOrWhiteSpace(isrc)).Select(isrc => new IsrcDto(isrc))],
            Moods: null,
            DurationInSeconds: recording.Length is long length && length > 0 ? (int)(length / 1000) : null,
            SampleRate: null,
            Channels: null,
            BitDepth: null,
            AudioCodec: null,
            Bitrate: null,
            MusicBrainzRecordingId: ParseGuid(recording.Id),
            MusicBrainzTrackId: null,
            Contributors: contributors,
            Ratings: MapRating(recording.Rating));
    }

    /// <summary>
    /// Maps a MusicBrainz work into a work DTO.
    /// </summary>
    /// <param name="work">The work response to map.</param>
    /// <returns>The mapped work.</returns>
    public static MusicWorkDto MapWork(MusicBrainzWorkResponse work)
    {
        return new MusicWorkDto(
            MusicBrainzWorkId: ParseGuid(work.Id),
            Title: NullIfWhiteSpace(work.Title),
            Type: NullIfWhiteSpace(work.Type),
            Languages: [.. work.Languages.Select(MapLanguage).Where(language => language is not null).Select(language => language!)],
            Iswcs: [.. work.Iswcs.Where(iswc => !string.IsNullOrWhiteSpace(iswc))]);
    }

    /// <summary>
    /// Maps a MusicBrainz area into an area DTO.
    /// </summary>
    /// <param name="area">The area response to map.</param>
    /// <returns>The mapped area, or <see langword="null"/> when no area is provided.</returns>
    public static MusicAreaDto? MapArea(MusicBrainzAreaResponse? area)
    {
        if (area is null)
            return null;

        string? isoCode = area.Iso3166Part1Codes.FirstOrDefault() ?? area.Iso3166Part2Codes.FirstOrDefault();
        return new MusicAreaDto(
            MusicBrainzAreaId: ParseGuid(area.Id),
            Name: NullIfWhiteSpace(area.Name),
            SortName: NullIfWhiteSpace(area.SortName),
            Disambiguation: NullIfWhiteSpace(area.Disambiguation),
            Type: NullIfWhiteSpace(area.Type),
            Iso3166Code: NullIfWhiteSpace(isoCode));
    }

    /// <summary>
    /// Maps a MusicBrainz alias into an alias DTO.
    /// </summary>
    /// <param name="alias">The alias response to map.</param>
    /// <returns>The mapped alias.</returns>
    private static MusicArtistAliasDto MapAlias(MusicBrainzAliasResponse alias)
    {
        return new MusicArtistAliasDto(
            Name: NullIfWhiteSpace(alias.Name),
            SortName: NullIfWhiteSpace(alias.SortName),
            Type: NullIfWhiteSpace(alias.Type),
            Locale: NullIfWhiteSpace(alias.Locale),
            IsPrimary: alias.IsPrimary ?? false,
            BeginDate: ParseDate(alias.Begin).Date,
            EndDate: ParseDate(alias.End).Date,
            IsEnded: alias.IsEnded ?? false);
    }

    /// <summary>
    /// Maps the genres of a MusicBrainz entity into genre DTOs.
    /// </summary>
    /// <param name="genres">The genre responses to map.</param>
    /// <returns>The mapped genre DTOs.</returns>
    private static List<GenreDto> MapGenres(IReadOnlyCollection<MusicBrainzTagResponse> genres)
    {
        return [.. genres
            .Where(genre => !string.IsNullOrWhiteSpace(genre.Name))
            .Select(genre => new GenreDto(genre.Name))
            .DistinctBy(genre => genre.Name, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// Maps the tags of a MusicBrainz entity into tag DTOs.
    /// </summary>
    /// <param name="tags">The tag responses to map.</param>
    /// <returns>The mapped tag DTOs.</returns>
    private static List<TagDto> MapTags(IReadOnlyCollection<MusicBrainzTagResponse> tags)
    {
        return [.. tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag.Name))
            .Select(tag => new TagDto(tag.Name))
            .DistinctBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// Maps the rating of a MusicBrainz entity into a rating DTO.
    /// </summary>
    /// <param name="rating">The rating response to map.</param>
    /// <returns>The mapped rating DTO, or an empty list when no rating is provided.</returns>
    private static List<AudioRatingDto> MapRating(MusicBrainzRatingResponse? rating)
    {
        if (rating is null || (rating.Value is null && rating.VotesCount is null))
            return [];
        return [new AudioRatingDto(rating.Value, 5m, AudioRatingSource.MusicBrainz, rating.VotesCount)];
    }

    /// <summary>
    /// Maps an ISO 639-3 language code into a language DTO.
    /// </summary>
    /// <param name="rawCode">The code to map.</param>
    /// <returns>The mapped language, or <see langword="null"/> when the code is empty.</returns>
    private static LanguageInfoDto? MapLanguage(string? rawCode)
    {
        if (string.IsNullOrWhiteSpace(rawCode))
            return null;

        string code = rawCode.Trim().ToLowerInvariant();
        // A language code can list several languages, such as "eng/ara"; the first one is the language of the release.
        string primaryCode = code.Split('/').Select(part => part.Trim()).FirstOrDefault(part => part.Length > 0) ?? code;

        // These ISO 639-3 codes describe a language absence rather than a culture, so they cannot be resolved through a culture.
        switch (primaryCode)
        {
            case "zxx":
                return new LanguageInfoDto(primaryCode, "No linguistic content", null);
            case "mul":
                return new LanguageInfoDto(primaryCode, "Multiple languages", null);
            case "und":
                return new LanguageInfoDto(primaryCode, "Undetermined", null);
            default:
                break;
        }

        // Only neutral cultures are matched, because a specific culture would resolve to an arbitrary regional name such as "German (Austria)".
        return s_neutralCulturesByCode.TryGetValue(primaryCode, out CultureInfo? culture)
            ? new LanguageInfoDto(culture.TwoLetterISOLanguageName, culture.EnglishName, culture.NativeName)
            : new LanguageInfoDto(primaryCode, primaryCode, null);
    }

    /// <summary>
    /// Builds the lookup of the process' neutral cultures, indexed by both their two letter and three letter ISO 639 codes.
    /// </summary>
    /// <returns>The neutral cultures, indexed by their ISO 639 codes.</returns>
    private static IReadOnlyDictionary<string, CultureInfo> BuildNeutralCultureLookup()
    {
        Dictionary<string, CultureInfo> culturesByCode = new(StringComparer.OrdinalIgnoreCase);
        foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
            // A culture without a two letter ISO 639 code is not a language, so it is skipped; the two letter code is the one carried by the DTOs.
            if (culture.TwoLetterISOLanguageName.Length != 2)
                continue;
            culturesByCode.TryAdd(culture.ThreeLetterISOLanguageName, culture);
            culturesByCode.TryAdd(culture.TwoLetterISOLanguageName, culture);
        }
        return culturesByCode;
    }

    /// <summary>
    /// Maps the artist credit of a MusicBrainz entity into attributed performing contributors.
    /// </summary>
    /// <param name="artistCredit">The artist credit to map.</param>
    /// <returns>The mapped contributors.</returns>
    private static List<MediaContributorDto> MapArtistCredit(IReadOnlyCollection<MusicBrainzArtistCreditResponse> artistCredit)
    {
        List<MediaContributorDto> contributors = [];
        foreach (MusicBrainzArtistCreditResponse credit in artistCredit)
        {
            string? name = NullIfWhiteSpace(credit.Name) ?? NullIfWhiteSpace(credit.Artist?.Name);
            if (name is null)
                continue;
            AddContributor(contributors, name, MediaContributorRole.Performer);
        }
        return contributors;
    }

    /// <summary>
    /// Maps the artist relations of a MusicBrainz entity into contributors, excluding the URL relations.
    /// </summary>
    /// <param name="relations">The relations to map.</param>
    /// <returns>The mapped contributors.</returns>
    private static List<MediaContributorDto> MapContributorsFromRelations(IReadOnlyCollection<MusicBrainzRelationResponse> relations)
    {
        List<MediaContributorDto> contributors = [];
        foreach (MusicBrainzRelationResponse relation in relations)
        {
            if (!string.Equals(relation.TargetType, "artist", StringComparison.OrdinalIgnoreCase))
                continue;

            string? name = NullIfWhiteSpace(relation.Artist?.Name);
            if (name is null)
                continue;
            AddContributor(contributors, name, MapRole(relation.Type, relation.Attributes));
        }
        return contributors;
    }

    /// <summary>
    /// Maps the relations of an artist into its contributing members, and its official homepage.
    /// </summary>
    /// <param name="relations">The relations of the artist.</param>
    /// <returns>The mapped contributors, and the official homepage of the artist.</returns>
    private static (List<MediaContributorDto> Contributors, string? Website) MapArtistRelations(IReadOnlyCollection<MusicBrainzRelationResponse> relations)
    {
        List<MediaContributorDto> contributors = [];
        string? website = null;
        foreach (MusicBrainzRelationResponse relation in relations)
        {
            if (string.Equals(relation.TargetType, "url", StringComparison.OrdinalIgnoreCase))
            {
                if (website is null && string.Equals(relation.Type, "official homepage", StringComparison.OrdinalIgnoreCase))
                    website = NullIfWhiteSpace(relation.Url?.Resource);
                continue;
            }

            if (!string.Equals(relation.TargetType, "artist", StringComparison.OrdinalIgnoreCase))
                continue;

            string? name = NullIfWhiteSpace(relation.Artist?.Name);
            if (name is null)
                continue;
            AddContributor(contributors, name, MapRole(relation.Type, relation.Attributes));
        }
        return (contributors, website);
    }

    /// <summary>
    /// Adds a contributor to the provided list, unless an identical name and role pair is already present.
    /// </summary>
    /// <param name="contributors">The contributors collected so far.</param>
    /// <param name="displayName">The display name of the contributor.</param>
    /// <param name="role">The role the contributor played.</param>
    private static void AddContributor(List<MediaContributorDto> contributors, string displayName, MediaContributorRole role)
    {
        if (contributors.Any(contributor =>
                string.Equals(contributor.Name?.DisplayName, displayName, StringComparison.OrdinalIgnoreCase) &&
                contributor.Role == role))
            return;
        contributors.Add(new MediaContributorDto(new MediaContributorNameDto(displayName, null), role));
    }

    /// <summary>
    /// Adds the <paramref name="incoming"/> contributors to <paramref name="contributors"/>, skipping the duplicate name and role pairs.
    /// </summary>
    /// <param name="contributors">The contributors collected so far.</param>
    /// <param name="incoming">The contributors to add.</param>
    private static void AddContributors(List<MediaContributorDto> contributors, IReadOnlyCollection<MediaContributorDto> incoming)
    {
        foreach (MediaContributorDto contributor in incoming)
        {
            if (contributor.Name?.DisplayName is not string displayName)
                continue;
            AddContributor(contributors, displayName, contributor.Role ?? MediaContributorRole.Other);
        }
    }

    /// <summary>
    /// Maps a MusicBrainz relationship type, and its attributes, into a canonical media contributor role.
    /// </summary>
    /// <param name="relationType">The relationship type to map.</param>
    /// <param name="attributes">The attributes of the relationship.</param>
    /// <returns>The mapped contributor role.</returns>
    private static MediaContributorRole MapRole(string? relationType, IReadOnlyCollection<string> attributes)
    {
        // An instrument attribute is more specific than the generic relationship type, so it is preferred.
        foreach (string attribute in attributes)
        {
            MediaContributorRole? attributed = MapInstrument(attribute);
            if (attributed is not null)
                return attributed.Value;
        }

        if (string.IsNullOrWhiteSpace(relationType))
            return MediaContributorRole.Other;

        string type = relationType.Trim().ToLowerInvariant();
        if (type.Contains("executive producer", StringComparison.Ordinal))
            return MediaContributorRole.ExecutiveProducer;
        if (type.Contains("producer", StringComparison.Ordinal))
            return MediaContributorRole.Producer;
        if (type.Contains("remixer", StringComparison.Ordinal))
            return MediaContributorRole.Remixer;
        if (type.Contains("mix", StringComparison.Ordinal))
            return MediaContributorRole.Mixer;
        if (type.Contains("engineer", StringComparison.Ordinal) || type.Contains("mastering", StringComparison.Ordinal))
            return MediaContributorRole.Engineer;
        if (type.Contains("composer", StringComparison.Ordinal))
            return MediaContributorRole.Composer;
        if (type.Contains("writer", StringComparison.Ordinal))
            return MediaContributorRole.Author;
        if (type.Contains("lyricist", StringComparison.Ordinal))
            return MediaContributorRole.Lyricist;
        if (type.Contains("librettist", StringComparison.Ordinal))
            return MediaContributorRole.Lyricist;
        if (type.Contains("arranger", StringComparison.Ordinal))
            return MediaContributorRole.Arranger;
        if (type.Contains("conductor", StringComparison.Ordinal))
            return MediaContributorRole.Conductor;
        if (type.Contains("orchestrator", StringComparison.Ordinal))
            return MediaContributorRole.Orchestrator;
        if (type.Contains("backing vocals", StringComparison.Ordinal))
            return MediaContributorRole.BackingVocals;
        if (type.Contains("vocal", StringComparison.Ordinal))
            return MediaContributorRole.Vocals;
        if (type.Contains("member of band", StringComparison.Ordinal) || type.Contains("performer", StringComparison.Ordinal) || type.Contains("orchestra", StringComparison.Ordinal) || type.Contains("choir", StringComparison.Ordinal))
            return MediaContributorRole.Performer;
        return MediaContributorRole.Other;
    }

    /// <summary>
    /// Maps a MusicBrainz instrument attribute into a canonical media contributor role.
    /// </summary>
    /// <param name="attribute">The instrument attribute to map.</param>
    /// <returns>The mapped contributor role, or <see langword="null"/> when the attribute is not an instrument.</returns>
    private static MediaContributorRole? MapInstrument(string? attribute)
    {
        if (string.IsNullOrWhiteSpace(attribute))
            return null;

        string value = attribute.Trim().ToLowerInvariant();
        if (value.Contains("backing vocal", StringComparison.Ordinal))
            return MediaContributorRole.BackingVocals;
        if (value.Contains("vocal", StringComparison.Ordinal))
            return MediaContributorRole.Vocals;
        if (value.Contains("bass guitar", StringComparison.Ordinal))
            return MediaContributorRole.BassGuitar;
        if (value.Contains("guitar", StringComparison.Ordinal))
            return MediaContributorRole.Guitar;
        if (value.Contains("drum", StringComparison.Ordinal))
            return MediaContributorRole.Drums;
        if (value.Contains("percussion", StringComparison.Ordinal))
            return MediaContributorRole.Percussion;
        if (value.Contains("keyboard", StringComparison.Ordinal))
            return MediaContributorRole.Keyboards;
        if (value.Contains("piano", StringComparison.Ordinal))
            return MediaContributorRole.Piano;
        if (value.Contains("synthesizer", StringComparison.Ordinal) || value.Contains("synth", StringComparison.Ordinal))
            return MediaContributorRole.Synthesizer;
        if (value.Contains("string", StringComparison.Ordinal) || value.Contains("violin", StringComparison.Ordinal) || value.Contains("cello", StringComparison.Ordinal) || value.Contains("viola", StringComparison.Ordinal))
            return MediaContributorRole.Strings;
        if (value.Contains("brass", StringComparison.Ordinal) || value.Contains("trumpet", StringComparison.Ordinal) || value.Contains("trombone", StringComparison.Ordinal) || value.Contains("horn", StringComparison.Ordinal))
            return MediaContributorRole.Brass;
        if (value.Contains("woodwind", StringComparison.Ordinal) || value.Contains("flute", StringComparison.Ordinal) || value.Contains("saxophone", StringComparison.Ordinal) || value.Contains("clarinet", StringComparison.Ordinal))
            return MediaContributorRole.Woodwinds;
        return null;
    }

    /// <summary>
    /// Maps a MusicBrainz artist type into the application enum.
    /// </summary>
    /// <param name="type">The artist type to map.</param>
    /// <returns>The mapped artist type, or <see langword="null"/> when the type is unknown.</returns>
    private static MusicArtistType? MapArtistType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return null;
        return type.Trim().ToLowerInvariant() switch
        {
            "person" => MusicArtistType.Person,
            "group" => MusicArtistType.Group,
            "orchestra" => MusicArtistType.Orchestra,
            "choir" => MusicArtistType.Choir,
            "character" => MusicArtistType.Character,
            "other" => MusicArtistType.Other,
            _ => null
        };
    }

    /// <summary>
    /// Maps a MusicBrainz gender into the application enum.
    /// </summary>
    /// <param name="gender">The gender to map.</param>
    /// <returns>The mapped gender, or <see langword="null"/> when the gender is unknown.</returns>
    private static MusicArtistGender? MapArtistGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            return null;
        return gender.Trim().ToLowerInvariant() switch
        {
            "male" => MusicArtistGender.Male,
            "female" => MusicArtistGender.Female,
            "other" => MusicArtistGender.Other,
            "not applicable" => MusicArtistGender.NotApplicable,
            _ => null
        };
    }

    /// <summary>
    /// Maps the primary and secondary types of a release group into the application enum.
    /// </summary>
    /// <param name="primaryType">The primary type of the release group.</param>
    /// <param name="secondaryTypes">The secondary types of the release group.</param>
    /// <returns>The mapped release types.</returns>
    private static List<MusicReleaseType> MapReleaseTypes(string? primaryType, IReadOnlyCollection<string> secondaryTypes)
    {
        List<MusicReleaseType> releaseTypes = [];
        MusicReleaseType? primary = MapReleaseType(primaryType);
        if (primary is not null)
            releaseTypes.Add(primary.Value);

        foreach (string secondaryType in secondaryTypes)
        {
            MusicReleaseType? mapped = MapReleaseType(secondaryType);
            if (mapped is not null && !releaseTypes.Contains(mapped.Value))
                releaseTypes.Add(mapped.Value);
        }
        return releaseTypes;
    }

    /// <summary>
    /// Maps a single MusicBrainz release type into the application enum.
    /// </summary>
    /// <param name="type">The release type to map.</param>
    /// <returns>The mapped release type, or <see langword="null"/> when the type is unknown.</returns>
    private static MusicReleaseType? MapReleaseType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return null;
        return type.Trim().ToLowerInvariant() switch
        {
            "album" => MusicReleaseType.Album,
            "single" => MusicReleaseType.Single,
            "ep" => MusicReleaseType.Ep,
            "broadcast" => MusicReleaseType.Broadcast,
            "other" => MusicReleaseType.Other,
            "compilation" => MusicReleaseType.Compilation,
            "live" => MusicReleaseType.Live,
            "soundtrack" => MusicReleaseType.Soundtrack,
            "spokenword" => MusicReleaseType.SpokenWord,
            "audio drama" => MusicReleaseType.AudioDrama,
            "audiobook" => MusicReleaseType.AudioBook,
            "demo" => MusicReleaseType.Demo,
            "dj-mix" => MusicReleaseType.DjMix,
            "field recording" => MusicReleaseType.FieldRecording,
            "interview" => MusicReleaseType.Interview,
            "mixtape/street" => MusicReleaseType.Mixtape,
            "remix" => MusicReleaseType.Remix,
            _ => null
        };
    }

    /// <summary>
    /// Maps a MusicBrainz release status into the application enum.
    /// </summary>
    /// <param name="status">The release status to map.</param>
    /// <returns>The mapped release status, or <see langword="null"/> when the status is unknown.</returns>
    private static MusicReleaseStatus? MapReleaseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;
        return status.Trim().ToLowerInvariant() switch
        {
            "official" => MusicReleaseStatus.Official,
            "promotion" => MusicReleaseStatus.Promotion,
            "bootleg" => MusicReleaseStatus.Bootleg,
            "pseudo-release" => MusicReleaseStatus.PseudoRelease,
            "withdrawn" => MusicReleaseStatus.Withdrawn,
            "cancelled" => MusicReleaseStatus.Cancelled,
            "expunged" => MusicReleaseStatus.Expunged,
            _ => null
        };
    }

    /// <summary>
    /// Maps a MusicBrainz medium format into the application enum.
    /// </summary>
    /// <param name="format">The format to map.</param>
    /// <returns>The mapped media format, or <see langword="null"/> when the format is unknown.</returns>
    private static MusicMediaFormat? MapMediaFormat(string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return null;
        return format.Trim().ToLowerInvariant() switch
        {
            "cd" => MusicMediaFormat.CD,
            "vinyl" => MusicMediaFormat.Vinyl,
            "12\" vinyl" => MusicMediaFormat.Inch12Vinyl,
            "10\" vinyl" => MusicMediaFormat.Inch10Vinyl,
            "7\" vinyl" => MusicMediaFormat.Inch7Vinyl,
            "digital media" => MusicMediaFormat.DigitalMedia,
            "cassette" => MusicMediaFormat.Cassette,
            "dvd" => MusicMediaFormat.DVD,
            "dvd-video" => MusicMediaFormat.DVDVideo,
            "dvd-audio" => MusicMediaFormat.DVDAudio,
            "sacd" => MusicMediaFormat.SACD,
            "hybrid sacd" => MusicMediaFormat.HybridSACD,
            "blu-ray" => MusicMediaFormat.BluRay,
            "minidisc" => MusicMediaFormat.MiniDisc,
            "8cm cd" => MusicMediaFormat.Cm8CD,
            "dat" => MusicMediaFormat.DAT,
            "other" => MusicMediaFormat.Other,
            _ => MusicMediaFormat.Other
        };
    }

    /// <summary>
    /// Maps a MusicBrainz release packaging into the application enum.
    /// </summary>
    /// <param name="packaging">The packaging to map.</param>
    /// <returns>The mapped packaging, or <see langword="null"/> when the packaging is unknown.</returns>
    private static MusicReleasePackaging? MapPackaging(string? packaging)
    {
        if (string.IsNullOrWhiteSpace(packaging))
            return null;
        return packaging.Trim().ToLowerInvariant() switch
        {
            "book" => MusicReleasePackaging.Book,
            "box" => MusicReleasePackaging.Box,
            "cardboard/paper sleeve" => MusicReleasePackaging.CardboardPaperSleeve,
            "cassette case" => MusicReleasePackaging.CassetteCase,
            "clamshell case" => MusicReleasePackaging.ClamshellCase,
            "digibook" => MusicReleasePackaging.Digibook,
            "digifile" => MusicReleasePackaging.Digifile,
            "digipak" => MusicReleasePackaging.Digipak,
            "discbox slider" => MusicReleasePackaging.DiscboxSlider,
            "fatbox" => MusicReleasePackaging.Fatbox,
            "gatefold cover" => MusicReleasePackaging.GatefoldCover,
            "jewel case" => MusicReleasePackaging.JewelCase,
            "keep case" => MusicReleasePackaging.KeepCase,
            "longbox" => MusicReleasePackaging.Longbox,
            "metal tin" => MusicReleasePackaging.MetalTin,
            "plastic sleeve" => MusicReleasePackaging.PlasticSleeve,
            "slidepack" => MusicReleasePackaging.Slidepack,
            "slim jewel case" => MusicReleasePackaging.SlimJewelCase,
            "slipcase" => MusicReleasePackaging.Slipcase,
            "snap case" => MusicReleasePackaging.SnapCase,
            "snappack" => MusicReleasePackaging.SnapPack,
            "super jewel box" => MusicReleasePackaging.SuperJewelBox,
            "none" => MusicReleasePackaging.None,
            "other" => MusicReleasePackaging.Other,
            _ => MusicReleasePackaging.Other
        };
    }

    /// <summary>
    /// Maps an ISO 3166-1 alpha-2 country code into the application enum.
    /// </summary>
    /// <param name="country">The country code to map.</param>
    /// <returns>The mapped release country, or <see langword="null"/> when the code is unknown.</returns>
    private static ReleaseCountry? MapReleaseCountry(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return null;
        return Enum.TryParse(country.Trim(), ignoreCase: true, out ReleaseCountry releaseCountry) ? releaseCountry : null;
    }

    /// <summary>
    /// Parses a MusicBrainz identifier into a <see cref="Guid"/>.
    /// </summary>
    /// <param name="value">The MusicBrainz identifier to parse.</param>
    /// <returns>The parsed identifier, or <see langword="null"/> when the value is not a valid identifier.</returns>
    private static Guid? ParseGuid(string? value)
    {
        return Guid.TryParse(value, out Guid parsed) ? parsed : null;
    }

    /// <summary>
    /// Parses a MusicBrainz date string into a date and a year.
    /// </summary>
    /// <param name="value">The date string to parse.</param>
    /// <returns>The parsed date and year, or <see langword="null"/> values when the string could not be parsed.</returns>
    private static (DateOnly? Date, int? Year) ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, null);

        if (DateOnly.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly exact))
            return (exact, exact.Year);

        Match yearMatch = YearPattern().Match(value);
        return yearMatch.Success && int.TryParse(yearMatch.Value, CultureInfo.InvariantCulture, out int year) ? (null, year) : (null, null);
    }

    /// <summary>
    /// Returns the trimmed value, or <see langword="null"/> when the value is empty or white space.
    /// </summary>
    /// <param name="value">The value to process.</param>
    /// <returns>The trimmed value, or <see langword="null"/>.</returns>
    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
