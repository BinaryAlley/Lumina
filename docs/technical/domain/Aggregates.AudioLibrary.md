- [Lumina Domain](#lumina-domain)
  - [Music Library Aggregate](#music-library-aggregate)
    - [Artist](#artist)
    - [Album](#album)
    - [Track](#track)
    - [Audio Rating](#audio-rating)

# [Lumina Domain](#lumina-domain)

## [Music Library Aggregate](#music-library-aggregate)

The music library aggregate models the music of a media library. `Artist` is the aggregate root, owning its albums, and each album owns its tracks. Albums and tracks carry full metadata: title, description, release information, languages, genres and tags, and the tracks additionally carry the audio characteristics of their file, their moods and their ISRC codes. The media contributors that make up an artist, or that performed on an album or a track, are carried as `MusicMediaContributor` value objects.

### [Artist](#artist)

```csharp
class Artist
{
    Result<Artist> Create(
        LibraryId libraryId,
        string name,
        Optional<string> sortName,
        Optional<string> disambiguation,
        Optional<MusicArtistType> type,
        Optional<MusicArtistGender> gender,
        Optional<string> country,
        Optional<MusicArea> area,
        Optional<MusicArea> beginArea,
        Optional<MusicArea> endArea,
        Optional<DateOnly> lifeSpanBegin,
        Optional<DateOnly> lifeSpanEnd,
        bool isEnded,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        List<string> ipis,
        List<string> isnis,
        List<MusicArtistAlias> aliases,
        List<Genre> genres,
        List<Tag> tags,
        List<AudioRating> ratings,
        List<MusicMediaContributor> contributors,
        List<Album> albums);
}
```

```json
{
  "id": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "libraryId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "name": "Queen",
  "sortName": "Queen",
  "disambiguation": null,
  "type": "Group",
  "gender": null,
  "country": "GB",
  "area": {
    "musicBrainzAreaId": {
      "value": "00000000-0000-0000-0000-000000000000"
    },
    "name": "United Kingdom",
    "sortName": null,
    "disambiguation": null,
    "type": "Country",
    "iso3166Code": "GB"
  },
  "beginArea": null,
  "endArea": null,
  "lifeSpanBegin": "1970-06-27",
  "lifeSpanEnd": null,
  "isEnded": false,
  "website": "https://www.queenonline.com",
  "musicBrainzArtistId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "ipis": [],
  "isnis": [],
  "aliases": [
    {
      "name": "Queen",
      "sortName": "Queen",
      "type": "Artist name",
      "locale": "en",
      "isPrimary": true,
      "beginDate": "1970-06-27",
      "endDate": null,
      "isEnded": false
    }
  ],
  "genres": [
    { "name": "Rock" }
  ],
  "tags": [
    { "name": "classic" }
  ],
  "ratings": [
    {
      "value": 4.5,
      "maxValue": 5,
      "source": "MusicBrainz",
      "voteCount": 2345
    }
  ],
  "contributors": [],
  "albums": [
    {
      "value": "00000000-0000-0000-0000-000000000000"
    }
  ]
}
```

### [Album](#album)

```csharp
class Album
{
    Result<Album> Create(
        AlbumMetadata metadata,
        Optional<string> disambiguation,
        Optional<MusicMediaFormat> mediaFormat,
        Optional<MusicReleasePackaging> packaging,
        Optional<string> script,
        Optional<Barcode> barcode,
        List<string> catalogNumbers,
        Optional<string> label,
        Optional<string> asin,
        Optional<MusicBrainzId> musicBrainzReleaseId,
        Optional<MusicBrainzId> musicBrainzReleaseGroupId,
        Optional<MusicBrainzId> musicBrainzReleaseArtistId,
        List<MusicMediaContributor> contributors,
        List<AudioRating> ratings,
        List<Track> tracks);
}
```

```json
{
  "id": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "metadata": {
    "title": "A Night at the Opera",
    "originalTitle": "A Night at the Opera",
    "releaseTitle": "A Night at the Opera",
    "description": "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
    "releaseInfo": {
      "originalReleaseDate": "1975-11-21",
      "originalReleaseYear": 1975,
      "reReleaseDate": null,
      "reReleaseYear": null,
      "releaseCountry": "GB",
      "releaseVersion": "Remastered"
    },
    "genres": [
      { "name": "Rock" },
      { "name": "Progressive Rock" }
    ],
    "tags": [
      { "name": "classic" },
      { "name": "vinyl" }
    ],
    "language": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "originalLanguage": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "releaseTypes": [ "Album" ],
    "releaseStatus": "Official",
    "totalDiscs": 1,
    "totalTracks": 12
  },
  "disambiguation": null,
  "mediaFormat": "CD",
  "packaging": "JewelCase",
  "script": "Latn",
  "barcode": {
    "value": "0042282778329"
  },
  "catalogNumbers": ["EMC 4008"],
  "label": "EMI",
  "asin": "B000000000",
  "musicBrainzReleaseId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "musicBrainzReleaseGroupId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "musicBrainzReleaseArtistId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "contributors": [],
  "ratings": [
    {
      "value": 4.5,
      "maxValue": 5,
      "source": "MusicBrainz",
      "voteCount": 2345
    },
    {
      "value": 4.8,
      "maxValue": 5,
      "source": "LastFm",
      "voteCount": 1234
    }
  ],
  "tracks": [
    {
      "value": "00000000-0000-0000-0000-000000000000"
    }
  ]
}
```

### [Track](#track)

```csharp
class Track
{
    Result<Track> Create(
        string path,
        AudioMetadata metadata,
        Optional<string> disambiguation,
        int trackNumber,
        Optional<int> discNumber,
        List<Mood> moods,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        bool isVideo,
        List<Isrc> isrcs,
        Optional<MusicWork> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId,
        List<MusicMediaContributor> contributors,
        List<AudioRating> ratings);
}
```

```json
{
  "id": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "path": "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
  "metadata": {
    "title": "Bohemian Rhapsody",
    "originalTitle": "Bohemian Rhapsody",
    "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
    "releaseInfo": {
      "originalReleaseDate": "1975-10-31",
      "originalReleaseYear": 1975,
      "reReleaseDate": null,
      "reReleaseYear": null,
      "releaseCountry": "GB",
      "releaseVersion": "Original"
    },
    "genres": [
      { "name": "Rock" },
      { "name": "Progressive Rock" }
    ],
    "tags": [
      { "name": "classic" },
      { "name": "epic" }
    ],
    "language": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "originalLanguage": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980,
    "acoustId": "f0e1d2c3-b4a5-4c6d-8e7f-9a0b1c2d3e4f",
    "replayGainTrackGain": -7.5,
    "replayGainTrackPeak": 0.9877,
    "replayGainAlbumGain": -6.8,
    "replayGainAlbumPeak": 0.9999
  },
  "disambiguation": null,
  "trackNumber": 1,
  "discNumber": 1,
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "isVideo": false,
  "work": {
    "musicBrainzWorkId": {
      "value": "00000000-0000-0000-0000-000000000000"
    },
    "title": "Bohemian Rhapsody",
    "type": "Song",
    "languages": [
      {
        "languageCode": "en",
        "languageName": "English",
        "nativeName": "English"
      }
    ],
    "iswcs": []
  },
  "musicBrainzRecordingId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "musicBrainzTrackId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "moods": [
    { "name": "dramatic" },
    { "name": "anxious" }
  ],
  "isrcs": [
    {
      "value": "GBUM71029604"
    }
  ],
  "contributors": [],
  "ratings": [
    {
      "value": 4.5,
      "maxValue": 5,
      "source": "MusicBrainz",
      "voteCount": 2345
    }
  ]
}
```

### [Audio Rating](#audio-rating)

The rating of a music media element derives from the common `Rating` value object and adds the source the rating came from.

```csharp
class AudioRating : Rating
{
    Result<AudioRating> Create(decimal value, decimal maxValue, Optional<AudioRatingSource> source, Optional<int> voteCount);
}
```

```json
{
    "value": 9.3,
    "maxValue": 10.0,
    "source": "MusicBrainz",
    "voteCount": 84
}
```

`AudioRatingSource` is `User`, `MusicBrainz`, `Discogs` or `LastFm`.
