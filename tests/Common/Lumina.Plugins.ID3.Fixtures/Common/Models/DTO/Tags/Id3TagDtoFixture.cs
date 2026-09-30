#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.ID3.Common.Models.DTO.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.ID3.Fixtures.Common.Models.DTO.Tags;

/// <summary>
/// Fixture class for generating <see cref="Id3TagDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class Id3TagDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="Id3TagDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="album">Optional. The title of the album the track belongs to.</param>
    /// <param name="includeAlbum">Whether the album should be included, or forced to <see langword="null"/>.</param>
    /// <param name="trackArtists">Optional. The artists credited on the track.</param>
    /// <param name="albumArtists">Optional. The artists credited on the album.</param>
    /// <param name="albumArtistSortName">Optional. The sort name of the album artist.</param>
    /// <param name="includeAlbumArtistSortName">Whether the album artist sort name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="trackArtistSortName">Optional. The sort name of the track artist.</param>
    /// <param name="includeTrackArtistSortName">Whether the track artist sort name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc, or a random one when not supplied.</param>
    /// <param name="trackCount">Optional. The number of tracks on the disc, or a random one when not supplied.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to, or a random one when not supplied.</param>
    /// <param name="discCount">Optional. The number of discs of the release, or a random one when not supplied.</param>
    /// <param name="year">Optional. The year the track was released.</param>
    /// <param name="includeYear">Whether the year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="originalReleaseDate">Optional. The original release date of the track.</param>
    /// <param name="includeOriginalReleaseDate">Whether the original release date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="originalReleaseYear">Optional. The original release year of the track.</param>
    /// <param name="includeOriginalReleaseYear">Whether the original release year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="genres">Optional. The genres the tags associate with the track.</param>
    /// <param name="composers">Optional. The composers credited on the track.</param>
    /// <param name="lyricists">Optional. The lyricists credited on the track.</param>
    /// <param name="conductors">Optional. The conductors credited on the track.</param>
    /// <param name="remixers">Optional. The remixers credited on the track.</param>
    /// <param name="involvedPeople">Optional. The people credited on the track, each with their role.</param>
    /// <param name="language">Optional. The language of the track.</param>
    /// <param name="includeLanguage">Whether the language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="script">Optional. The script used by the language of the track.</param>
    /// <param name="includeScript">Whether the script should be included, or forced to <see langword="null"/>.</param>
    /// <param name="label">Optional. The label that issued the album.</param>
    /// <param name="includeLabel">Whether the label should be included, or forced to <see langword="null"/>.</param>
    /// <param name="catalogNumbers">Optional. The catalog numbers of the album.</param>
    /// <param name="barcode">Optional. The barcode of the album.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <param name="releaseTypes">Optional. The types of the release.</param>
    /// <param name="releaseStatus">Optional. The status of the release.</param>
    /// <param name="includeReleaseStatus">Whether the release status should be included, or forced to <see langword="null"/>.</param>
    /// <param name="releaseCountry">Optional. The country the release was issued in.</param>
    /// <param name="includeReleaseCountry">Whether the release country should be included, or forced to <see langword="null"/>.</param>
    /// <param name="mediaFormat">Optional. The medium of the release.</param>
    /// <param name="includeMediaFormat">Whether the media format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="packaging">Optional. The packaging of the release.</param>
    /// <param name="includePackaging">Whether the packaging should be included, or forced to <see langword="null"/>.</param>
    /// <param name="asin">Optional. The ASIN of the album.</param>
    /// <param name="includeAsin">Whether the ASIN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="isrcs">Optional. The ISRCs of the track.</param>
    /// <param name="moods">Optional. The moods of the track.</param>
    /// <param name="workTitle">Optional. The title of the work the track is a recording of.</param>
    /// <param name="includeWorkTitle">Whether the work title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="musicKey">Optional. The musical key of the track.</param>
    /// <param name="includeMusicKey">Whether the musical key should be included, or forced to <see langword="null"/>.</param>
    /// <param name="bpm">Optional. The tempo of the track in beats per minute.</param>
    /// <param name="includeBpm">Whether the tempo should be included, or forced to <see langword="null"/>.</param>
    /// <param name="durationInSeconds">Optional. The duration of the audio of the track in seconds.</param>
    /// <param name="includeDurationInSeconds">Whether the duration should be included, or forced to <see langword="null"/>.</param>
    /// <param name="sampleRate">Optional. The sample rate of the audio of the track in Hz.</param>
    /// <param name="includeSampleRate">Whether the sample rate should be included, or forced to <see langword="null"/>.</param>
    /// <param name="channels">Optional. The number of audio channels of the track.</param>
    /// <param name="includeChannels">Whether the channel count should be included, or forced to <see langword="null"/>.</param>
    /// <param name="bitDepth">Optional. The bit depth of the audio of the track.</param>
    /// <param name="includeBitDepth">Whether the bit depth should be included, or forced to <see langword="null"/>.</param>
    /// <param name="bitrate">Optional. The bitrate of the audio of the track in kbps.</param>
    /// <param name="includeBitrate">Whether the bitrate should be included, or forced to <see langword="null"/>.</param>
    /// <param name="audioCodec">Optional. The description of the audio codec of the track.</param>
    /// <param name="includeAudioCodec">Whether the audio codec should be included, or forced to <see langword="null"/>.</param>
    /// <param name="website">Optional. The official website associated with the artist.</param>
    /// <param name="includeWebsite">Whether the website should be included, or forced to <see langword="null"/>.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="includeMusicBrainzArtistId">Whether the MusicBrainz artist identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="musicBrainzReleaseArtistId">Optional. The MusicBrainz identifier of the release artist.</param>
    /// <param name="includeMusicBrainzReleaseArtistId">Whether the MusicBrainz release artist identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="musicBrainzReleaseGroupId">Optional. The MusicBrainz identifier of the release group.</param>
    /// <param name="includeMusicBrainzReleaseGroupId">Whether the MusicBrainz release group identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz identifier of the release.</param>
    /// <param name="includeMusicBrainzReleaseId">Whether the MusicBrainz release identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="musicBrainzRecordingId">Optional. The MusicBrainz identifier of the recording of the track.</param>
    /// <param name="includeMusicBrainzRecordingId">Whether the MusicBrainz recording identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="musicBrainzReleaseTrackId">Optional. The MusicBrainz identifier of the track on its release.</param>
    /// <param name="includeMusicBrainzReleaseTrackId">Whether the MusicBrainz release track identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="musicBrainzWorkId">Optional. The MusicBrainz identifier of the work the track is a recording of.</param>
    /// <param name="includeMusicBrainzWorkId">Whether the MusicBrainz work identifier should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="Id3TagDto"/> instance.</returns>
    public Id3TagDto Create(
        string? title = null,
        bool includeTitle = true,
        string? album = null,
        bool includeAlbum = true,
        List<string>? trackArtists = null,
        List<string>? albumArtists = null,
        string? albumArtistSortName = null,
        bool includeAlbumArtistSortName = true,
        string? trackArtistSortName = null,
        bool includeTrackArtistSortName = true,
        uint? trackNumber = null,
        uint? trackCount = null,
        uint? discNumber = null,
        uint? discCount = null,
        int? year = null,
        bool includeYear = true,
        DateOnly? originalReleaseDate = null,
        bool includeOriginalReleaseDate = true,
        int? originalReleaseYear = null,
        bool includeOriginalReleaseYear = true,
        List<string>? genres = null,
        List<string>? composers = null,
        List<string>? lyricists = null,
        List<string>? conductors = null,
        List<string>? remixers = null,
        List<(string Role, string Name)>? involvedPeople = null,
        string? language = null,
        bool includeLanguage = true,
        string? script = null,
        bool includeScript = true,
        string? label = null,
        bool includeLabel = true,
        List<string>? catalogNumbers = null,
        string? barcode = null,
        bool includeBarcode = true,
        List<string>? releaseTypes = null,
        string? releaseStatus = null,
        bool includeReleaseStatus = true,
        string? releaseCountry = null,
        bool includeReleaseCountry = true,
        string? mediaFormat = null,
        bool includeMediaFormat = true,
        string? packaging = null,
        bool includePackaging = true,
        string? asin = null,
        bool includeAsin = true,
        List<string>? isrcs = null,
        List<string>? moods = null,
        string? workTitle = null,
        bool includeWorkTitle = true,
        string? musicKey = null,
        bool includeMusicKey = true,
        int? bpm = null,
        bool includeBpm = true,
        int? durationInSeconds = null,
        bool includeDurationInSeconds = true,
        int? sampleRate = null,
        bool includeSampleRate = true,
        int? channels = null,
        bool includeChannels = true,
        int? bitDepth = null,
        bool includeBitDepth = true,
        int? bitrate = null,
        bool includeBitrate = true,
        string? audioCodec = null,
        bool includeAudioCodec = true,
        string? website = null,
        bool includeWebsite = true,
        string? musicBrainzArtistId = null,
        bool includeMusicBrainzArtistId = true,
        string? musicBrainzReleaseArtistId = null,
        bool includeMusicBrainzReleaseArtistId = true,
        string? musicBrainzReleaseGroupId = null,
        bool includeMusicBrainzReleaseGroupId = true,
        string? musicBrainzReleaseId = null,
        bool includeMusicBrainzReleaseId = true,
        string? musicBrainzRecordingId = null,
        bool includeMusicBrainzRecordingId = true,
        string? musicBrainzReleaseTrackId = null,
        bool includeMusicBrainzReleaseTrackId = true,
        string? musicBrainzWorkId = null,
        bool includeMusicBrainzWorkId = true)
    {
        return new Id3TagDto
        {
            Title = includeTitle ? title ?? _faker.Lorem.Sentence(3) : null,
            Album = includeAlbum ? album ?? _faker.Lorem.Sentence(2) : null,
            TrackArtists = trackArtists ?? [.. _faker.Make(2, () => _faker.Name.FullName())],
            AlbumArtists = albumArtists ?? [.. _faker.Make(2, () => _faker.Name.FullName())],
            AlbumArtistSortName = includeAlbumArtistSortName ? albumArtistSortName ?? _faker.Name.LastName() : null,
            TrackArtistSortName = includeTrackArtistSortName ? trackArtistSortName ?? _faker.Name.LastName() : null,
            TrackNumber = trackNumber ?? (uint)_faker.Random.Int(1, 30),
            TrackCount = trackCount ?? (uint)_faker.Random.Int(1, 30),
            DiscNumber = discNumber ?? (uint)_faker.Random.Int(1, 4),
            DiscCount = discCount ?? (uint)_faker.Random.Int(1, 4),
            Year = includeYear ? year ?? _faker.Random.Int(1950, 2024) : null,
            OriginalReleaseDate = includeOriginalReleaseDate ? originalReleaseDate ?? DateOnly.FromDateTime(_faker.Date.Past()) : null,
            OriginalReleaseYear = includeOriginalReleaseYear ? originalReleaseYear ?? _faker.Random.Int(1950, 2024) : null,
            Genres = genres ?? [.. _faker.Make(2, () => _faker.Lorem.Word())],
            Composers = composers ?? [.. _faker.Make(2, () => _faker.Name.FullName())],
            Lyricists = lyricists ?? [.. _faker.Make(2, () => _faker.Name.FullName())],
            Conductors = conductors ?? [.. _faker.Make(2, () => _faker.Name.FullName())],
            Remixers = remixers ?? [.. _faker.Make(2, () => _faker.Name.FullName())],
            InvolvedPeople = involvedPeople ?? [(_faker.PickRandom("producer", "engineer", "mixer"), _faker.Name.FullName())],
            Language = includeLanguage ? language ?? _faker.Random.String2(3, "abcdefghijklmnopqrstuvwxyz") : null,
            Script = includeScript ? script ?? _faker.Random.String2(4, "abcdefghijklmnopqrstuvwxyz") : null,
            Label = includeLabel ? label ?? _faker.Company.CompanyName() : null,
            CatalogNumbers = catalogNumbers ?? [.. _faker.Make(2, () => _faker.Random.AlphaNumeric(8))],
            Barcode = includeBarcode ? barcode ?? _faker.Random.AlphaNumeric(12) : null,
            ReleaseTypes = releaseTypes ?? [_faker.PickRandom("album", "single", "ep")],
            ReleaseStatus = includeReleaseStatus ? releaseStatus ?? _faker.PickRandom("official", "promotion") : null,
            ReleaseCountry = includeReleaseCountry ? releaseCountry ?? _faker.PickRandom("US", "GB", "DE") : null,
            MediaFormat = includeMediaFormat ? mediaFormat ?? _faker.PickRandom("CD", "Vinyl") : null,
            Packaging = includePackaging ? packaging ?? _faker.PickRandom("Jewel Case", "Digipak") : null,
            Asin = includeAsin ? asin ?? _faker.Random.AlphaNumeric(10) : null,
            Isrcs = isrcs ?? [.. _faker.Make(2, () => $"US{_faker.Random.AlphaNumeric(10).ToUpperInvariant()}")],
            Moods = moods ?? [.. _faker.Make(2, () => _faker.Lorem.Word())],
            WorkTitle = includeWorkTitle ? workTitle ?? _faker.Lorem.Sentence(3) : null,
            MusicKey = includeMusicKey ? musicKey ?? _faker.PickRandom("C major", "A minor") : null,
            Bpm = includeBpm ? bpm ?? _faker.Random.Int(60, 200) : null,
            DurationInSeconds = includeDurationInSeconds ? durationInSeconds ?? _faker.Random.Int(30, 600) : null,
            SampleRate = includeSampleRate ? sampleRate ?? _faker.PickRandom(44100, 48000, 96000) : null,
            Channels = includeChannels ? channels ?? _faker.Random.Int(1, 2) : null,
            BitDepth = includeBitDepth ? bitDepth ?? _faker.PickRandom(16, 24, 32) : null,
            Bitrate = includeBitrate ? bitrate ?? _faker.Random.Int(128, 320) : null,
            AudioCodec = includeAudioCodec ? audioCodec ?? _faker.PickRandom("PCM Audio", "MPEG Audio") : null,
            Website = includeWebsite ? website ?? _faker.Internet.Url() : null,
            MusicBrainzArtistId = includeMusicBrainzArtistId ? musicBrainzArtistId ?? Guid.NewGuid().ToString() : null,
            MusicBrainzReleaseArtistId = includeMusicBrainzReleaseArtistId ? musicBrainzReleaseArtistId ?? Guid.NewGuid().ToString() : null,
            MusicBrainzReleaseGroupId = includeMusicBrainzReleaseGroupId ? musicBrainzReleaseGroupId ?? Guid.NewGuid().ToString() : null,
            MusicBrainzReleaseId = includeMusicBrainzReleaseId ? musicBrainzReleaseId ?? Guid.NewGuid().ToString() : null,
            MusicBrainzRecordingId = includeMusicBrainzRecordingId ? musicBrainzRecordingId ?? Guid.NewGuid().ToString() : null,
            MusicBrainzReleaseTrackId = includeMusicBrainzReleaseTrackId ? musicBrainzReleaseTrackId ?? Guid.NewGuid().ToString() : null,
            MusicBrainzWorkId = includeMusicBrainzWorkId ? musicBrainzWorkId ?? Guid.NewGuid().ToString() : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="Id3TagDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Id3TagDto"/> instances.</returns>
    public List<Id3TagDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
