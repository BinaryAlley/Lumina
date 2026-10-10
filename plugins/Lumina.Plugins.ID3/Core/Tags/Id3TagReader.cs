#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.ID3.Common.Models.DTO.Tags;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
#endregion

namespace Lumina.Plugins.ID3.Core.Tags;

/// <summary>
/// Reads the embedded tags of an audio file into a snapshot, caching the snapshot per path so that the tags of a single file are read only once.
/// </summary>
internal sealed class Id3TagReader
{
    /// <summary>
    /// The container tag types whose extended tags are searched, in the order they are tried.
    /// </summary>
    private static readonly TagLib.TagTypes[] s_extendedTagContainerTypes = [TagLib.TagTypes.Xiph, TagLib.TagTypes.Id3v2, TagLib.TagTypes.Apple, TagLib.TagTypes.Ape];

    /// <summary>
    /// The keys of the free-form Apple tags that carry the involved people of a track, paired with the role each of them describes.
    /// </summary>
    private static readonly (string Role, string Key)[] s_appleInvolvedPeopleKeys =
    [
        ("performer", "performer"),
        ("producer", "PRODUCER"),
        ("engineer", "ENGINEER"),
        ("mixer", "MIXER"),
        ("arranger", "arranger"),
        ("writer", "writer")
    ];

    /// <summary>
    /// Matches a four digit year.
    /// </summary>
    private static readonly Regex s_yearPattern = new(@"\b(?:1[0-9]{3}|20[0-9]{2}|2100)\b", RegexOptions.Compiled);

    /// <summary>
    /// The maximum number of tag snapshots the cache retains, so that reading a large library does not retain the tags of every file it visits.
    /// </summary>
    private const int MAX_CACHED_SNAPSHOTS = 512;

    private readonly Dictionary<string, Id3TagDto> _cache = new(StringComparer.Ordinal);
    private readonly Queue<string> _cacheOrder = new();
    private readonly Lock _cacheLock = new();

    /// <summary>
    /// Reads the embedded tags of the audio file stored at <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The file system path of the audio file whose tags are read.</param>
    /// <returns>The snapshot of the tags, or <see langword="null"/> when the file does not exist or its tags cannot be read.</returns>
    public Id3TagDto? Read(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        lock (_cacheLock)
        {
            if (_cache.TryGetValue(path, out Id3TagDto? cached))
                return cached;
        }

        Id3TagDto? data = ReadTagData(path);
        // A file whose tags cannot be read is not cached, so that a transient failure does not skip the file for the rest of the scan.
        if (data is null)
            return null;

        lock (_cacheLock)
        {
            if (!_cache.ContainsKey(path))
            {
                if (_cache.Count >= MAX_CACHED_SNAPSHOTS)
                    EvictOldestSnapshot();
                _cache[path] = data;
                _cacheOrder.Enqueue(path);
            }
        }
        return data;
    }

    /// <summary>
    /// Removes the snapshot that was cached first, keeping the cache within its maximum size.
    /// </summary>
    private void EvictOldestSnapshot()
    {
        if (_cacheOrder.Count == 0)
            return;
        string oldestPath = _cacheOrder.Dequeue();
        _cache.Remove(oldestPath);
    }

