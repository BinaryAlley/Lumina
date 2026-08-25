#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Plugins.Calibre.Common.Models.DTO.Opf;
using Lumina.Plugins.Calibre.Core.Mapping;
using Lumina.Plugins.Calibre.Fixtures.Common.Models.DTO.Opf;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Calibre.UnitTests.Core.Mapping;

/// <summary>
/// Contains unit tests for the <see cref="CalibreMapper"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CalibreMapperTests
{
    private readonly OpfDocumentDtoFixture _opfDocumentDtoFixture = new();
    private readonly OpfCreatorDtoFixture _opfCreatorDtoFixture = new();
    private readonly OpfContributorDtoFixture _opfContributorDtoFixture = new();
    private readonly OpfIdentifierDtoFixture _opfIdentifierDtoFixture = new();

    [Fact]
    public void Map_WhenDocumentHasAllFields_ShouldMapThemToTheBookMetadataDto()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(
            title: "Test Book Title",
            description: "<p>A test description with <b>markup</b>.   </p>",
            publisher: "Test Publisher",
            publishDate: new DateTimeOffset(2015, 6, 15, 0, 0, 0, TimeSpan.Zero),
            languageCode: "en",
            series: "The Series",
            seriesIndex: 2.5,
            rating: 8,
            subjects: ["Science fiction", "Space opera"],
            creators: [_opfCreatorDtoFixture.Create("Test Author", "aut"), _opfCreatorDtoFixture.Create("Co Author", "oth")],
            contributors:
            [
                _opfContributorDtoFixture.Create("calibre (3.48.0) [https://calibre-ebook.com]", "bkp"),
                _opfContributorDtoFixture.Create("Test Translator", "trl")
            ],
            identifiers:
            [
                _opfIdentifierDtoFixture.Create("ISBN", "978-0-306-40615-7"),
                _opfIdentifierDtoFixture.Create("goodreads", "123456"),
                _opfIdentifierDtoFixture.Create("ASIN", "B00TEST1"),
                _opfIdentifierDtoFixture.Create("lccn", "test-lccn"),
                _opfIdentifierDtoFixture.Create("oclc", "ocm12345678"),
                _opfIdentifierDtoFixture.Create("openlibrary", "OL100M"),
                _opfIdentifierDtoFixture.Create("google", "google-id")
            ]);

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Equal("Test Book Title", result.Title);
        Assert.Equal("A test description with markup .", result.Description);
        Assert.Equal("Test Publisher", result.Publisher);
        Assert.NotNull(result.ReleaseInfo);
        Assert.Equal(new DateOnly(2015, 6, 15), result.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(2015, result.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal("The Series", result.Series!.Title);
        Assert.Equal(2.5f, result.VolumeNumber);
        Assert.Equal("B00TEST1", result.ASIN);
        Assert.Equal("123456", result.GoodreadsId);
        Assert.Equal("test-lccn", result.LCCN);
        Assert.Equal("ocm12345678", result.OCLCNumber);
        Assert.Equal("OL100M", result.OpenLibraryId);
        Assert.Equal("google-id", result.GoogleBooksId);
        Assert.Null(result.Genres);
        Assert.Null(result.CoverImagePath);
    }

    [Fact]
    public void Map_WhenDescriptionContainsHtmlEntities_ShouldDecodeAndStripThem()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(description: "A &amp; B &lt;tag&gt;description&lt;/tag&gt; with  multiple   spaces");

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Equal("A & B description with multiple spaces", result.Description);
    }

    [Fact]
    public void Map_WhenDescriptionIsEmpty_ShouldReturnNullDescription()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(description: "   ");

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Null(result.Description);
    }

    [Theory]
    [InlineData(0)] // a zero series index is not a meaningful volume number
    public void Map_WhenSeriesIndexIsZero_ShouldReturnNullVolumeNumber(double seriesIndex)
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(seriesIndex: seriesIndex);

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Null(result.VolumeNumber);
    }

    [Fact]
    public void Map_WhenSeriesIndexIsNull_ShouldReturnNullVolumeNumber()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create();

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Null(result.VolumeNumber);
    }

    [Fact]
    public void Map_WhenSeriesIsNull_ShouldReturnNullSeries()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create();

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Null(result.Series);
    }

    [Fact]
    public void Map_WhenRatingIsPresent_ShouldMapItToACalibreRatingWithMaxValueTen()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(rating: 8);

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        BookRatingDto rating = Assert.Single(result.Ratings!);
        Assert.Equal(8m, rating.Value);
        Assert.Equal(10m, rating.MaxValue);
        Assert.Equal(BookRatingSource.Calibre, rating.Source);
        Assert.Null(rating.VoteCount);
    }

    [Fact]
    public void Map_WhenRatingIsMissing_ShouldReturnNullRatings()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create();

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Null(result.Ratings);
    }

    [Fact]
    public void Map_WhenSubjectsArePresent_ShouldMapThemToTagsWithNullGenres()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(subjects: ["Science fiction", "Space opera"]);

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Null(result.Genres);
        Assert.Equal(2, result.Tags!.Count);
        Assert.Equal("Science fiction", result.Tags[0].Name);
        Assert.Equal("Space opera", result.Tags[1].Name);
    }

    [Fact]
    public void Map_WhenCreatorsAndContributorsArePresent_ShouldMapRolesAndCategories()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(
            creators: [_opfCreatorDtoFixture.Create("Test Author", "aut"), _opfCreatorDtoFixture.Create("Co Author", null)],
            contributors:
            [
                _opfContributorDtoFixture.Create("Test Translator", "trl"),
                _opfContributorDtoFixture.Create("Test Illustrator", "ill"),
                _opfContributorDtoFixture.Create("calibre (3.48.0) [https://calibre-ebook.com]", "bkp"),
                _opfContributorDtoFixture.Create("Misc Contributor", null)
            ]);

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Equal(6, result.Contributors!.Count);

        MediaContributorDto author = result.Contributors[0];
        Assert.Equal("Test Author", author.Name!.DisplayName);
        Assert.Equal("Author", author.Role!.Name);
        Assert.Equal("Writing", author.Role.Category);

        MediaContributorDto coAuthor = result.Contributors[1];
        Assert.Equal("Co Author", coAuthor.Name!.DisplayName);
        Assert.Equal("Author", coAuthor.Role!.Name);
        Assert.Equal("Writing", coAuthor.Role.Category);

        MediaContributorDto translator = result.Contributors[2];
        Assert.Equal("Test Translator", translator.Name!.DisplayName);
        Assert.Equal("trl", translator.Role!.Name);
        Assert.Equal("Translation", translator.Role.Category);

        MediaContributorDto illustrator = result.Contributors[3];
        Assert.Equal("Test Illustrator", illustrator.Name!.DisplayName);
        Assert.Equal("ill", illustrator.Role!.Name);
        Assert.Equal("Art", illustrator.Role.Category);

        MediaContributorDto production = result.Contributors[4];
        Assert.Equal("calibre (3.48.0) [https://calibre-ebook.com]", production.Name!.DisplayName);
        Assert.Equal("bkp", production.Role!.Name);
        Assert.Equal("Production", production.Role.Category);

        MediaContributorDto misc = result.Contributors[5];
        Assert.Equal("Misc Contributor", misc.Name!.DisplayName);
        Assert.Equal("Contributor", misc.Role!.Name);
        Assert.Equal("Other", misc.Role.Category);
    }

    [Fact]
    public void Map_WhenCreatorsAreDuplicatedWithTheSameRole_ShouldDeDuplicateThem()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(creators: [_opfCreatorDtoFixture.Create("Test Author", "aut"), _opfCreatorDtoFixture.Create("Test Author", "aut")]);

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        MediaContributorDto author = Assert.Single(result.Contributors!);
        Assert.Equal("Test Author", author.Name!.DisplayName);
    }

    [Fact]
    public void Map_WhenIsbnIdentifiersArePresent_ShouldNormalizeAndMapThem()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(identifiers:
        [
            _opfIdentifierDtoFixture.Create("ISBN", "978-0-306-40615-7"),
            _opfIdentifierDtoFixture.Create("ISBN", "0306406152"),
            _opfIdentifierDtoFixture.Create("ISBN", "978-0-306-40615-7"),
            _opfIdentifierDtoFixture.Create("ISBN", "not-an-isbn")
        ]);

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Equal(2, result.Isbns!.Count);
        Assert.Contains(result.Isbns, isbn => isbn.Value == "9780306406157" && isbn.Format == IsbnFormat.Isbn13);
        Assert.Contains(result.Isbns, isbn => isbn.Value == "0306406152" && isbn.Format == IsbnFormat.Isbn10);
    }

    [Fact]
    public void Map_WhenNoIdentifiersArePresent_ShouldReturnEmptyIsbnList()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create();

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Empty(result.Isbns!);
    }

    [Fact]
    public void Map_WhenAmazonIdentifiersArePresent_ShouldMapAsin()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(identifiers: [_opfIdentifierDtoFixture.Create("amazon", "B00TEST1")]);

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Equal("B00TEST1", result.ASIN);
    }

    [Fact]
    public void Map_WhenLanguageCodeIsUnknown_ShouldReturnTheRawCode()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(languageCode: "zz");

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Equal("zz", result.Language!.LanguageCode);
        Assert.Equal("zz", result.Language.LanguageName);
    }

    [Fact]
    public void Map_WhenLanguageCodeIsEmpty_ShouldReturnNullLanguage()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(languageCode: "  ");

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Null(result.Language);
    }

    [Fact]
    public void Map_WhenDocumentHasNoMetadata_ShouldReturnEmptyDto()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create();

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        Assert.Null(result.Title);
        Assert.Null(result.Description);
        Assert.NotNull(result.ReleaseInfo);
        Assert.Null(result.Publisher);
        Assert.Null(result.Series);
        Assert.Null(result.VolumeNumber);
        Assert.Null(result.Ratings);
    }

    [Fact]
    public void Map_WhenPublishDateIsMissing_ShouldReturnEmptyReleaseInfo()
    {
        // Arrange
        OpfDocumentDto document = _opfDocumentDtoFixture.Create(publishDate: null);

        // Act
        BookMetadataDto result = CalibreMapper.Map(document);

        // Assert
        // a book without a publication date must still be enrichable, so the release info is present with empty fields
        Assert.NotNull(result.ReleaseInfo);
        Assert.Null(result.ReleaseInfo!.OriginalReleaseDate);
        Assert.Null(result.ReleaseInfo.OriginalReleaseYear);
        Assert.Null(result.ReleaseInfo.ReReleaseDate);
        Assert.Null(result.ReleaseInfo.ReReleaseYear);
        Assert.Null(result.ReleaseInfo.ReleaseCountry);
        Assert.Null(result.ReleaseInfo.ReleaseVersion);
    }
}
