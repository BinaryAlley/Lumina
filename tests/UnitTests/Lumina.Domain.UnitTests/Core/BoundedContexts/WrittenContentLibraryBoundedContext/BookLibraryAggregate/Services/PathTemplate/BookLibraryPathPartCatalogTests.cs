#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.PathTemplate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.PathTemplate;

/// <summary>
/// Contains unit tests for the <see cref="BookLibraryPathPartCatalog"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLibraryPathPartCatalogTests
{
    private readonly BookLibraryPathPartCatalog _sut = new();

    [Fact]
    public void SupportedLibraryType_WhenCalled_ShouldReturnBook()
    {
        // Act
        LibraryType result = _sut.SupportedLibraryType;

        // Assert
        Assert.Equal(LibraryType.Book, result);
    }

    [Fact]
    public void GetPartDefinitions_WhenCalled_ShouldReturnTheExpectedDefinitions()
    {
        // Act
        IReadOnlyList<LibraryPathPartDefinition> result = _sut.GetPartDefinitions();

        // Assert
        Assert.Equal(8, result.Count);

        Assert.Equal(LibraryPathPartKind.Literal, result[0].Kind);
        Assert.Equal(LibraryPathValueType.None, result[0].ValueType);
        Assert.Equal(string.Empty, result[0].DefaultRepresentation);
        Assert.False(result[0].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.Separator, result[1].Kind);
        Assert.Equal(LibraryPathValueType.None, result[1].ValueType);
        Assert.Equal(string.Empty, result[1].DefaultRepresentation);
        Assert.False(result[1].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.Author, result[2].Kind);
        Assert.Equal(LibraryPathValueType.Text, result[2].ValueType);
        Assert.Equal("{0}", result[2].DefaultRepresentation);
        Assert.False(result[2].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.Title, result[3].Kind);
        Assert.Equal(LibraryPathValueType.Text, result[3].ValueType);
        Assert.Equal("{0}", result[3].DefaultRepresentation);
        Assert.False(result[3].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.Series, result[4].Kind);
        Assert.Equal(LibraryPathValueType.Text, result[4].ValueType);
        Assert.Equal("{0}", result[4].DefaultRepresentation);
        Assert.True(result[4].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.SeriesNumber, result[5].Kind);
        Assert.Equal(LibraryPathValueType.Integer, result[5].ValueType);
        Assert.Equal("{0}", result[5].DefaultRepresentation);
        Assert.True(result[5].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.BookId, result[6].Kind);
        Assert.Equal(LibraryPathValueType.Integer, result[6].ValueType);
        Assert.Equal("{0}", result[6].DefaultRepresentation);
        Assert.False(result[6].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.Extension, result[7].Kind);
        Assert.Equal(LibraryPathValueType.Text, result[7].ValueType);
        Assert.Equal("{0}", result[7].DefaultRepresentation);
        Assert.False(result[7].IsOptionalByDefault);
    }

    [Fact]
    public void GetDefaultTemplate_WhenCalled_ShouldReturnTheExpectedParts()
    {
        // Act
        IReadOnlyList<LibraryPathPart> result = _sut.GetDefaultTemplate().Parts;

        // Assert
        Assert.Equal(12, result.Count);

        Assert.Equal(LibraryPathPartKind.Author, result[0].Kind);
        Assert.Equal("{0}", result[0].Representation);
        Assert.False(result[0].IsOptional);

        Assert.Equal(LibraryPathPartKind.Separator, result[1].Kind);
        Assert.Equal(string.Empty, result[1].Representation);
        Assert.False(result[1].IsOptional);

        Assert.Equal(LibraryPathPartKind.Title, result[2].Kind);
        Assert.Equal("{0}", result[2].Representation);
        Assert.False(result[2].IsOptional);

        Assert.Equal(LibraryPathPartKind.Literal, result[3].Kind);
        Assert.Equal(" (", result[3].Representation);
        Assert.False(result[3].IsOptional);

        Assert.Equal(LibraryPathPartKind.BookId, result[4].Kind);
        Assert.Equal("{0}", result[4].Representation);
        Assert.False(result[4].IsOptional);

        Assert.Equal(LibraryPathPartKind.Literal, result[5].Kind);
        Assert.Equal(")", result[5].Representation);
        Assert.False(result[5].IsOptional);

        Assert.Equal(LibraryPathPartKind.Separator, result[6].Kind);
        Assert.Equal(string.Empty, result[6].Representation);
        Assert.False(result[6].IsOptional);

        Assert.Equal(LibraryPathPartKind.Title, result[7].Kind);
        Assert.Equal("{0}", result[7].Representation);
        Assert.False(result[7].IsOptional);

        Assert.Equal(LibraryPathPartKind.Literal, result[8].Kind);
        Assert.Equal(" - ", result[8].Representation);
        Assert.False(result[8].IsOptional);

        Assert.Equal(LibraryPathPartKind.Author, result[9].Kind);
        Assert.Equal("{0}", result[9].Representation);
        Assert.False(result[9].IsOptional);

        Assert.Equal(LibraryPathPartKind.Literal, result[10].Kind);
        Assert.Equal(".", result[10].Representation);
        Assert.False(result[10].IsOptional);

        Assert.Equal(LibraryPathPartKind.Extension, result[11].Kind);
        Assert.Equal("{0}", result[11].Representation);
        Assert.False(result[11].IsOptional);
    }
}
