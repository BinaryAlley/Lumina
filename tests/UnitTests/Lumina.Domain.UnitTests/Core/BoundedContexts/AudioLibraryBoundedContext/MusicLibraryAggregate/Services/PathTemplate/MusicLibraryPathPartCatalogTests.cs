#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.PathTemplate;

/// <summary>
/// Contains unit tests for the <see cref="MusicLibraryPathPartCatalog"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicLibraryPathPartCatalogTests
{
    private readonly MusicLibraryPathPartCatalog _sut = new();

    [Fact]
    public void SupportedLibraryType_WhenCalled_ShouldReturnMusic()
    {
        // Act
        LibraryType result = _sut.SupportedLibraryType;

        // Assert
        Assert.Equal(LibraryType.Music, result);
    }

    [Fact]
    public void GetPartDefinitions_WhenCalled_ShouldReturnTheExpectedDefinitions()
    {
        // Act
        IReadOnlyList<LibraryPathPartDefinition> result = _sut.GetPartDefinitions();

        // Assert
        Assert.Equal(10, result.Count);

        Assert.Equal(LibraryPathPartKind.Literal, result[0].Kind);
        Assert.Equal(LibraryPathValueType.None, result[0].ValueType);
        Assert.Equal(string.Empty, result[0].DefaultRepresentation);
        Assert.False(result[0].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.Separator, result[1].Kind);
        Assert.Equal(LibraryPathValueType.None, result[1].ValueType);
        Assert.Equal(string.Empty, result[1].DefaultRepresentation);
        Assert.False(result[1].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.Artist, result[2].Kind);
        Assert.Equal(LibraryPathValueType.Text, result[2].ValueType);
        Assert.Equal("{0}", result[2].DefaultRepresentation);
        Assert.False(result[2].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.ReleaseType, result[3].Kind);
        Assert.Equal(LibraryPathValueType.Enum, result[3].ValueType);
        Assert.Equal("{0}", result[3].DefaultRepresentation);
        Assert.False(result[3].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.ReleaseYear, result[4].Kind);
        Assert.Equal(LibraryPathValueType.Year, result[4].ValueType);
        Assert.Equal("{0}", result[4].DefaultRepresentation);
        Assert.False(result[4].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.ReleaseName, result[5].Kind);
        Assert.Equal(LibraryPathValueType.Text, result[5].ValueType);
        Assert.Equal("{0}", result[5].DefaultRepresentation);
        Assert.False(result[5].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.TrackNumber, result[6].Kind);
        Assert.Equal(LibraryPathValueType.Integer, result[6].ValueType);
        Assert.Equal("{0:00}", result[6].DefaultRepresentation);
        Assert.False(result[6].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.TrackName, result[7].Kind);
        Assert.Equal(LibraryPathValueType.Text, result[7].ValueType);
        Assert.Equal("{0}", result[7].DefaultRepresentation);
        Assert.False(result[7].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.Extension, result[8].Kind);
        Assert.Equal(LibraryPathValueType.Text, result[8].ValueType);
        Assert.Equal("{0}", result[8].DefaultRepresentation);
        Assert.False(result[8].IsOptionalByDefault);

        Assert.Equal(LibraryPathPartKind.DiscNumber, result[9].Kind);
        Assert.Equal(LibraryPathValueType.Integer, result[9].ValueType);
        Assert.Equal("{0:00}", result[9].DefaultRepresentation);
        Assert.True(result[9].IsOptionalByDefault);
    }

    [Fact]
    public void GetDefaultTemplate_WhenCalled_ShouldReturnTheExpectedParts()
    {
        // Act
        IReadOnlyList<LibraryPathPart> result = _sut.GetDefaultTemplate().Parts;

        // Assert
        Assert.Equal(16, result.Count);

        Assert.Equal(LibraryPathPartKind.Artist, result[0].Kind);
        Assert.Equal("{0}", result[0].Representation);
        Assert.False(result[0].IsOptional);

        Assert.Equal(LibraryPathPartKind.Separator, result[1].Kind);
        Assert.Equal(string.Empty, result[1].Representation);
        Assert.False(result[1].IsOptional);

        Assert.Equal(LibraryPathPartKind.ReleaseType, result[2].Kind);
        Assert.Equal("{0}", result[2].Representation);
        Assert.False(result[2].IsOptional);

        Assert.Equal(LibraryPathPartKind.Separator, result[3].Kind);
        Assert.Equal(string.Empty, result[3].Representation);
        Assert.False(result[3].IsOptional);

        Assert.Equal(LibraryPathPartKind.ReleaseYear, result[4].Kind);
        Assert.Equal("{0}", result[4].Representation);
        Assert.False(result[4].IsOptional);

        Assert.Equal(LibraryPathPartKind.Literal, result[5].Kind);
        Assert.Equal(" - ", result[5].Representation);
        Assert.False(result[5].IsOptional);

        Assert.Equal(LibraryPathPartKind.ReleaseName, result[6].Kind);
        Assert.Equal("{0}", result[6].Representation);
        Assert.False(result[6].IsOptional);

        Assert.Equal(LibraryPathPartKind.Separator, result[7].Kind);
        Assert.Equal(string.Empty, result[7].Representation);
        Assert.True(result[7].IsOptional);

        Assert.Equal(LibraryPathPartKind.Literal, result[8].Kind);
        Assert.Equal("Disk ", result[8].Representation);
        Assert.True(result[8].IsOptional);

        Assert.Equal(LibraryPathPartKind.DiscNumber, result[9].Kind);
        Assert.Equal("{0:00}", result[9].Representation);
        Assert.True(result[9].IsOptional);

        Assert.Equal(LibraryPathPartKind.Separator, result[10].Kind);
        Assert.Equal(string.Empty, result[10].Representation);
        Assert.False(result[10].IsOptional);

        Assert.Equal(LibraryPathPartKind.TrackNumber, result[11].Kind);
        Assert.Equal("{0:00}", result[11].Representation);
        Assert.False(result[11].IsOptional);

        Assert.Equal(LibraryPathPartKind.Literal, result[12].Kind);
        Assert.Equal(" - ", result[12].Representation);
        Assert.False(result[12].IsOptional);

        Assert.Equal(LibraryPathPartKind.TrackName, result[13].Kind);
        Assert.Equal("{0}", result[13].Representation);
        Assert.False(result[13].IsOptional);

        Assert.Equal(LibraryPathPartKind.Literal, result[14].Kind);
        Assert.Equal(".", result[14].Representation);
        Assert.False(result[14].IsOptional);

        Assert.Equal(LibraryPathPartKind.Extension, result[15].Kind);
        Assert.Equal("{0}", result[15].Representation);
        Assert.False(result[15].IsOptional);
    }
}
