- [Lumina Domain](#lumina-domain)
  - [Video Library Aggregate](#video-library-aggregate)
    - [Movie](#movie)

# [Lumina Domain](#lumina-domain)

## [Video Library Aggregate](#video-library-aggregate)

### [Movie](#movie)

```csharp
class Movie
{
    ErrorOr<Movie> Create(MovieId id, VideoMetadata metadata, List<MediaContributorId> contributors);
}
```

```json
{
    "id": { "value": "00000000-0000-0000-0000-000000000000" },
    "metadata": {
        "title": "The Lord of the Rings - The Fellowship of the Ring",
        "originalTitle": "The Lord of the Rings - The Fellowship of the Ring",
        "description": "The greatest fantasy movie ever made",
        "releaseInfo": {
            "originalReleaseDate": "2001-12-19",
            "originalReleaseYear": 2001,
            "reReleaseDate": "2024-06-08",
            "reReleaseYear": 2024,
            "releaseCountry": "us",
            "releaseVersion": "Director's Cut"
        },
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
        "tags": [
            { "name": "epic quest" },
            { "name": "friendship" },
            { "name": "middle-earth" }
        ],
        "genres": [
            { "name": "fantasy" },
            { "name": "adventure" },
            { "name": "action" }
        ],
        "durationInSeconds": 10680,
        "resolution": "3840x2160",
        "frameRate": 29.97,
        "videoCodec": "h.265/hevc",
        "audioCodec": "FLAC"
    },
    "contributors": [
        { "value": "00000000-0000-0000-0000-000000000000" }
    ],
    "ratings": [
        {
            "value": 9.8,
            "maxValue": 10.0,
            "source": "TheMovieDb",
            "voteCount": 1547036
        }
    ]
}
```