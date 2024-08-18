- [Lumina Domain](#lumina-domain)
  - [Common](#common)
    - [Metadata](#metadata)
      - [Genre](#genre)
      - [Tag](#tag)
      - [Rating](#rating)
      - [Release Info](#release-info)
      - [Language Info](#language-info)

# [Lumina Domain](#lumina-domain)

## [Common](#common)

### [Metadata](#metadata)

#### [Genre](#genre)

```csharp
class Genre
{
    ErrorOr<Genre> Create(string name);
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
    ErrorOr<Tag> Create(string name);
}
```

```json
{
    "name": "artificial intelligence"
}
```

#### [Rating](#rating)

```csharp
class Rating
{
    ErrorOr<Rating> Create(decimal value, decimal maxValue, Optional<RatingSource> source = default, Optional<int> voteCount = default);
}
```

```json
{
    "value": 9.3,
    "maxValue": 10.0,
    "source": "Imdb",
    "voteCount": 84
}
```

#### [Release Info](#release-info)

```csharp
class ReleaseInfo
{
    ErrorOr<ReleaseInfo> Create(Optional<DateOnly> originalReleaseDate, Optional<int> originalReleaseYear,
        Optional<DateOnly> reReleaseDate, Optional<int> reReleaseYear, Optional<string> releaseCountry, Optional<string> releaseVersion);
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
    ErrorOr<LanguageInfo> Create(string languageCode, string languageName, Optional<string> nativeName);
}
```

```json
{
    "languageCode": "FR",
    "languageName": "French",
    "nativeName": "Française",
}
```