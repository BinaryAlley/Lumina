#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="LibraryPathPartDefinition"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryPathPartDefinitionTests
{
    private readonly LibraryPathPartDefinitionFixture _libraryPathPartDefinitionFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Act
        LibraryPathPartDefinition definition = new(LibraryPathPartKind.TrackNumber, LibraryPathValueType.Integer, "{0:00}", isOptionalByDefault: true);

        // Assert
        Assert.Equal(LibraryPathPartKind.TrackNumber, definition.Kind);
        Assert.Equal(LibraryPathValueType.Integer, definition.ValueType);
        Assert.Equal("{0:00}", definition.DefaultRepresentation);
        Assert.True(definition.IsOptionalByDefault);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LibraryPathPartDefinition firstDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.Artist,
            valueType: LibraryPathValueType.Text,
            defaultRepresentation: "{0}",
            isOptionalByDefault: false);

        // Act
        LibraryPathPartDefinition secondDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.Artist,
            valueType: LibraryPathValueType.Text,
            defaultRepresentation: "{0}",
            isOptionalByDefault: false);

        // Assert
        Assert.Equal(firstDefinition, secondDefinition);
    }

    [Fact]
    public void Equals_WithDifferentKind_ShouldReturnFalse()
    {
        // Arrange
        LibraryPathPartDefinition firstDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.Artist,
            valueType: LibraryPathValueType.Text,
            defaultRepresentation: "{0}");

        // Act
        LibraryPathPartDefinition secondDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.ReleaseName,
            valueType: LibraryPathValueType.Text,
            defaultRepresentation: "{0}");

        // Assert
        Assert.NotEqual(firstDefinition, secondDefinition);
    }

    [Fact]
    public void Equals_WithDifferentValueType_ShouldReturnFalse()
    {
        // Arrange
        LibraryPathPartDefinition firstDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.TrackNumber,
            valueType: LibraryPathValueType.Integer,
            defaultRepresentation: "{0}");

        // Act
        LibraryPathPartDefinition secondDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.TrackNumber,
            valueType: LibraryPathValueType.Text,
            defaultRepresentation: "{0}");

        // Assert
        Assert.NotEqual(firstDefinition, secondDefinition);
    }

    [Fact]
    public void Equals_WithDifferentDefaultRepresentation_ShouldReturnFalse()
    {
        // Arrange
        LibraryPathPartDefinition firstDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.ReleaseYear,
            valueType: LibraryPathValueType.Year,
            defaultRepresentation: "{0}");

        // Act
        LibraryPathPartDefinition secondDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.ReleaseYear,
            valueType: LibraryPathValueType.Year,
            defaultRepresentation: "Year {0}");

        // Assert
        Assert.NotEqual(firstDefinition, secondDefinition);
    }

    [Fact]
    public void Equals_WithDifferentIsOptionalByDefault_ShouldReturnFalse()
    {
        // Arrange
        LibraryPathPartDefinition firstDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.DiscNumber,
            valueType: LibraryPathValueType.Integer,
            defaultRepresentation: "{0:00}",
            isOptionalByDefault: false);

        // Act
        LibraryPathPartDefinition secondDefinition = _libraryPathPartDefinitionFixture.Create(
            kind: LibraryPathPartKind.DiscNumber,
            valueType: LibraryPathValueType.Integer,
            defaultRepresentation: "{0:00}",
            isOptionalByDefault: true);

        // Assert
        Assert.NotEqual(firstDefinition, secondDefinition);
    }
}
