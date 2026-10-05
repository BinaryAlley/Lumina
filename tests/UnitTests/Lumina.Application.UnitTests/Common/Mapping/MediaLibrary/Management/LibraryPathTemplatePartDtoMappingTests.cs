#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="LibraryPathTemplatePartDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathTemplatePartDtoMappingTests
{
    private readonly LibraryPathTemplatePartDtoFixture _libraryPathTemplatePartDtoFixture = new();

    [Fact]
    public void ToDomainParts_WhenPartsIsNull_ShouldReturnEmptyList()
    {
        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = LibraryPathTemplatePartDtoMapping.ToDomainParts(null);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void ToDomainParts_WhenPartsIsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        List<LibraryPathTemplatePartDto> parts = [];

        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = parts.ToDomainParts();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void ToDomainParts_WhenPartsAreValid_ShouldMapKindRepresentationAndIsOptional()
    {
        // Arrange
        LibraryPathTemplatePartDto literalPart = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Literal),
            representation: "Various Artists",
            isOptional: false);
        LibraryPathTemplatePartDto typedPart = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Artist),
            representation: "{0}",
            isOptional: true);
        LibraryPathTemplatePartDto[] parts = [literalPart, typedPart];

        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = parts.ToDomainParts();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(LibraryPathPartKind.Literal, result.Value[0].Kind);
        Assert.Equal("Various Artists", result.Value[0].Representation);
        Assert.False(result.Value[0].IsOptional);
        Assert.Equal(LibraryPathPartKind.Artist, result.Value[1].Kind);
        Assert.Equal("{0}", result.Value[1].Representation);
        Assert.True(result.Value[1].IsOptional);
    }

    [Fact]
    public void ToDomainParts_WhenPartIsSeparator_ShouldMapWithEmptyRepresentation()
    {
        // Arrange
        LibraryPathTemplatePartDto separatorPart = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Separator),
            representation: @"\",
            isOptional: false);
        LibraryPathTemplatePartDto[] parts = [separatorPart];

        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = parts.ToDomainParts();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal(LibraryPathPartKind.Separator, result.Value[0].Kind);
        Assert.Equal(string.Empty, result.Value[0].Representation);
        Assert.False(result.Value[0].IsOptional);
    }

    [Fact]
    public void ToDomainParts_WhenKindIsUnknown_ShouldReturnPathTemplatePartKindNotSupportedForLibraryType()
    {
        // Arrange
        LibraryPathTemplatePartDto part = _libraryPathTemplatePartDtoFixture.Create(kind: "NotARealLibraryPathPartKind");

        LibraryPathTemplatePartDto[] parts = [part];

        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = parts.ToDomainParts();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.PathTemplatePartKindNotSupportedForLibraryType, result.FirstError);
    }

    [Fact]
    public void ToDomainParts_WhenKindIsNull_ShouldReturnPathTemplatePartKindNotSupportedForLibraryType()
    {
        // Arrange
        LibraryPathTemplatePartDto part = _libraryPathTemplatePartDtoFixture.Create() with { Kind = null };

        LibraryPathTemplatePartDto[] parts = [part];

        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = parts.ToDomainParts();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.PathTemplatePartKindNotSupportedForLibraryType, result.FirstError);
    }

    [Fact]
    public void ToDomainParts_WhenKindUsesDifferentCasing_ShouldMapKind()
    {
        // Arrange
        LibraryPathTemplatePartDto part = _libraryPathTemplatePartDtoFixture.Create(
            kind: "artist",
            representation: "{0}",
            isOptional: false);

        LibraryPathTemplatePartDto[] parts = [part];

        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = parts.ToDomainParts();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal(LibraryPathPartKind.Artist, result.Value[0].Kind);
        Assert.Equal("{0}", result.Value[0].Representation);
        Assert.False(result.Value[0].IsOptional);
    }

    [Fact]
    public void ToDomainParts_WhenTypedPartRepresentationIsEmpty_ShouldReturnPathTemplateLiteralCannotBeEmpty()
    {
        // Arrange
        LibraryPathTemplatePartDto part = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Artist),
            representation: string.Empty);

        LibraryPathTemplatePartDto[] parts = [part];

        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = parts.ToDomainParts();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.PathTemplateLiteralCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainParts_WhenTypedPartRepresentationHasNoPlaceholder_ShouldReturnPathTemplateValuePartMustContainSinglePlaceholder()
    {
        // Arrange
        LibraryPathTemplatePartDto part = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Artist),
            representation: "Artist Name");

        LibraryPathTemplatePartDto[] parts = [part];

        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = parts.ToDomainParts();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.PathTemplateValuePartMustContainSinglePlaceholder, result.FirstError);
    }

    [Fact]
    public void ToDomainParts_WhenTypedPartRepresentationHasDuplicatePlaceholder_ShouldReturnPathTemplateValuePartMustContainSinglePlaceholder()
    {
        // Arrange
        LibraryPathTemplatePartDto part = _libraryPathTemplatePartDtoFixture.Create(
            kind: nameof(LibraryPathPartKind.Artist),
            representation: "{0} - {0}");

        LibraryPathTemplatePartDto[] parts = [part];

        // Act
        Result<IReadOnlyList<LibraryPathPart>> result = parts.ToDomainParts();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.PathTemplateValuePartMustContainSinglePlaceholder, result.FirstError);
    }
}
