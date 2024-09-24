#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
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

            Guid id = root.GetProperty("id").GetProperty("value").GetGuid();
            DateTime created = root.GetProperty("created").GetDateTime();
            WrittenContentMetadata metadata = DeserializeMetadata(root.GetProperty("metadata"));
            BookFormat? format = null;
            if (root.TryGetProperty("format", out JsonElement formatElement))
            {
                if (formatElement.ValueKind == JsonValueKind.String)
                {
                    string formatString = formatElement.GetString()!;
                    if (Enum.TryParse(formatString, out BookFormat parsedFormat))
                        format = parsedFormat;
                }
            }
            string? edition = root.GetProperty("edition").GetString();
            int volumeNumber = root.GetProperty("volumeNumber").GetInt32();
            BookSeries series = DeserializeBookSeries(root.GetProperty("series"));
            string? asin = root.GetProperty("asin").GetString();
            string? goodreadsId = root.GetProperty("goodreadsId").GetString();
            string? lccn = root.GetProperty("lccn").GetString();
            string? oclcNumber = root.GetProperty("oclcNumber").GetString();
            string? openLibraryId = root.GetProperty("openLibraryId").GetString();
            string? libraryThingId = root.GetProperty("libraryThingId").GetString();
            string? googleBooksId = root.GetProperty("googleBooksId").GetString();
            string? barnesAndNobleId = root.GetProperty("barnesAndNobleId").GetString();
            string? appleBooksId = root.GetProperty("appleBooksId").GetString();
            List<Isbn> isbns = DeserializeIsbns(root.GetProperty("isbNs"));
            List<MediaContributorId> contributorIds = DeserializeContributorIds(root.GetProperty("contributors"));
            List<BookRating> ratings = DeserializeRatings(root.GetProperty("ratings"));

            ErrorOr.ErrorOr<Book> createBookResult = Book.Create(
                BookId.Create(id),
                metadata,
                Optional<BookFormat>.FromNullable(format),
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
        string? title = element.GetProperty("title").GetString();
        string? originalTitle = element.GetProperty("originalTitle").GetString();
        string? description = element.GetProperty("description").GetString();
        ReleaseInfo releaseInfo = DeserializeReleaseInfo(element.GetProperty("releaseInfo"));
        List<Genre> genres = DeserializeGenres(element.GetProperty("genres"));
        List<Tag> tags = DeserializeTags(element.GetProperty("tags"));
        LanguageInfo? language = DeserializeLanguageInfo(element.GetProperty("language"));
        LanguageInfo? originalLanguage = DeserializeLanguageInfo(element.GetProperty("originalLanguage"));
        string? publisher = element.GetProperty("publisher").GetString();
        int? pageCount = null;
        if (element.TryGetProperty("pageCount", out JsonElement pageCountElement))
            if (pageCountElement.ValueKind != JsonValueKind.Null)
                pageCount = pageCountElement.GetInt32();
        ErrorOr.ErrorOr<WrittenContentMetadata> metadataResult = WrittenContentMetadata.Create(
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
            throw new JsonException($"Failed to create WrittenContentMetadata: {string.Join(", ", metadataResult.Errors)}");
        return metadataResult.Value;
    }

    /// <summary>
    /// Deserializes the release info from JSON.
    /// </summary>
    /// <param name="element">The JSON element containing the release info.</param>
    /// <returns>The deserialized <see cref="ReleaseInfo"/>.</returns>
    private ReleaseInfo DeserializeReleaseInfo(JsonElement element)
    {
        DateOnly? originalReleaseDate = element.TryGetProperty("originalReleaseDate", out JsonElement ordElement) ?
            DateOnly.Parse(ordElement.GetString()!) : (DateOnly?)null;
        int? originalReleaseYear = element.TryGetProperty("originalReleaseYear", out JsonElement oryElement) && oryElement.ValueKind == JsonValueKind.Number
            ? oryElement.GetInt32() : (int?)null;
        DateOnly? reReleaseDate = element.TryGetProperty("reReleaseDate", out JsonElement rrdElement) && rrdElement.ValueKind == JsonValueKind.String ?
            DateOnly.Parse(rrdElement.GetString()!) : (DateOnly?)null;
        int? reReleaseYear = element.TryGetProperty("reReleaseYear", out JsonElement rryElement) && rryElement.ValueKind == JsonValueKind.Number
           ? rryElement.GetInt32() : (int?)null;
        string? releaseCountry = element.TryGetProperty("releaseCountry", out JsonElement rcElement) ?
            rcElement.GetString() : null;
        string? releaseVersion = element.TryGetProperty("releaseVersion", out JsonElement rvElement) ?
            rvElement.GetString() : null;

        ErrorOr.ErrorOr<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
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
    private LanguageInfo? DeserializeLanguageInfo(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Null)
        {
            string? languageCode = element.GetProperty("languageCode").GetString();
            string? languageName = element.GetProperty("languageName").GetString();
            string? nativeName = element.TryGetProperty("nativeName", out JsonElement nnElement) ? nnElement.GetString() : null;

            ErrorOr.ErrorOr<LanguageInfo> languageInfoResult = LanguageInfo.Create(
                languageCode!,
                languageName!,
                Optional<string>.FromNullable(nativeName)
            );

            if (languageInfoResult.IsError)
                throw new JsonException($"Failed to create LanguageInfo: {string.Join(", ", languageInfoResult.Errors)}");
            return languageInfoResult.Value;
        }
        else
            return null;
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
        return [];
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
                Optional<int>.FromNullable(
                    r.TryGetProperty("voteCount", out JsonElement voteCountElement) && voteCountElement.ValueKind != JsonValueKind.Null
                        ? voteCountElement.GetInt32()
                        : (int?)null)
            ).Value)
            .ToList();
    }
    #endregion
}
