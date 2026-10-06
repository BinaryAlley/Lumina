- [Lumina Domain](#lumina-domain)
  - [Common](#common)
    - [Metadata](#metadata)
      - [Genre](#genre)
      - [Tag](#tag)
      - [Rating](#rating)
      - [Mood](#mood)
      - [Release Info](#release-info)
      - [Language Info](#language-info)

# [Lumina Domain](#lumina-domain)

## [Common](#common)

### [Metadata](#metadata)

#### [Genre](#genre)

```csharp
class Genre
{
    Result<Genre> Create(string name);
}
```

```json
{
    "name": "SF"
}
```

#### [Tag](#tag)

```csharp
class Tag
{
    Result<Tag> Create(string name);
}
```

```json
{
    "name": "artificial intelligence"
}
```

#### [Rating](#rating)

`Rating` is the abstract base of the ratings of media elements. It carries the numeric value, its maximum and an optional vote count, but no factory of its own, because the concrete ratings of each media type derive from it and add their own source. The rating of the audio library context, `AudioRating` with its `AudioRatingSource`, is documented alongside the music library aggregate.

```csharp
abstract class Rating
{
    decimal Value { get; }
    decimal MaxValue { get; }
    Optional<int> VoteCount { get; }
    decimal AsPercentage();
}
```

#### [Mood](#mood)

```csharp
class Mood
{
    Result<Mood> Create(string name);
}
```

```json
{
    "name": "dramatic"
}
```

#### [Release Info](#release-info)

```csharp
class ReleaseInfo
{
    Result<ReleaseInfo> Create(Optional<DateOnly> originalReleaseDate, Optional<int> originalReleaseYear,
        Optional<DateOnly> reReleaseDate, Optional<int> reReleaseYear, Optional<ReleaseCountry> releaseCountry, Optional<string> releaseVersion);
}
```

```json
{
    "originalReleaseDate": "2024-08-18",
    "originalReleaseYear": 2024,
    "reReleaseDate": "2024-08-18",
    "reReleaseYear": 2024,
    "releaseCountry": "JP",
    "releaseVersion": "director's cut"    
}
```

#### [Language Info](#language-info)

```csharp
class LanguageInfo
{
    LanguageInfo Create(string languageCode, string languageName, Optional<string> nativeName);
}
```

```json
{
    "languageCode": "FR",
    "languageName": "French",
    "nativeName": "Française",
}
```
