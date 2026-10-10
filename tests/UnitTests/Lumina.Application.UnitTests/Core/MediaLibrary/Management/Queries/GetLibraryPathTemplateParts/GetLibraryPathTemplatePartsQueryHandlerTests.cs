#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;
using Lumina.Contracts.Responses.MediaLibrary.Management;
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
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryPathTemplatePartsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryPathTemplatePartsQueryHandlerTests
{
    private readonly ILibraryPathPartCatalog _mockBookCatalog;
    private readonly ILibraryPathPartCatalog _mockMusicCatalog;
    private readonly GetLibraryPathTemplatePartsQueryHandler _sut;
    private readonly GetLibraryPathTemplatePartsQueryFixture _getLibraryPathTemplatePartsQueryFixture = new();
    private readonly LibraryPathPartDefinitionFixture _libraryPathPartDefinitionFixture = new();
    private readonly LibraryPathPartFixture _libraryPathPartFixture = new();
    private readonly LibraryPathTemplateFixture _libraryPathTemplateFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryPathTemplatePartsQueryHandlerTests"/> class.
    /// </summary>
    public GetLibraryPathTemplatePartsQueryHandlerTests()
    {
        _mockBookCatalog = Substitute.For<ILibraryPathPartCatalog>();
        _mockBookCatalog.SupportedLibraryType.Returns(LibraryType.Book);
        _mockBookCatalog.GetPartDefinitions().Returns([]);
        _mockBookCatalog.GetDefaultTemplate().Returns(LibraryPathTemplate.Empty());
        _mockMusicCatalog = Substitute.For<ILibraryPathPartCatalog>();
        _mockMusicCatalog.SupportedLibraryType.Returns(LibraryType.Music);
        _mockMusicCatalog.GetPartDefinitions().Returns([]);
        _mockMusicCatalog.GetDefaultTemplate().Returns(LibraryPathTemplate.Empty());

        _sut = new GetLibraryPathTemplatePartsQueryHandler([_mockBookCatalog, _mockMusicCatalog]);
    }

    [Theory]
    [InlineData("")] // an absent library type
    [InlineData("   ")] // a whitespace library type
    [InlineData("NotARealLibraryType")] // an unknown library type
    public async Task HandleAsync_WhenLibraryTypeIsUnknownOrAbsent_ShouldReturnUnknownLibraryTypeError(string libraryType)
    {
        // Arrange
        GetLibraryPathTemplatePartsQuery query = _getLibraryPathTemplatePartsQueryFixture.Create(libraryType: libraryType);

        // Act
        Result<LibraryPathTemplateCatalogResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.UnknownLibraryType, result.FirstError);
        _ = _mockBookCatalog.DidNotReceive().SupportedLibraryType;
        _ = _mockMusicCatalog.DidNotReceive().SupportedLibraryType;
    }

    [Fact]
    public async Task HandleAsync_WhenNoCatalogSupportsLibraryType_ShouldReturnEmptyPartsAndDefaultTemplateParts()
    {
        // Arrange
        GetLibraryPathTemplatePartsQuery query = _getLibraryPathTemplatePartsQueryFixture.Create(libraryType: nameof(LibraryType.Photo));

        // Act
        Result<LibraryPathTemplateCatalogResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Parts);
        Assert.Empty(result.Value.DefaultTemplateParts);
        _ = _mockBookCatalog.Received(1).SupportedLibraryType;
        _ = _mockMusicCatalog.Received(1).SupportedLibraryType;
        _mockBookCatalog.DidNotReceive().GetPartDefinitions();
        _mockBookCatalog.DidNotReceive().GetDefaultTemplate();
        _mockMusicCatalog.DidNotReceive().GetPartDefinitions();
        _mockMusicCatalog.DidNotReceive().GetDefaultTemplate();
    }

    [Fact]
    public async Task HandleAsync_WhenCatalogSupportsLibraryType_ShouldMapPartDefinitionsAndDefaultTemplatePartsInOrder()
    {
        // Arrange
        List<LibraryPathPartDefinition> partDefinitions =
        [
            _libraryPathPartDefinitionFixture.Create(
                kind: LibraryPathPartKind.Artist,
                valueType: LibraryPathValueType.Text,
                defaultRepresentation: "{0}",
                isOptionalByDefault: false),
            _libraryPathPartDefinitionFixture.Create(
                kind: LibraryPathPartKind.TrackNumber,
                valueType: LibraryPathValueType.Integer,
                defaultRepresentation: "{0:00}",
                isOptionalByDefault: true),
            _libraryPathPartDefinitionFixture.Create(
                kind: LibraryPathPartKind.ReleaseYear,
                valueType: LibraryPathValueType.Year,
                defaultRepresentation: "{0}",
                isOptionalByDefault: false)
        ];
        _mockMusicCatalog.GetPartDefinitions().Returns(partDefinitions);

        LibraryPathPart artistPart = _libraryPathPartFixture.Create(
            kind: LibraryPathPartKind.Artist,
            representation: "{0}",
            isOptional: false);
        LibraryPathPart separatorPart = _libraryPathPartFixture.Create(
            kind: LibraryPathPartKind.Separator,
            representation: string.Empty,
            isOptional: false);
        LibraryPathPart trackNamePart = _libraryPathPartFixture.Create(
            kind: LibraryPathPartKind.TrackName,
            representation: "{0}",
            isOptional: true);
        LibraryPathTemplate defaultTemplate = _libraryPathTemplateFixture.Create(parts: [artistPart, separatorPart, trackNamePart]);
        _mockMusicCatalog.GetDefaultTemplate().Returns(defaultTemplate);

        GetLibraryPathTemplatePartsQuery query = _getLibraryPathTemplatePartsQueryFixture.Create(libraryType: nameof(LibraryType.Music));

        // Act
        Result<LibraryPathTemplateCatalogResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Parts.Count);
        Assert.Equal(LibraryPathPartKind.Artist.ToString(), result.Value.Parts[0].Kind);
        Assert.Equal(LibraryPathValueType.Text.ToString(), result.Value.Parts[0].ValueType);
        Assert.Equal("{0}", result.Value.Parts[0].DefaultRepresentation);
        Assert.False(result.Value.Parts[0].IsOptionalByDefault);
        Assert.Equal(LibraryPathPartKind.TrackNumber.ToString(), result.Value.Parts[1].Kind);
        Assert.Equal(LibraryPathValueType.Integer.ToString(), result.Value.Parts[1].ValueType);
        Assert.Equal("{0:00}", result.Value.Parts[1].DefaultRepresentation);
        Assert.True(result.Value.Parts[1].IsOptionalByDefault);
        Assert.Equal(LibraryPathPartKind.ReleaseYear.ToString(), result.Value.Parts[2].Kind);
        Assert.Equal(LibraryPathValueType.Year.ToString(), result.Value.Parts[2].ValueType);
        Assert.Equal("{0}", result.Value.Parts[2].DefaultRepresentation);
        Assert.False(result.Value.Parts[2].IsOptionalByDefault);

        Assert.Equal(3, result.Value.DefaultTemplateParts.Count);
        Assert.Equal(LibraryPathPartKind.Artist.ToString(), result.Value.DefaultTemplateParts[0].Kind);
        Assert.Equal("{0}", result.Value.DefaultTemplateParts[0].Representation);
        Assert.False(result.Value.DefaultTemplateParts[0].IsOptional);
        Assert.Equal(LibraryPathPartKind.Separator.ToString(), result.Value.DefaultTemplateParts[1].Kind);
        Assert.Equal(string.Empty, result.Value.DefaultTemplateParts[1].Representation);
        Assert.False(result.Value.DefaultTemplateParts[1].IsOptional);
        Assert.Equal(LibraryPathPartKind.TrackName.ToString(), result.Value.DefaultTemplateParts[2].Kind);
        Assert.Equal("{0}", result.Value.DefaultTemplateParts[2].Representation);
        Assert.True(result.Value.DefaultTemplateParts[2].IsOptional);

        _mockMusicCatalog.Received(1).GetPartDefinitions();
        _mockMusicCatalog.Received(1).GetDefaultTemplate();
        _mockBookCatalog.DidNotReceive().GetPartDefinitions();
        _mockBookCatalog.DidNotReceive().GetDefaultTemplate();
    }
}
