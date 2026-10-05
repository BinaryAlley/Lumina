#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;

/// <summary>
/// Contains unit tests for the <see cref="LibraryPathTemplateService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplateServiceTests
{
    private readonly ILibraryPathPartCatalog _mockCatalog = Substitute.For<ILibraryPathPartCatalog>();
    private readonly LibraryPathTemplateService _sut;
    private readonly LibraryPathPartFixture _libraryPathPartFixture = new();
    private readonly LibraryPathTemplateFixture _libraryPathTemplateFixture = new();
    private readonly LibraryPathPartDefinitionFixture _libraryPathPartDefinitionFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPathTemplateServiceTests"/> class.
    /// </summary>
    public LibraryPathTemplateServiceTests()
    {
        _mockCatalog.SupportedLibraryType.Returns(LibraryType.Music);
        _sut = new LibraryPathTemplateService([_mockCatalog]);
    }

    [Fact]
    public void GetDefaultTemplate_WhenCatalogExists_ShouldReturnTheCatalogDefaultTemplate()
    {
        // Arrange
        LibraryPathTemplate defaultTemplate = _libraryPathTemplateFixture.Create();
        _mockCatalog.GetDefaultTemplate().Returns(defaultTemplate);

        // Act
        LibraryPathTemplate result = _sut.GetDefaultTemplate(LibraryType.Music);

        // Assert
        Assert.Equal(defaultTemplate, result);
    }

    [Fact]
    public void GetDefaultTemplate_WhenNoCatalogExistsForTheLibraryType_ShouldReturnAnEmptyTemplate()
    {
        // Act
        LibraryPathTemplate result = _sut.GetDefaultTemplate(LibraryType.Book);

        // Assert
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void ResolveTemplate_WhenProvidedPartsAreEmpty_ShouldReturnTheCatalogDefaultTemplate()
    {
        // Arrange
        SetupCatalog();
        LibraryPathTemplate defaultTemplate = CreateMusicTemplate();
        _mockCatalog.GetDefaultTemplate().Returns(defaultTemplate);

        // Act
        Result<LibraryPathTemplate> result = _sut.ResolveTemplate(LibraryType.Music, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(defaultTemplate, result.Value);
        _mockCatalog.Received(1).GetDefaultTemplate();
    }

    [Fact]
    public void ResolveTemplate_WhenProvidedPartsAreEmptyAndNoCatalogExists_ShouldReturnAnEmptyTemplate()
    {
        // Act
        Result<LibraryPathTemplate> result = _sut.ResolveTemplate(LibraryType.Book, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.Value.IsEmpty);
    }

    [Fact]
    public void ResolveTemplate_WhenProvidedPartsAreNotEmpty_ShouldReturnATemplateWithThoseParts()
    {
        // Arrange
        SetupCatalog();
        LibraryPathPart providedPart = _libraryPathPartFixture.Create(
            kind: LibraryPathPartKind.Artist,
            representation: "{0}",
            isOptional: false);

        // Act
        Result<LibraryPathTemplate> result = _sut.ResolveTemplate(LibraryType.Music, [providedPart]);

        // Assert
        Assert.False(result.IsFailure);
        LibraryPathPart pathPart = Assert.Single(result.Value.Parts);
        Assert.Equal(LibraryPathPartKind.Artist, pathPart.Kind);
        Assert.Equal("{0}", pathPart.Representation);
        Assert.False(pathPart.IsOptional);
        _mockCatalog.DidNotReceive().GetDefaultTemplate();
    }

    [Fact]
    public void ResolveTemplate_WhenProvidedPartsAreNotSupportedByTheCatalog_ShouldReturnError()
    {
        // Arrange
        SetupCatalog(
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Artist, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Separator, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty)
        ]);
        LibraryPathPart releaseNamePart = _libraryPathPartFixture.Create(
            kind: LibraryPathPartKind.ReleaseName,
            representation: "{0}",
            isOptional: false);

        // Act
        Result<LibraryPathTemplate> result = _sut.ResolveTemplate(LibraryType.Music, [releaseNamePart]);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.PathTemplatePartKindNotSupportedForLibraryType, result.FirstError);
    }

    [Fact]
    public void ResolveTemplate_WhenNoCatalogExistsForTheLibraryTypeAndPartsAreProvided_ShouldReturnNotSupportedError()
    {
        // Arrange
        LibraryPathPart artistPart = _libraryPathPartFixture.Create(
            kind: LibraryPathPartKind.Artist,
            representation: "{0}",
            isOptional: false);

        // Act
        Result<LibraryPathTemplate> result = _sut.ResolveTemplate(LibraryType.Book, [artistPart]);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.PathTemplateIsNotSupportedForLibraryType, result.FirstError);
    }

    [Fact]
    public void Validate_WhenTemplateIsEmpty_ShouldReturnSuccess()
    {
        // Act
        Result<Success> result = _sut.Validate(LibraryType.Music, LibraryPathTemplate.Empty());

        // Assert
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Validate_WhenTemplateIsEmptyAndNoCatalogExists_ShouldReturnSuccess()
    {
        // Act
        Result<Success> result = _sut.Validate(LibraryType.Book, LibraryPathTemplate.Empty());

        // Assert
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Validate_WhenNoCatalogExistsForTheLibraryType_ShouldReturnNotSupportedError()
    {
        // Act
        Result<Success> result = _sut.Validate(LibraryType.Book, CreateMusicTemplate());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.PathTemplateIsNotSupportedForLibraryType, result.FirstError);
    }

    [Fact]
    public void Validate_WhenPartKindIsNotSupportedByTheCatalog_ShouldReturnError()
    {
        // Arrange
        SetupCatalog(definitions:
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Artist, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Separator, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.TrackName, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}")
        ]);
        LibraryPathTemplate template = _libraryPathTemplateFixture.Create(
        [
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Separator, representation: string.Empty),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.ReleaseName, representation: "{0}")
        ]);

        // Act
        Result<Success> result = _sut.Validate(LibraryType.Music, template);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.PathTemplatePartKindNotSupportedForLibraryType, result.FirstError);
    }

    [Fact]
    public void Validate_WhenAllPartKindsAreSupportedByTheCatalog_ShouldReturnSuccess()
    {
        // Arrange
        SetupCatalog();
        LibraryPathTemplate template = CreateMusicTemplate();

        // Act
        Result<Success> result = _sut.Validate(LibraryType.Music, template);

        // Assert
        Assert.False(result.IsFailure);
    }

    [Theory]
    [InlineData("")] // the relative path is empty
    [InlineData("   ")] // the relative path is whitespace
    public void Parse_WhenRelativePathIsEmpty_ShouldReturnNull(string relativePath)
    {
        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, CreateMusicTemplate(), relativePath, '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_WhenTemplateIsEmpty_ShouldReturnNull()
    {
        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, LibraryPathTemplate.Empty(), "Pink Floyd/1973 - Time.mp3", '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_WhenTemplateIsNotSupportedForTheLibraryType_ShouldReturnError()
    {
        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Book, CreateMusicTemplate(), "Pink Floyd/1973 - Time.mp3", '/');

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.PathTemplateIsNotSupportedForLibraryType, result.FirstError);
    }

    [Fact]
    public void Parse_WhenPathMatchesTheTemplate_ShouldReturnTheCapturedValues()
    {
        // Arrange
        SetupCatalog();
        LibraryPathTemplate template = CreateMusicTemplate();

        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, template, "Pink Floyd/1973 - Time.mp3", '/');

        // Assert
        Assert.False(result.IsFailure);
        ParsedLibraryPath parsedPath = result.Value!;
        Assert.Equal("Pink Floyd", parsedPath.GetString(LibraryPathPartKind.Artist).Value);
        Assert.Equal(1973, parsedPath.GetInt(LibraryPathPartKind.ReleaseYear).Value);
        Assert.Equal("Time", parsedPath.GetString(LibraryPathPartKind.TrackName).Value);
        Assert.Equal("mp3", parsedPath.GetString(LibraryPathPartKind.Extension).Value);
    }

    [Fact]
    public void Parse_WhenPathDoesNotMatchTheTemplate_ShouldReturnNull()
    {
        // Arrange
        SetupCatalog();
        LibraryPathTemplate template = CreateMusicTemplate();

        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, template, "Pink Floyd/no year/Time.mp3", '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_WhenIntegerPartHasAFixedWidth_ShouldCaptureIt()
    {
        // Arrange
        SetupCatalog(
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Artist, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Separator, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.TrackNumber, valueType: LibraryPathValueType.Integer, defaultRepresentation: "{0:00}")
        ]);
        LibraryPathTemplate template = _libraryPathTemplateFixture.Create(
        [
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Separator, representation: string.Empty),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.TrackNumber, representation: "{0:00}")
        ]);

        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, template, "Pink Floyd/07", '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(7, result.Value!.GetInt(LibraryPathPartKind.TrackNumber).Value);
    }

    [Fact]
    public void Parse_WhenIntegerPartDoesNotMatchTheFixedWidth_ShouldReturnNull()
    {
        // Arrange
        SetupCatalog(
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Artist, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Separator, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.TrackNumber, valueType: LibraryPathValueType.Integer, defaultRepresentation: "{0:00}")
        ]);
        LibraryPathTemplate template = _libraryPathTemplateFixture.Create(
        [
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Separator, representation: string.Empty),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.TrackNumber, representation: "{0:00}")
        ]);

        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, template, "Pink Floyd/007", '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_WhenYearPartHasAFourDigitValue_ShouldCaptureIt()
    {
        // Arrange
        SetupCatalog(
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.ReleaseYear, valueType: LibraryPathValueType.Year, defaultRepresentation: "{0}")
        ]);
        LibraryPathTemplate template = _libraryPathTemplateFixture.Create(
        [
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.ReleaseYear, representation: "{0}")
        ]);

        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, template, "1973", '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1973, result.Value!.GetInt(LibraryPathPartKind.ReleaseYear).Value);
    }

    [Fact]
    public void Parse_WhenYearPartDoesNotHaveAFourDigitValue_ShouldReturnNull()
    {
        // Arrange
        SetupCatalog(
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.ReleaseYear, valueType: LibraryPathValueType.Year, defaultRepresentation: "{0}")
        ]);
        LibraryPathTemplate template = _libraryPathTemplateFixture.Create(
        [
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.ReleaseYear, representation: "{0}")
        ]);

        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, template, "73", '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_WhenAnOptionalRunIsAbsent_ShouldStillMatchThePath()
    {
        // Arrange
        SetupCatalog(
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Artist, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Separator, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.DiscNumber, valueType: LibraryPathValueType.Integer, defaultRepresentation: "{0:00}")
        ]);
        LibraryPathTemplate template = _libraryPathTemplateFixture.Create(
        [
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Separator, representation: string.Empty),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.DiscNumber, representation: "{0:00}", isOptional: true)
        ]);

        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, template, "Pink Floyd/", '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Pink Floyd", result.Value!.GetString(LibraryPathPartKind.Artist).Value);
        Assert.False(result.Value.GetInt(LibraryPathPartKind.DiscNumber).HasValue);
    }

    [Fact]
    public void Parse_WhenAnOptionalRunIsPresent_ShouldCaptureIt()
    {
        // Arrange
        SetupCatalog(
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Artist, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Separator, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.DiscNumber, valueType: LibraryPathValueType.Integer, defaultRepresentation: "{0:00}")
        ]);
        LibraryPathTemplate template = _libraryPathTemplateFixture.Create(
        [
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Separator, representation: string.Empty),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.DiscNumber, representation: "{0:00}", isOptional: true)
        ]);

        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, template, "Pink Floyd/02", '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Pink Floyd", result.Value!.GetString(LibraryPathPartKind.Artist).Value);
        Assert.Equal(2, result.Value.GetInt(LibraryPathPartKind.DiscNumber).Value);
    }

    [Fact]
    public void Parse_WhenTheSameKindAppearsMoreThanOnce_ShouldKeepTheFirstCapture()
    {
        // Arrange
        SetupCatalog(
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Author, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Separator, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Title, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Literal, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty)
        ]);
        LibraryPathTemplate template = _libraryPathTemplateFixture.Create(
        [
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Author, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Separator, representation: string.Empty),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Title, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Literal, representation: " - "),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Author, representation: "{0}")
        ]);

        // Act
        Result<ParsedLibraryPath?> result = _sut.Parse(LibraryType.Music, template, "Tolkien/The Lord of the Rings - J.R.R. Tolkien", '/');

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Tolkien", result.Value!.GetString(LibraryPathPartKind.Author).Value);
    }

    /// <summary>
    /// Sets up the mock catalog to return a music-like set of part definitions and a default template.
    /// </summary>
    /// <param name="definitions">Optional. The part definitions the catalog returns. When omitted, a standard music set is used.</param>
    private void SetupCatalog(IReadOnlyList<LibraryPathPartDefinition>? definitions = null)
    {
        _mockCatalog.GetPartDefinitions().Returns(definitions ??
        [
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Literal, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Artist, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Separator, valueType: LibraryPathValueType.None, defaultRepresentation: string.Empty),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.ReleaseYear, valueType: LibraryPathValueType.Year, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.TrackNumber, valueType: LibraryPathValueType.Integer, defaultRepresentation: "{0:00}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.TrackName, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}"),
            _libraryPathPartDefinitionFixture.Create(kind: LibraryPathPartKind.Extension, valueType: LibraryPathValueType.Text, defaultRepresentation: "{0}")
        ]);
    }

    /// <summary>
    /// Creates a music-like path template whose parts are all supported by the standard music set of definitions.
    /// </summary>
    /// <returns>The created path template.</returns>
    private LibraryPathTemplate CreateMusicTemplate()
    {
        return _libraryPathTemplateFixture.Create(
        [
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Separator, representation: string.Empty),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.ReleaseYear, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Literal, representation: " - "),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.TrackName, representation: "{0}"),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Literal, representation: "."),
            _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Extension, representation: "{0}")
        ]);
    }
}