    /// <summary>
    /// Reads the embedded tags of the audio file stored at <paramref name="path"/> into a snapshot.
    /// </summary>
    /// <param name="path">The file system path of the audio file whose tags are read.</param>
    /// <returns>The snapshot of the tags, or <see langword="null"/> when the file does not exist or its tags cannot be read.</returns>
    private static Id3TagDto? ReadTagData(string path)
    {
        // A file whose path cannot be represented by the operating system, or that is not an audio container, yields no metadata rather than an error.
        if (!File.Exists(path))
            return null;

        try
        {
            // A link inside a library root must not cause the tags of a file outside the library to be read.
            if (File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint))
                return null;

            using (TagLib.File tagFile = TagLib.File.Create(path))
            {
                TagLib.Tag tag = tagFile.Tag;
                TagLib.Properties audioProperties = tagFile.Properties;
                List<Func<string, List<string>>> extendedFields = BuildExtendedFieldSources(tagFile);

                (DateOnly? releaseDate, int? releaseYear) = ParseDate(GetExtendedField(extendedFields, "TDRC", "DATE"));
                (DateOnly? originalDate, int? originalYear) = ParseDate(GetExtendedField(extendedFields, "TDOR", "originaldate", "ORIGINALDATE", "originalyear"));
                int? year = releaseYear ?? (tag.Year > 0 ? (int)tag.Year : null);
                // The original release date and year must describe the same point in time, so the explicit original date is preferred and the year of the chosen date is used whenever a date is known, rather than the year of the release described by the tags.
                DateOnly? originalReleaseDate = originalDate ?? releaseDate;
                int? originalReleaseYear = originalReleaseDate?.Year ?? originalYear ?? year;

                return new Id3TagDto
                {
                    Title = NullIfWhiteSpace(tag.Title),
                    Album = NullIfWhiteSpace(tag.Album),
                    TrackArtists = Filter(tag.Performers),
                    AlbumArtists = Filter(tag.AlbumArtists),
                    AlbumArtistSortName = NullIfWhiteSpace(tag.AlbumArtistsSort?.FirstOrDefault()),
                    TrackArtistSortName = NullIfWhiteSpace(tag.PerformersSort?.FirstOrDefault()),
                    TrackNumber = tag.Track,
                    TrackCount = tag.TrackCount,
                    DiscNumber = tag.Disc,
                    DiscCount = tag.DiscCount,
                    Year = year,
                    OriginalReleaseDate = originalReleaseDate,
                    OriginalReleaseYear = originalReleaseYear,
                    Genres = SplitValues(Filter(tag.Genres)),
                    Composers = SplitValues(Filter(tag.Composers)),
                    Lyricists = SplitValues(GetExtendedFields(extendedFields, "LYRICIST", "lyricist", "TEXT")),
                    Conductors = Filter([tag.Conductor]),
                    Remixers = Filter([tag.RemixedBy]),
                    InvolvedPeople = ReadInvolvedPeople(extendedFields),
                    Language = NullIfWhiteSpace(GetExtendedField(extendedFields, "LANGUAGE", "TLAN")),
                    Script = NullIfWhiteSpace(GetExtendedField(extendedFields, "SCRIPT")),
                    Label = NullIfWhiteSpace(tag.Publisher) ?? NullIfWhiteSpace(GetExtendedField(extendedFields, "LABEL")),
                    CatalogNumbers = SplitValuesOnSemicolons(GetExtendedFields(extendedFields, "CATALOGNUMBER")),
                    Barcode = NullIfWhiteSpace(GetExtendedField(extendedFields, "BARCODE", "barcode")),
                    ReleaseTypes = SplitValuesOnSemicolons([tag.MusicBrainzReleaseType]),
                    ReleaseStatus = NullIfWhiteSpace(tag.MusicBrainzReleaseStatus),
                    ReleaseCountry = NullIfWhiteSpace(tag.MusicBrainzReleaseCountry),
                    MediaFormat = NullIfWhiteSpace(GetExtendedField(extendedFields, "MEDIA", "TMED")),
                    Packaging = NullIfWhiteSpace(GetExtendedField(extendedFields, "PACKAGING")),
                    Asin = NullIfWhiteSpace(tag.AmazonId) ?? NullIfWhiteSpace(GetExtendedField(extendedFields, "ASIN")),
                    Isrcs = ReadIsrcs(extendedFields, tag),
                    Moods = SplitValues(GetExtendedFields(extendedFields, "mood", "MOOD", "MOODS")),
                    WorkTitle = NullIfWhiteSpace(GetExtendedField(extendedFields, "WORK", "MUSICBRAINZ_WORK")),
                    MusicKey = NullIfWhiteSpace(tag.InitialKey),
                    Bpm = tag.BeatsPerMinute > 0 ? (int)tag.BeatsPerMinute : null,
                    DurationInSeconds = audioProperties.Duration > TimeSpan.Zero ? (int)audioProperties.Duration.TotalSeconds : null,
                    SampleRate = audioProperties.AudioSampleRate > 0 ? audioProperties.AudioSampleRate : null,
                    Channels = audioProperties.AudioChannels > 0 ? audioProperties.AudioChannels : null,
                    BitDepth = audioProperties.BitsPerSample > 0 ? audioProperties.BitsPerSample : null,
                    Bitrate = audioProperties.AudioBitrate > 0 ? audioProperties.AudioBitrate : null,
                    AudioCodec = NullIfWhiteSpace(audioProperties.Description),
                    Website = NullIfWhiteSpace(GetExtendedField(extendedFields, "WOAR", "website")),
                    MusicBrainzArtistId = NullIfWhiteSpace(tag.MusicBrainzArtistId),
                    MusicBrainzReleaseArtistId = NullIfWhiteSpace(tag.MusicBrainzReleaseArtistId),
                    MusicBrainzReleaseGroupId = NullIfWhiteSpace(tag.MusicBrainzReleaseGroupId),
                    MusicBrainzReleaseId = NullIfWhiteSpace(tag.MusicBrainzReleaseId),
                    MusicBrainzRecordingId = NullIfWhiteSpace(tag.MusicBrainzTrackId),
                    MusicBrainzReleaseTrackId = NullIfWhiteSpace(GetExtendedField(extendedFields, "MusicBrainz Release Track Id", "MUSICBRAINZ_RELEASETRACKID")),
                    MusicBrainzWorkId = NullIfWhiteSpace(GetExtendedField(extendedFields, "MusicBrainz Work Id", "MUSICBRAINZ_WORKID"))
                };
            }
        }
        catch (Exception exception) when (exception is TagLib.CorruptFileException or TagLib.UnsupportedFormatException or IOException or UnauthorizedAccessException)
        {
            // A file whose tags cannot be read still contributes the metadata derived from its path, so the failure is not propagated.
            return null;
        }
    }

    /// <summary>
    /// Reads the people credited on the track, together with the free-form role each of them is credited for.
    /// </summary>
    /// <param name="extendedFields">The extended fields of the file, in the order their containers are searched.</param>
    /// <returns>The people credited on the track, as role and name pairs.</returns>
    private static List<(string Role, string Name)> ReadInvolvedPeople(List<Func<string, List<string>>> extendedFields)
    {
        List<(string Role, string Name)> people = [];

        // The ID3v2.3 involved people list (IPLS) and the ID3v2.4 equivalent (TIPL) store roles and names as a single flat sequence of alternating values.
        List<string> id3People = GetExtendedFields(extendedFields, "IPLS", "TIPL", "TMCL");
        for (int index = 0; index + 1 < id3People.Count; index += 2)
            people.Add((id3People[index], id3People[index + 1]));

        // The Apple containers store each role as its own free-form tag, carrying one or more names.
        foreach ((string role, string key) in s_appleInvolvedPeopleKeys)
            foreach (string name in GetExtendedFields(extendedFields, key))
                if (!string.IsNullOrWhiteSpace(name))
                    people.Add((role, name));

        return people;
    }

    /// <summary>
    /// Reads the ISRCs of the track, from both the dedicated tag and the extended tags, without duplicates.
    /// </summary>
    /// <param name="extendedFields">The extended fields of the file, in the order their containers are searched.</param>
    /// <param name="tag">The combined tag of the file.</param>
    /// <returns>The ISRCs of the track.</returns>
    private static List<string> ReadIsrcs(List<Func<string, List<string>>> extendedFields, TagLib.Tag tag)
    {
        List<string> values = SplitValues(GetExtendedFields(extendedFields, "ISRC", "TSRC"));
        if (!string.IsNullOrWhiteSpace(tag.ISRC))
            values.AddRange(SplitValues([tag.ISRC]));
        return [.. values.Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// Builds the ordered accessors that resolve an extended tag value from whichever container tag of the file carries it.
    /// </summary>
    /// <param name="tagFile">The file whose extended tags are read.</param>
    /// <returns>The accessors, in the order the containers are searched.</returns>
    private static List<Func<string, List<string>>> BuildExtendedFieldSources(TagLib.File tagFile)
    {
        List<Func<string, List<string>>> sources = [];
        foreach (TagLib.TagTypes tagType in s_extendedTagContainerTypes)
        {
            TagLib.Tag? tag = tagFile.GetTag(tagType);
            if (tag is TagLib.Ogg.XiphComment xiphComment)
                sources.Add(key => [.. (xiphComment.GetField(key) ?? [])]);
            else if (tag is TagLib.Id3v2.Tag id3v2Tag)
            {
                Dictionary<string, List<string>> id3Fields = BuildId3v2Fields(id3v2Tag);
                sources.Add(key => id3Fields.TryGetValue(key, out List<string>? values) ? values : []);
            }
            else if (tag is TagLib.Mpeg4.AppleTag appleTag)
                sources.Add(key => GetAppleFields(appleTag, key));
            else if (tag is TagLib.Ape.Tag apeTag)
                sources.Add(key => GetApeFields(apeTag, key));
        }
        return sources;
    }

    /// <summary>
    /// Builds the map of the ID3v2 frames of the file, keyed by the name each frame is looked up with, so that the frames are enumerated only once per file.
    /// </summary>
    /// <param name="id3v2Tag">The ID3v2 tag whose frames are indexed.</param>
    /// <returns>The values of every frame, keyed by the frame identifier or the description of a user defined frame.</returns>
    private static Dictionary<string, List<string>> BuildId3v2Fields(TagLib.Id3v2.Tag id3v2Tag)
    {
        Dictionary<string, List<string>> fields = new(StringComparer.OrdinalIgnoreCase);
        foreach (TagLib.Id3v2.Frame frame in id3v2Tag.GetFrames())
        {
            string key;
            List<string> values;
            if (frame is TagLib.Id3v2.UserTextInformationFrame userTextViewFrame)
            {
                key = userTextViewFrame.Description;
                values = [.. userTextViewFrame.Text];
            }
            else if (frame is TagLib.Id3v2.UrlLinkFrame urlFrame)
            {
                key = urlFrame.FrameId.ToString();
                values = [.. urlFrame.Text];
            }
            else if (frame is TagLib.Id3v2.TextInformationFrame textFrame)
            {
                key = textFrame.FrameId.ToString();
                values = [.. textFrame.Text];
            }
            else
                continue;

            // A user defined frame without a description carries no lookup key, so it is not indexed.
            if (string.IsNullOrEmpty(key))
                continue;

            if (fields.TryGetValue(key, out List<string>? existing))
                existing.AddRange(values);
            else
                fields[key] = values;
        }
        return fields;
    }

    /// <summary>
    /// Reads the values of a free-form Apple tag with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="appleTag">The Apple tag whose values are read.</param>
    /// <param name="key">The name of the free-form tag to read.</param>
    /// <returns>Every value of the tag.</returns>
    private static List<string> GetAppleFields(TagLib.Mpeg4.AppleTag appleTag, string key)
    {
        List<string> values = [];
        foreach (string appleValue in appleTag.GetDashBoxes("com.apple.iTunes", key) ?? [])
            values.AddRange(appleValue.Split('\0', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return values;
    }

    /// <summary>
    /// Reads the values of an APE tag with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="apeTag">The APE tag whose values are read.</param>
    /// <param name="key">The name of the tag to read.</param>
    /// <returns>Every value of the tag.</returns>
    private static List<string> GetApeFields(TagLib.Ape.Tag apeTag, string key)
    {
        if (apeTag.HasItem(key) && apeTag.GetItem(key) is TagLib.Ape.Item apeItem)
            return [.. apeItem.ToStringArray()];
        return [];
    }

    /// <summary>
    /// Reads the first non-empty value of an extended tag, from whichever container tag of the file carries it.
    /// </summary>
    /// <param name="extendedFields">The extended fields of the file, in the order their containers are searched.</param>
    /// <param name="keys">The candidate names of the extended tag to read, in the order they are tried.</param>
    /// <returns>The first non-empty value found, or <see langword="null"/> when the file carries none of the provided tags.</returns>
    private static string? GetExtendedField(List<Func<string, List<string>>> extendedFields, params string[] keys)
    {
        foreach (Func<string, List<string>> extendedField in extendedFields)
        {
            foreach (string key in keys)
            {
                List<string> values = extendedField(key);
                string? value = values.FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate));
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }
        return null;
    }

    /// <summary>
    /// Reads every value of an extended tag, from whichever container tag of the file carries it.
    /// </summary>
    /// <param name="extendedFields">The extended fields of the file, in the order their containers are searched.</param>
    /// <param name="keys">The candidate names of the extended tag to read, in the order they are tried.</param>
    /// <returns>Every non-empty value found, or an empty list when the file carries none of the provided tags.</returns>
    private static List<string> GetExtendedFields(List<Func<string, List<string>>> extendedFields, params string[] keys)
    {
        List<string> values = [];
        foreach (Func<string, List<string>> extendedField in extendedFields)
            foreach (string key in keys)
                // A container whose keys are matched case insensitively resolves several case variant aliases to the same entry, so the values already read are not added again.
                foreach (string value in extendedField(key))
                    if (!string.IsNullOrWhiteSpace(value) && !values.Contains(value, StringComparer.Ordinal))
                        values.Add(value);
        return values;
    }

    /// <summary>
    /// Keeps only the non-empty values of the provided collection, trimmed.
    /// </summary>
    /// <param name="values">The values to filter.</param>
    /// <returns>The non-empty, trimmed values.</returns>
    private static List<string> Filter(IEnumerable<string>? values)
    {
        return [.. (values ?? []).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim())];
    }

    /// <summary>
    /// Splits every provided value on the separators used to pack several values into a single tag, and keeps the non-empty results.
    /// </summary>
    /// <param name="values">The values to split.</param>
    /// <returns>The individual values found.</returns>
    private static List<string> SplitValues(IEnumerable<string> values)
    {
        return SplitValues(values, ['/', ';']);
    }

    /// <summary>
    /// Splits every provided value on semicolons only, for the tags whose own values may contain a slash.
    /// </summary>
    /// <param name="values">The values to split.</param>
    /// <returns>The individual values found.</returns>
    private static List<string> SplitValuesOnSemicolons(IEnumerable<string> values)
    {
        return SplitValues(values, [';']);
    }

    /// <summary>
    /// Splits every provided value on the provided separators, and keeps the non-empty results.
    /// </summary>
    /// <param name="values">The values to split.</param>
    /// <param name="separators">The separators to split the values on.</param>
    /// <returns>The individual values found.</returns>
    private static List<string> SplitValues(IEnumerable<string> values, char[] separators)
    {
        List<string> result = [];
        foreach (string value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;
            foreach (string part in value.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (!string.IsNullOrWhiteSpace(part))
                    result.Add(part);
        }
        return result;
    }

    /// <summary>
    /// Parses a date string into a date and a year.
    /// </summary>
    /// <param name="value">The date string to parse.</param>
    /// <returns>The parsed date and year, or <see langword="null"/> values when the string could not be parsed.</returns>
    private static (DateOnly? Date, int? Year) ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (null, null);

        if (DateOnly.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly exact))
            return (exact, exact.Year);

        Match yearMatch = s_yearPattern.Match(value);
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
