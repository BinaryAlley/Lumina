#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Plugins.ID3.Common.Models.DTO.Tags;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#endregion

namespace Lumina.Plugins.ID3.Core.Mapping;

/// <summary>
/// Maps the metadata read from the embedded tags of an audio file into the application's media metadata DTOs.
/// </summary>
internal static class Id3TagMapper
{
    /// <summary>
    /// The neutral cultures of the application, keyed by both their two letter and three letter language codes, so that a language code is resolved without enumerating the cultures of the system for every mapped file.
    /// </summary>
    private static readonly Dictionary<string, CultureInfo> s_culturesByLanguageCode = BuildCulturesByLanguageCode();

    /// <summary>
    /// Builds the map of the neutral cultures of the application, keyed by their language codes.
    /// </summary>
    /// <returns>The cultures, keyed by both their two letter and three letter language codes.</returns>
    private static Dictionary<string, CultureInfo> BuildCulturesByLanguageCode()
    {
        Dictionary<string, CultureInfo> cultures = new(StringComparer.OrdinalIgnoreCase);
        foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
            if (culture.ThreeLetterISOLanguageName.Length == 3 && !cultures.ContainsKey(culture.ThreeLetterISOLanguageName))
                cultures[culture.ThreeLetterISOLanguageName] = culture;
            if (culture.TwoLetterISOLanguageName.Length == 2 && !cultures.ContainsKey(culture.TwoLetterISOLanguageName))
                cultures[culture.TwoLetterISOLanguageName] = culture;
        }
        return cultures;
    }

    /// <summary>
    /// Maps the tags of an audio file into artist metadata.
    /// </summary>
    /// <param name="data">The tags of a track of the artist.</param>
    /// <param name="artistName">The name of the artist the metadata is mapped for, when known, used to select the matching credited artist.</param>
    /// <returns>The mapped artist metadata.</returns>
    public static ArtistMetadataDto MapArtist(Id3TagDto data, string? artistName)
    {
        bool doesMatchTrackArtist = !string.IsNullOrWhiteSpace(artistName) && data.TrackArtists.Any(artist => string.Equals(artist, artistName, StringComparison.OrdinalIgnoreCase));
        bool doesMatchAlbumArtist = !string.IsNullOrWhiteSpace(artistName) && data.AlbumArtists.Any(artist => string.Equals(artist, artistName, StringComparison.OrdinalIgnoreCase));

        string? mappedName;
        string? mappedSortName;
        Guid? musicBrainzArtistId;
        if (doesMatchTrackArtist)
        {
            mappedName = NullIfWhiteSpace(artistName);
            mappedSortName = data.TrackArtistSortName ?? data.AlbumArtistSortName;
            musicBrainzArtistId = ParseGuid(data.MusicBrainzArtistId) ?? ParseGuid(data.MusicBrainzReleaseArtistId);
        }
        else if (doesMatchAlbumArtist)
        {
            mappedName = NullIfWhiteSpace(artistName);
            mappedSortName = data.AlbumArtistSortName ?? data.TrackArtistSortName;
            musicBrainzArtistId = ParseGuid(data.MusicBrainzReleaseArtistId) ?? ParseGuid(data.MusicBrainzArtistId);
        }
        else
        {
            mappedName = NullIfWhiteSpace(data.AlbumArtists.FirstOrDefault()) ?? NullIfWhiteSpace(data.TrackArtists.FirstOrDefault());
            mappedSortName = data.AlbumArtistSortName ?? data.TrackArtistSortName;
            musicBrainzArtistId = ParseGuid(data.MusicBrainzArtistId) ?? ParseGuid(data.MusicBrainzReleaseArtistId);
        }

        return new ArtistMetadataDto(
            Name: mappedName,
            SortName: mappedSortName,
            Disambiguation: null,
            Type: null,
            Gender: null,
            Country: null,
            Area: null,
            BeginArea: null,
            EndArea: null,
            LifeSpanBegin: null,
            LifeSpanEnd: null,
            IsEnded: false,
            Website: data.Website,
            MusicBrainzArtistId: musicBrainzArtistId,
            Ipis: null,
            Isnis: null,
            Aliases: null,
            Genres: null,
            Tags: null,
            Ratings: null,
            Contributors: null);
    }

    /// <summary>
    /// Maps the tags of an audio file into album metadata.
    /// </summary>
    /// <param name="data">The tags of a track of the album.</param>
    /// <returns>The mapped album metadata.</returns>
    public static AlbumMetadataDto MapAlbum(Id3TagDto data)
    {
        return new AlbumMetadataDto(
            Title: NullIfWhiteSpace(data.Album),
            OriginalTitle: null,
            Description: null,
            Disambiguation: null,
            ReleaseInfo: BuildReleaseInfo(data),
            Language: MapLanguage(data.Language),
            OriginalLanguage: null,
            Tags: null,
            Genres: MapGenres(data.Genres),
            Script: NullIfWhiteSpace(data.Script),
            ReleaseTypes: MapReleaseTypes(data.ReleaseTypes),
            ReleaseStatus: MapReleaseStatus(data.ReleaseStatus),
            MediaFormat: MapMediaFormat(data.MediaFormat),
            Packaging: MapPackaging(data.Packaging),
            TotalDiscs: data.DiscCount > 0 ? (int)data.DiscCount : null,
            TotalTracks: data.TrackCount > 0 ? (int)data.TrackCount : null,
            Barcode: NullIfWhiteSpace(data.Barcode),
            CatalogNumbers: data.CatalogNumbers.Count > 0 ? [.. data.CatalogNumbers] : null,
            Label: NullIfWhiteSpace(data.Label),
            ASIN: NullIfWhiteSpace(data.Asin),
            MusicBrainzReleaseId: ParseGuid(data.MusicBrainzReleaseId),
            MusicBrainzReleaseGroupId: ParseGuid(data.MusicBrainzReleaseGroupId),
            MusicBrainzReleaseArtistId: ParseGuid(data.MusicBrainzReleaseArtistId),
            Contributors: MapAlbumContributors(data),
            Ratings: null,
            ReleaseTitle: null);
    }

    /// <summary>
    /// Maps the tags of an audio file into track metadata.
    /// </summary>
    /// <param name="data">The tags of the track.</param>
    /// <returns>The mapped track metadata.</returns>
    public static AudioMetadataDto MapTrack(Id3TagDto data)
    {
        return new AudioMetadataDto(
            Title: NullIfWhiteSpace(data.Title),
            OriginalTitle: null,
            Description: null,
            Disambiguation: null,
            ReleaseInfo: BuildReleaseInfo(data),
            Language: MapLanguage(data.Language),
            OriginalLanguage: null,
            Tags: null,
            Genres: MapGenres(data.Genres),
            Script: NullIfWhiteSpace(data.Script),
            Key: MapKey(data.MusicKey),
            Bpm: data.Bpm,
            IsVideo: false,
            Work: MapWork(data),
            Isrcs: MapIsrcs(data.Isrcs),
            Moods: MapMoods(data.Moods),
            DurationInSeconds: data.DurationInSeconds,
            SampleRate: data.SampleRate,
            Channels: data.Channels,
            BitDepth: data.BitDepth,
            AudioCodec: NullIfWhiteSpace(data.AudioCodec),
            Bitrate: data.Bitrate,
            MusicBrainzRecordingId: ParseGuid(data.MusicBrainzRecordingId),
            MusicBrainzTrackId: ParseGuid(data.MusicBrainzReleaseTrackId),
            Contributors: MapTrackContributors(data),
            Ratings: null);
    }

    /// <summary>
    /// Builds the release information of the metadata from the tags, or <see langword="null"/> when the tags carry none of it.
    /// </summary>
    /// <param name="data">The tags of the track.</param>
    /// <returns>The release information, or <see langword="null"/> when the tags carry no release field.</returns>
    private static ReleaseInfoDto? BuildReleaseInfo(Id3TagDto data)
    {
        ReleaseCountry? releaseCountry = MapReleaseCountry(data.ReleaseCountry);
        if (data.OriginalReleaseDate is null && data.OriginalReleaseYear is null && releaseCountry is null)
            return null;

        return new ReleaseInfoDto(
            OriginalReleaseDate: data.OriginalReleaseDate,
            OriginalReleaseYear: data.OriginalReleaseYear,
            ReReleaseDate: null,
            ReReleaseYear: null,
            ReleaseCountry: releaseCountry,
            ReleaseVersion: null);
    }

    /// <summary>
    /// Maps the work of the track, or <see langword="null"/> when the tags carry none.
    /// </summary>
    /// <param name="data">The tags of the track.</param>
    /// <returns>The mapped work, or <see langword="null"/>.</returns>
    private static MusicWorkDto? MapWork(Id3TagDto data)
    {
        Guid? workId = ParseFirstGuid(data.MusicBrainzWorkId);
        string? workTitle = FirstValue(data.WorkTitle);
        // The work is only mapped when both its identifier and its title are known, because the domain requires both and would otherwise reject the whole track.
        if (workId is null || string.IsNullOrWhiteSpace(workTitle))
            return null;
        return new MusicWorkDto(
            MusicBrainzWorkId: workId,
            Title: workTitle,
            Type: null,
            Languages: null,
            Iswcs: null);
    }

    /// <summary>
    /// Maps the contributors credited on the track, each with its canonical role.
    /// </summary>
    /// <param name="data">The tags of the track.</param>
    /// <returns>The mapped contributors, or <see langword="null"/> when the tags credit none.</returns>
    private static List<MediaContributorDto>? MapTrackContributors(Id3TagDto data)
    {
        List<MediaContributorDto> contributors = [];
        foreach (string artist in data.TrackArtists)
            AddContributor(contributors, artist, MediaContributorRole.Performer);
        foreach (string composer in data.Composers)
            AddContributor(contributors, composer, MediaContributorRole.Composer);
        foreach (string lyricist in data.Lyricists)
            AddContributor(contributors, lyricist, MediaContributorRole.Lyricist);
        foreach (string conductor in data.Conductors)
            AddContributor(contributors, conductor, MediaContributorRole.Conductor);
        foreach (string remixer in data.Remixers)
            AddContributor(contributors, remixer, MediaContributorRole.Remixer);
        foreach ((string role, string name) in data.InvolvedPeople)
            AddContributor(contributors, name, MapRole(role));

        return contributors.Count > 0 ? contributors : null;
    }

    /// <summary>
    /// Maps the contributors credited on the album, which the tags only expose as the album artists.
    /// </summary>
    /// <param name="data">The tags of a track of the album.</param>
    /// <returns>The mapped contributors, or <see langword="null"/> when the tags credit none.</returns>
    private static List<MediaContributorDto>? MapAlbumContributors(Id3TagDto data)
    {
        List<string> artists = data.AlbumArtists.Count > 0 ? data.AlbumArtists : data.TrackArtists;
        List<MediaContributorDto> contributors = [];
        foreach (string artist in artists)
            AddContributor(contributors, artist, MediaContributorRole.Performer);
        return contributors.Count > 0 ? contributors : null;
    }

    /// <summary>
    /// Maps the genres of the metadata, without duplicates.
    /// </summary>
    /// <param name="genres">The genre names read from the tags.</param>
    /// <returns>The mapped genres, or <see langword="null"/> when there are none.</returns>
    private static List<GenreDto>? MapGenres(List<string> genres)
    {
        List<GenreDto> result = [.. genres
            .Where(genre => !string.IsNullOrWhiteSpace(genre))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(genre => new GenreDto(genre))];
        return result.Count > 0 ? result : null;
    }

    /// <summary>
    /// Maps the moods of the metadata, without duplicates.
    /// </summary>
    /// <param name="moods">The mood names read from the tags.</param>
    /// <returns>The mapped moods, or <see langword="null"/> when there are none.</returns>
    private static List<MoodDto>? MapMoods(List<string> moods)
    {
        List<MoodDto> result = [.. moods
            .Where(mood => !string.IsNullOrWhiteSpace(mood))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(mood => new MoodDto(mood))];
        return result.Count > 0 ? result : null;
    }

    /// <summary>
    /// Maps the ISRCs of the track, without duplicates.
    /// </summary>
    /// <param name="isrcs">The ISRC values read from the tags.</param>
    /// <returns>The mapped ISRCs, or <see langword="null"/> when there are none.</returns>
    private static List<IsrcDto>? MapIsrcs(List<string> isrcs)
    {
        List<IsrcDto> result = [.. isrcs
            .Where(isrc => !string.IsNullOrWhiteSpace(isrc))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(isrc => new IsrcDto(isrc))];
        return result.Count > 0 ? result : null;
    }

    /// <summary>
    /// Maps the types of the release into the application enum, without duplicates.
    /// </summary>
    /// <param name="releaseTypes">The free-form release types read from the tags.</param>
    /// <returns>The mapped release types, or <see langword="null"/> when there are none.</returns>
    private static List<MusicReleaseType>? MapReleaseTypes(List<string> releaseTypes)
    {
        List<MusicReleaseType> result = [];
        foreach (string releaseType in releaseTypes)
        {
            MusicReleaseType? mapped = MapReleaseType(releaseType);
            if (mapped is not null && !result.Contains(mapped.Value))
                result.Add(mapped.Value);
        }
        return result.Count > 0 ? result : null;
    }

    /// <summary>
    /// Maps a single free-form release type into the application enum.
    /// </summary>
    /// <param name="releaseType">The release type to map.</param>
    /// <returns>The mapped release type, or <see langword="null"/> when the type is unknown.</returns>
    private static MusicReleaseType? MapReleaseType(string? releaseType)
    {
        if (string.IsNullOrWhiteSpace(releaseType))
            return null;
        return releaseType.Trim().ToLowerInvariant() switch
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
    /// Maps a free-form release status into the application enum.
    /// </summary>
    /// <param name="releaseStatus">The release status to map.</param>
    /// <returns>The mapped release status, or <see langword="null"/> when the status is unknown.</returns>
    private static MusicReleaseStatus? MapReleaseStatus(string? releaseStatus)
    {
        if (string.IsNullOrWhiteSpace(releaseStatus))
            return null;
        return releaseStatus.Trim().ToLowerInvariant() switch
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
    /// Maps a free-form media format into the application enum.
    /// </summary>
    /// <param name="mediaFormat">The media format to map.</param>
    /// <returns>The mapped media format, or <see langword="null"/> when the format is unknown.</returns>
    private static MusicMediaFormat? MapMediaFormat(string? mediaFormat)
    {
        if (string.IsNullOrWhiteSpace(mediaFormat))
            return null;

        string value = mediaFormat.Trim().ToLowerInvariant();
        MusicMediaFormat? mapped = value switch
        {
            "12\" vinyl" => MusicMediaFormat.Inch12Vinyl,
            "10\" vinyl" => MusicMediaFormat.Inch10Vinyl,
            "7\" vinyl" => MusicMediaFormat.Inch7Vinyl,
            "3\" vinyl" => MusicMediaFormat.Inch3Vinyl,
            "vinyl" => MusicMediaFormat.Vinyl,
            "cd" => MusicMediaFormat.CD,
            "sacd" => MusicMediaFormat.SACD,
            "hybrid sacd" => MusicMediaFormat.HybridSACD,
            "digital media" => MusicMediaFormat.DigitalMedia,
            "cassette" => MusicMediaFormat.Cassette,
            "dvd" => MusicMediaFormat.DVD,
            "dvd-video" => MusicMediaFormat.DVDVideo,
            "dvd-audio" => MusicMediaFormat.DVDAudio,
            "blu-ray" => MusicMediaFormat.BluRay,
            "minidisc" => MusicMediaFormat.MiniDisc,
            "8cm cd" => MusicMediaFormat.Cm8CD,
            "dat" => MusicMediaFormat.DAT,
            "vinyl disc" => MusicMediaFormat.VinylDisc,
            "other" => MusicMediaFormat.Other,
            _ => null
        };
        if (mapped is not null)
            return mapped;

        // Fall back to a direct parse for the values whose free-form name already matches an enum member, ignoring the separators.
        string normalized = value.Replace("\"", string.Empty, StringComparison.Ordinal).Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);
        return Enum.TryParse(normalized, ignoreCase: true, out MusicMediaFormat parsed) ? parsed : null;
    }

    /// <summary>
    /// Maps a free-form packaging into the application enum.
    /// </summary>
    /// <param name="packaging">The packaging to map.</param>
    /// <returns>The mapped packaging, or <see langword="null"/> when the packaging is unknown.</returns>
    private static MusicReleasePackaging? MapPackaging(string? packaging)
    {
        if (string.IsNullOrWhiteSpace(packaging))
            return null;

        string value = packaging.Trim().ToLowerInvariant().Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal).Replace("/", string.Empty, StringComparison.Ordinal);
        return Enum.TryParse(value, ignoreCase: true, out MusicReleasePackaging parsed) ? parsed : null;
    }

    /// <summary>
    /// Maps a free-form musical key into the application enum.
    /// </summary>
    /// <param name="musicKey">The musical key to map.</param>
    /// <returns>The mapped musical key, or <see langword="null"/> when the key is unknown.</returns>
    private static MusicKey? MapKey(string? musicKey)
    {
        if (string.IsNullOrWhiteSpace(musicKey))
            return null;

        string value = musicKey.Trim();
        bool isMinor = false;
        if (value.EndsWith("minor", StringComparison.OrdinalIgnoreCase))
        {
            isMinor = true;
            value = value[..^5];
        }
        else if (value.EndsWith("major", StringComparison.OrdinalIgnoreCase))
            value = value[..^5];
        else if (value.EndsWith("min", StringComparison.OrdinalIgnoreCase))
        {
            isMinor = true;
            value = value[..^3];
        }
        else if (value.EndsWith("maj", StringComparison.OrdinalIgnoreCase))
            value = value[..^3];
        else if (value.EndsWith("m", StringComparison.OrdinalIgnoreCase))
        {
            isMinor = true;
            value = value[..^1];
        }

        value = value.Trim().ToUpperInvariant().Replace("\u266f", "#", StringComparison.Ordinal).Replace("\u266d", "B", StringComparison.Ordinal);
        return value switch
        {
            "C" => isMinor ? MusicKey.CMinor : MusicKey.CMajor,
            "C#" or "DB" => isMinor ? MusicKey.CSharpMinor : MusicKey.CSharpMajor,
            "D" => isMinor ? MusicKey.DMinor : MusicKey.DMajor,
            "D#" or "EB" => isMinor ? MusicKey.DSharpMinor : MusicKey.DSharpMajor,
            "E" => isMinor ? MusicKey.EMinor : MusicKey.EMajor,
            "F" => isMinor ? MusicKey.FMinor : MusicKey.FMajor,
            "F#" or "GB" => isMinor ? MusicKey.FSharpMinor : MusicKey.FSharpMajor,
            "G" => isMinor ? MusicKey.GMinor : MusicKey.GMajor,
            "G#" or "AB" => isMinor ? MusicKey.GSharpMinor : MusicKey.GSharpMajor,
            "A" => isMinor ? MusicKey.AMinor : MusicKey.AMajor,
            "A#" or "BB" => isMinor ? MusicKey.ASharpMinor : MusicKey.ASharpMajor,
            "B" => isMinor ? MusicKey.BMinor : MusicKey.BMajor,
            _ => null
        };
    }

    /// <summary>
    /// Maps a free-form ISO 639 language code into language information.
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

        switch (primaryCode)
        {
            case "zxx":
                return new LanguageInfoDto(primaryCode, "No linguistic content", null);
            case "mul":
                return new LanguageInfoDto(primaryCode, "Multiple languages", null);
            case "und":
                return new LanguageInfoDto(primaryCode, "Undetermined", null);
        }

        if (s_culturesByLanguageCode.TryGetValue(primaryCode, out CultureInfo? culture))
            return new LanguageInfoDto(culture.TwoLetterISOLanguageName, culture.EnglishName, culture.NativeName);
        return new LanguageInfoDto(primaryCode, primaryCode, null);
    }

    /// <summary>
    /// Maps a free-form contributor role into a canonical media contributor role.
    /// </summary>
    /// <param name="role">The role to map.</param>
    /// <returns>The mapped contributor role.</returns>
    private static MediaContributorRole MapRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return MediaContributorRole.Other;

        string value = role.Trim().ToLowerInvariant();

        // An instrument is more specific than a generic role, so the instruments are matched first.
        if (value.Contains("backing vocal", StringComparison.Ordinal))
            return MediaContributorRole.BackingVocals;
        if (value.Contains("vocal", StringComparison.Ordinal))
            return MediaContributorRole.Vocals;
        if (value.Contains("bass guitar", StringComparison.Ordinal) || value.Contains("electric bass", StringComparison.Ordinal) || string.Equals(value, "bass", StringComparison.Ordinal))
            return MediaContributorRole.BassGuitar;
        if (value.Contains("guitar", StringComparison.Ordinal))
            return MediaContributorRole.Guitar;
        if (value.Contains("drum", StringComparison.Ordinal) || value.Contains("membranophone", StringComparison.Ordinal))
            return MediaContributorRole.Drums;
        if (value.Contains("percussion", StringComparison.Ordinal) || value.Contains("tambourine", StringComparison.Ordinal) || value.Contains("handclap", StringComparison.Ordinal) || value.Contains("stomp", StringComparison.Ordinal))
            return MediaContributorRole.Percussion;
        if (value.Contains("keyboard", StringComparison.Ordinal))
            return MediaContributorRole.Keyboards;
        if (value.Contains("piano", StringComparison.Ordinal))
            return MediaContributorRole.Piano;
        if (value.Contains("synthesizer", StringComparison.Ordinal) || value.Contains("synth", StringComparison.Ordinal) || value.Contains("sampler", StringComparison.Ordinal))
            return MediaContributorRole.Synthesizer;
        if (value.Contains("string", StringComparison.Ordinal) || value.Contains("violin", StringComparison.Ordinal) || value.Contains("cello", StringComparison.Ordinal) || value.Contains("viola", StringComparison.Ordinal))
            return MediaContributorRole.Strings;
        if (value.Contains("brass", StringComparison.Ordinal) || value.Contains("trumpet", StringComparison.Ordinal) || value.Contains("trombone", StringComparison.Ordinal) || value.Contains("horn", StringComparison.Ordinal))
            return MediaContributorRole.Brass;
        if (value.Contains("woodwind", StringComparison.Ordinal) || value.Contains("flute", StringComparison.Ordinal) || value.Contains("saxophone", StringComparison.Ordinal) || value.Contains("clarinet", StringComparison.Ordinal))
            return MediaContributorRole.Woodwinds;

        if (value.Contains("executive producer", StringComparison.Ordinal))
            return MediaContributorRole.ExecutiveProducer;
        if (value.Contains("producer", StringComparison.Ordinal))
            return MediaContributorRole.Producer;
        if (value.Contains("remix", StringComparison.Ordinal))
            return MediaContributorRole.Remixer;
        if (value.Contains("engineer", StringComparison.Ordinal) || value.Contains("mastering", StringComparison.Ordinal))
            return MediaContributorRole.Engineer;
        if (value.Contains("mix", StringComparison.Ordinal))
            return MediaContributorRole.Mixer;
        if (value.Contains("arranger", StringComparison.Ordinal))
            return MediaContributorRole.Arranger;
        if (value.Contains("conductor", StringComparison.Ordinal))
            return MediaContributorRole.Conductor;
        if (value.Contains("orchestrator", StringComparison.Ordinal))
            return MediaContributorRole.Orchestrator;
        if (value.Contains("composer", StringComparison.Ordinal))
            return MediaContributorRole.Composer;
        if (value.Contains("lyricist", StringComparison.Ordinal) || value.Contains("librettist", StringComparison.Ordinal))
            return MediaContributorRole.Lyricist;
        if (value.Contains("writer", StringComparison.Ordinal) || value.Contains("author", StringComparison.Ordinal))
            return MediaContributorRole.Author;
        if (value.Contains("choir", StringComparison.Ordinal) || value.Contains("chorus", StringComparison.Ordinal))
            return MediaContributorRole.Choir;
        if (value.Contains("performer", StringComparison.Ordinal) || value.Contains("member of band", StringComparison.Ordinal) || value.Contains("orchestra", StringComparison.Ordinal))
            return MediaContributorRole.Performer;

        return MediaContributorRole.Other;
    }

    /// <summary>
    /// Adds a contributor to the provided list, unless an identical name and role pair is already present.
    /// </summary>
    /// <param name="contributors">The contributors collected so far.</param>
    /// <param name="displayName">The display name of the contributor.</param>
    /// <param name="role">The role the contributor played.</param>
    private static void AddContributor(List<MediaContributorDto> contributors, string displayName, MediaContributorRole role)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return;
        if (contributors.Any(contributor =>
                string.Equals(contributor.Name?.DisplayName, displayName, StringComparison.OrdinalIgnoreCase) &&
                contributor.Role == role))
            return;
        contributors.Add(new MediaContributorDto(new MediaContributorNameDto(displayName.Trim(), null), role));
    }

    /// <summary>
    /// Parses the provided value as a globally unique identifier.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <returns>The parsed identifier, or <see langword="null"/> when the value is missing or malformed.</returns>
    private static Guid? ParseGuid(string? value)
    {
        return Guid.TryParse(value, out Guid parsed) ? parsed : null;
    }

    /// <summary>
    /// Parses the first globally unique identifier of a value that may hold several slash-separated identifiers.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <returns>The first parsed identifier, or <see langword="null"/> when the value holds none.</returns>
    private static Guid? ParseFirstGuid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        foreach (string candidate in value.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (Guid.TryParse(candidate, out Guid parsed))
                return parsed;
        return null;
    }

    /// <summary>
    /// Gets the first value of a tag that may hold several slash-separated values.
    /// </summary>
    /// <param name="value">The value to read.</param>
    /// <returns>The first non-empty value, or <see langword="null"/> when the value is empty.</returns>
    private static string? FirstValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return NullIfWhiteSpace(value.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault());
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
