#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="LibraryPathPart"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathPartTests
{
    private readonly LibraryPathPartFixture _libraryPathPartFixture = new();

    [Fact]
    public void Create_WhenKindIsSeparator_ShouldCreateSeparatorWithoutRepresentation()
    {
        // Act
        Result<LibraryPathPart> result = LibraryPathPart.Create(LibraryPathPartKind.Separator, "/", isOptional: false);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(LibraryPathPartKind.Separator, result.Value.Kind);
        Assert.Equal(string.Empty, result.Value.Representation);
    }

    [Fact]
    public void Create_WhenKindIsLiteralWithText_ShouldCreateLiteralWithThatRepresentation()
    {
        // Act
        Result<LibraryPathPart> result = LibraryPathPart.Create(LibraryPathPartKind.Literal, " - ", isOptional: false);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(LibraryPathPartKind.Literal, result.Value.Kind);
        Assert.Equal(" - ", result.Value.Representation);
    }

    [Theory]
    [InlineData(null)] // the representation is absent
    [InlineData("")] // the representation is empty
    [InlineData("   ")] // the representation is whitespace
    public void Create_WhenKindIsNotSeparatorAndRepresentationIsEmpty_ShouldReturnLiteralCannotBeEmptyError(string? representation)
    {
        // Act
        Result<LibraryPathPart> result = LibraryPathPart.Create(LibraryPathPartKind.Literal, representation, isOptional: false);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Lumina.Domain.Common.Errors.Errors.Library.PathTemplateLiteralCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Create_WhenTypedPartContainsSinglePlaceholder_ShouldCreateThePart()
    {
        // Act
        Result<LibraryPathPart> result = LibraryPathPart.Create(LibraryPathPartKind.Artist, "{0}", isOptional: false);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("{0}", result.Value.Representation);
    }

    [Fact]
    public void Create_WhenTypedPartContainsFixedWidthPlaceholder_ShouldCreateThePart()
    {
        // Act
        Result<LibraryPathPart> result = LibraryPathPart.Create(LibraryPathPartKind.TrackNumber, "Disc {0:00}", isOptional: true);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Disc {0:00}", result.Value.Representation);
        Assert.True(result.Value.IsOptional);
    }

    [Theory]
    [InlineData("no placeholder")] // the mask carries no value placeholder
    [InlineData("{0} {0}")] // the mask carries more than one value placeholder
    public void Create_WhenTypedPartDoesNotContainExactlyOnePlaceholder_ShouldReturnError(string representation)
    {
        // Act
        Result<LibraryPathPart> result = LibraryPathPart.Create(LibraryPathPartKind.Artist, representation, isOptional: false);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Lumina.Domain.Common.Errors.Errors.Library.PathTemplateValuePartMustContainSinglePlaceholder, result.FirstError);
    }

    [Fact]
    public void Create_WhenLiteralContainsNoPlaceholder_ShouldCreateThePart()
    {
        // Act
        Result<LibraryPathPart> result = LibraryPathPart.Create(LibraryPathPartKind.Literal, "Disk ", isOptional: false);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Disk ", result.Value.Representation);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LibraryPathPart firstPart = _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: true);

        // Act
        LibraryPathPart secondPart = _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: true);

        // Assert
        Assert.Equal(firstPart, secondPart);
    }

    [Fact]
    public void Equals_WithDifferentKind_ShouldReturnFalse()
    {
        // Arrange
        LibraryPathPart firstPart = _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);

        // Act
        LibraryPathPart secondPart = _libraryPathPartFixture.Create(kind: LibraryPathPartKind.ReleaseName, representation: "{0}", isOptional: false);

        // Assert
        Assert.NotEqual(firstPart, secondPart);
    }

    [Fact]
    public void Equals_WithDifferentRepresentation_ShouldReturnFalse()
    {
        // Arrange
        LibraryPathPart firstPart = _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);

        // Act
        LibraryPathPart secondPart = _libraryPathPartFixture.Create(kind: LibraryPathPartKind.Artist, representation: "Artist: {0}", isOptional: false);

        // Assert
        Assert.NotEqual(firstPart, secondPart);
    }

    [Fact]
    public void Equals_WithDifferentIsOptional_ShouldReturnFalse()
    {
        // Arrange
        LibraryPathPart firstPart = _libraryPathPartFixture.Create(kind: LibraryPathPartKind.DiscNumber, representation: "{0:00}", isOptional: false);

        // Act
        LibraryPathPart secondPart = _libraryPathPartFixture.Create(kind: LibraryPathPartKind.DiscNumber, representation: "{0:00}", isOptional: true);

        // Assert
        Assert.NotEqual(firstPart, secondPart);
    }
}
