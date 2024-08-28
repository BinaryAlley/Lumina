#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Converters;

/// <summary>
/// Custom JSON converter for the <see cref="Book"/> class.
/// </summary>
public class BookJsonConverter : JsonConverter<Book>
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Reads and converts JSON to a <see cref="Book"/> object.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type of the object to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted <see cref="Book"/> object.</returns>
    public override Book Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;

            var id = root.GetProperty("id").GetProperty("value").GetGuid();
            var created = root.GetProperty("created").GetDateTime();
            var metadata = DeserializeMetadata(root.GetProperty("metadata"));
            var format = Enum.Parse<BookFormat>(root.GetProperty("format").GetString()!);
            var edition = root.GetProperty("edition").GetString();
            var volumeNumber = root.GetProperty("volumeNumber").GetInt32();
            var series = DeserializeBookSeries(root.GetProperty("series"));
            var asin = root.GetProperty("asin").GetString();
            var goodreadsId = root.GetProperty("goodreadsId").GetString();
            var lccn = root.GetProperty("lccn").GetString();
            var oclcNumber = root.GetProperty("oclcNumber").GetString();
            var openLibraryId = root.GetProperty("openLibraryId").GetString();
            var libraryThingId = root.GetProperty("libraryThingId").GetString();
            var googleBooksId = root.GetProperty("googleBooksId").GetString();
            var barnesAndNobleId = root.GetProperty("barnesAndNobleId").GetString();
            var appleBooksId = root.GetProperty("appleBooksId").GetString();
            var isbns = DeserializeIsbns(root.GetProperty("isbNs"));
            var contributorIds = DeserializeContributorIds(root.GetProperty("contributors"));
            var ratings = DeserializeRatings(root.GetProperty("ratings"));

            var createBookResult = Book.Create(
                BookId.Create(id),
                metadata,
                format,
                Optional<string>.FromNullable(edition),
                volumeNumber,
                Optional<BookSeries>.FromNullable(series),
                Optional<string>.FromNullable(asin),
                Optional<string>.FromNullable(goodreadsId),
                Optional<string>.FromNullable(lccn),
                Optional<string>.FromNullable(oclcNumber),
                Optional<string>.FromNullable(openLibraryId),
                Optional<string>.FromNullable(libraryThingId),
                Optional<string>.FromNullable(googleBooksId),
                Optional<string>.FromNullable(barnesAndNobleId),
                Optional<string>.FromNullable(appleBooksId),
                created,
                default,
                isbns,
                contributorIds,
                ratings: ratings
            );
            if (createBookResult.IsError)
                throw new JsonException($"Failed to create Book: {string.Join(", ", createBookResult.Errors)}");
            return createBookResult.Value;
        }
    }

    /// <summary>
    /// Writes a <see cref="Book"/> object to JSON. This method is not implemented.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The <see cref="Book"/> object to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, Book value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Serialization is not implemented for Book class.");
    }

    /// <summary>
    /// Deserializes the metadata from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the metadata.</param>
    /// <returns>The deserialized <see cref="WrittenContentMetadata"/>.</returns>
    private WrittenContentMetadata DeserializeMetadata(JsonElement element)
    {
        var title = element.GetProperty("title").GetString();
        var originalTitle = element.GetProperty("originalTitle").GetString();
        var description = element.GetProperty("description").GetString();
        var releaseInfo = DeserializeReleaseInfo(element.GetProperty("releaseInfo"));
        var genres = DeserializeGenres(element.GetProperty("genres"));
        var tags = DeserializeTags(element.GetProperty("tags"));
        var language = DeserializeLanguageInfo(element.GetProperty("language"));
        var originalLanguage = DeserializeLanguageInfo(element.GetProperty("originalLanguage"));
        var publisher = element.GetProperty("publisher").GetString();
        var pageCount = element.GetProperty("pageCount").GetInt32();

        var metadataResult = WrittenContentMetadata.Create(
            title!,
            Optional<string>.FromNullable(originalTitle),
            Optional<string>.FromNullable(description),
            releaseInfo,
            genres,
            tags,
            Optional<LanguageInfo>.FromNullable(language),
            Optional<LanguageInfo>.FromNullable(originalLanguage),
            Optional<string>.FromNullable(publisher),
            Optional<int>.FromNullable(pageCount)
        );

        if (metadataResult.IsError)
        {
            throw new JsonException($"Failed to create WrittenContentMetadata: {string.Join(", ", metadataResult.Errors)}");
        }

        return metadataResult.Value;
    }

    /// <summary>
    /// Deserializes the release info from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the release info.</param>
    /// <returns>The deserialized <see cref="ReleaseInfo"/>.</returns>
    private ReleaseInfo DeserializeReleaseInfo(JsonElement element)
    {
        var originalReleaseDate = element.TryGetProperty("originalReleaseDate", out var ordElement) ?
            DateOnly.Parse(ordElement.GetString()!) : (DateOnly?)null;
        var originalReleaseYear = element.TryGetProperty("originalReleaseYear", out var oryElement) ?
            oryElement.GetInt32() : (int?)null;
        var reReleaseDate = element.TryGetProperty("reReleaseDate", out var rrdElement) ?
            DateOnly.Parse(rrdElement.GetString()!) : (DateOnly?)null;
        var reReleaseYear = element.TryGetProperty("reReleaseYear", out var rryElement) ?
            rryElement.GetInt32() : (int?)null;
        var releaseCountry = element.TryGetProperty("releaseCountry", out var rcElement) ?
            rcElement.GetString() : null;
        var releaseVersion = element.TryGetProperty("releaseVersion", out var rvElement) ?
            rvElement.GetString() : null;

        var releaseInfoResult = ReleaseInfo.Create(
            Optional<DateOnly>.FromNullable(originalReleaseDate),
            Optional<int>.FromNullable(originalReleaseYear),
            Optional<DateOnly>.FromNullable(reReleaseDate),
            Optional<int>.FromNullable(reReleaseYear),
            Optional<string>.FromNullable(releaseCountry),
            Optional<string>.FromNullable(releaseVersion)
        );

        if (releaseInfoResult.IsError)
            throw new JsonException($"Failed to create ReleaseInfo: {string.Join(", ", releaseInfoResult.Errors)}");
        return releaseInfoResult.Value;
    }

    /// <summary>
    /// Deserializes the genres from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the genres.</param>
    /// <returns>The deserialized list of <see cref="Genre"/>.</returns>
    private List<Genre> DeserializeGenres(JsonElement element)
    {
        return element.EnumerateArray()
            .Select(g => Genre.Create(g.GetProperty("name").GetString()!).Value)
            .ToList();
    }

    /// <summary>
    /// Deserializes the tags from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the tags.</param>
    /// <returns>The deserialized list of <see cref="Tag"/>.</returns>
    private List<Tag> DeserializeTags(JsonElement element)
    {
        return element.EnumerateArray()
            .Select(t => Tag.Create(t.GetProperty("name").GetString()!).Value)
            .ToList();
    }

    /// <summary>
    /// Deserializes the language info from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the language info.</param>
    /// <returns>The deserialized <see cref="LanguageInfo"/>.</returns>
    private LanguageInfo DeserializeLanguageInfo(JsonElement element)
    {
        var languageCode = element.GetProperty("languageCode").GetString();
        var languageName = element.GetProperty("languageName").GetString();
        var nativeName = element.TryGetProperty("nativeName", out var nnElement) ? nnElement.GetString() : null;

        var languageInfoResult = LanguageInfo.Create(
            languageCode!,
            languageName!,
            Optional<string>.FromNullable(nativeName)
        );

        if (languageInfoResult.IsError)
        {
            throw new JsonException($"Failed to create LanguageInfo: {string.Join(", ", languageInfoResult.Errors)}");
        }

        return languageInfoResult.Value;
    }

    /// <summary>
    /// Deserializes the book series from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the book series.</param>
    /// <returns>The deserialized <see cref="BookSeries"/>.</returns>
    private BookSeries DeserializeBookSeries(JsonElement element)
    {
        // TODO: implement when book series are implemented
        return null!;
    }

    /// <summary>
    /// Deserializes the ISBNs from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the ISBNs.</param>
    /// <returns>The deserialized list of <see cref="Isbn"/>.</returns>
    private List<Isbn> DeserializeIsbns(JsonElement element)
    {
        return element.EnumerateArray()
            .Select(i => Isbn.Create(i.GetProperty("value").GetString()!,
                Enum.Parse<IsbnFormat>(i.GetProperty("format").GetString()!)).Value)
            .ToList();
    }

    /// <summary>
    /// Deserializes the contributor IDs from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the contributor IDs.</param>
    /// <returns>The deserialized list of <see cref="MediaContributorId"/>.</returns>
    private List<MediaContributorId> DeserializeContributorIds(JsonElement element)
    {
        // TODO: implement when contributors are implemented
        return new List<MediaContributorId>();
    }

    /// <summary>
    /// Deserializes the ratings from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the ratings.</param>
    /// <returns>The deserialized list of <see cref="BookRating"/>.</returns>
    private List<BookRating> DeserializeRatings(JsonElement element)
    {
        return element.EnumerateArray()
            .Select(r => BookRating.Create(
                r.GetProperty("value").GetInt32(),
                r.GetProperty("maxValue").GetInt32(),
                Optional<BookRatingSource>.FromNullable(Enum.Parse<BookRatingSource>(r.GetProperty("source").GetString()!)),
                Optional<int>.FromNullable(r.GetProperty("voteCount").GetInt32())
            ).Value)
            .ToList();
    }
    #endregion
}