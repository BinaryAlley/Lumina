#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MusicLibraryPathMetadata"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicLibraryPathMetadataTests
{
    private readonly MusicLibraryPathMetadataFixture _musicLibraryPathMetadataFixture = new();
    private readonly ParsedLibraryPathFixture _parsedLibraryPathFixture = new();

    [Fact]
    public void FromParsedLibraryPath_WhenAllValuesAreCaptured_ShouldMapAllFields()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "Queen",
            [LibraryPathPartKind.ReleaseType] = "Album",
            [LibraryPathPartKind.ReleaseYear] = "1975",
            [LibraryPathPartKind.ReleaseName] = "A Night at the Opera",
            [LibraryPathPartKind.TrackNumber] = "11",
            [LibraryPathPartKind.DiscNumber] = "1",
            [LibraryPathPartKind.TrackName] = "Bohemian Rhapsody"
        });

        // Act
        MusicLibraryPathMetadata result = MusicLibraryPathMetadata.FromParsedLibraryPath(parsedPath);

        // Assert
        Assert.Equal("Queen", result.ArtistName.Value);
        Assert.Equal(MusicReleaseType.Album, result.ReleaseType.Value);
        Assert.Equal(1975, result.ReleaseYear.Value);
        Assert.Equal("A Night at the Opera", result.ReleaseName.Value);
        Assert.Equal(11, result.TrackNumber.Value);
        Assert.Equal(1, result.DiscNumber.Value);
        Assert.Equal("Bohemian Rhapsody", result.TrackTitle.Value);
    }

    [Fact]
    public void FromParsedLibraryPath_WhenReleaseTypeIsRecognized_ShouldParseItCaseInsensitively()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.ReleaseType] = "album"
        });

        // Act
        MusicLibraryPathMetadata result = MusicLibraryPathMetadata.FromParsedLibraryPath(parsedPath);

        // Assert
        Assert.True(result.ReleaseType.HasValue);
        Assert.Equal(MusicReleaseType.Album, result.ReleaseType.Value);
    }

    [Fact]
    public void FromParsedLibraryPath_WhenReleaseTypeIsNotRecognized_ShouldFallBackToOther()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.ReleaseType] = "not a release type"
        });

        // Act
        MusicLibraryPathMetadata result = MusicLibraryPathMetadata.FromParsedLibraryPath(parsedPath);

        // Assert
        Assert.True(result.ReleaseType.HasValue);
        Assert.Equal(MusicReleaseType.Other, result.ReleaseType.Value);
    }

    [Fact]
    public void FromParsedLibraryPath_WhenReleaseTypeIsAbsent_ShouldLeaveItUnset()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(new Dictionary<LibraryPathPartKind, string>
        {
            [LibraryPathPartKind.Artist] = "Queen"
        });

        // Act
        MusicLibraryPathMetadata result = MusicLibraryPathMetadata.FromParsedLibraryPath(parsedPath);

        // Assert
        Assert.False(result.ReleaseType.HasValue);
    }

    [Fact]
    public void FromParsedLibraryPath_WhenValuesAreAbsent_ShouldLeaveThemUnset()
    {
        // Arrange
        ParsedLibraryPath parsedPath = _parsedLibraryPathFixture.Create(includeValues: false);

        // Act
        MusicLibraryPathMetadata result = MusicLibraryPathMetadata.FromParsedLibraryPath(parsedPath);

        // Assert
        Assert.False(result.ArtistName.HasValue);
        Assert.False(result.ReleaseType.HasValue);
        Assert.False(result.ReleaseYear.HasValue);
        Assert.False(result.ReleaseName.HasValue);
        Assert.False(result.TrackNumber.HasValue);
        Assert.False(result.DiscNumber.HasValue);
        Assert.False(result.TrackTitle.HasValue);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        MusicLibraryPathMetadata firstMetadata = _musicLibraryPathMetadataFixture.Create();

        // Act
        MusicLibraryPathMetadata secondMetadata = _musicLibraryPathMetadataFixture.Create(
            artistName: firstMetadata.ArtistName.Value,
            releaseType: firstMetadata.ReleaseType.Value,
            releaseYear: firstMetadata.ReleaseYear.Value,
            releaseName: firstMetadata.ReleaseName.Value,
            trackNumber: firstMetadata.TrackNumber.Value,
            discNumber: firstMetadata.DiscNumber.Value,
            trackTitle: firstMetadata.TrackTitle.Value);

        // Assert
        Assert.Equal(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentArtistName_ShouldReturnFalse()
    {
        // Arrange
        MusicLibraryPathMetadata firstMetadata = _musicLibraryPathMetadataFixture.Create(artistName: "Queen");

        // Act
        MusicLibraryPathMetadata secondMetadata = _musicLibraryPathMetadataFixture.Create(artistName: "Pink Floyd");

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentReleaseType_ShouldReturnFalse()
    {
        // Arrange
        MusicLibraryPathMetadata firstMetadata = _musicLibraryPathMetadataFixture.Create(releaseType: MusicReleaseType.Album);

        // Act
        MusicLibraryPathMetadata secondMetadata = _musicLibraryPathMetadataFixture.Create(releaseType: MusicReleaseType.Single);

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentReleaseYear_ShouldReturnFalse()
    {
        // Arrange
        MusicLibraryPathMetadata firstMetadata = _musicLibraryPathMetadataFixture.Create(releaseYear: 1975);

        // Act
        MusicLibraryPathMetadata secondMetadata = _musicLibraryPathMetadataFixture.Create(releaseYear: 1980);

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentReleaseName_ShouldReturnFalse()
    {
        // Arrange
        MusicLibraryPathMetadata firstMetadata = _musicLibraryPathMetadataFixture.Create(releaseName: "A Night at the Opera");

        // Act
        MusicLibraryPathMetadata secondMetadata = _musicLibraryPathMetadataFixture.Create(releaseName: "The Game");

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentTrackNumber_ShouldReturnFalse()
    {
        // Arrange
        MusicLibraryPathMetadata firstMetadata = _musicLibraryPathMetadataFixture.Create(trackNumber: 11);

        // Act
        MusicLibraryPathMetadata secondMetadata = _musicLibraryPathMetadataFixture.Create(trackNumber: 12);

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentDiscNumber_ShouldReturnFalse()
    {
        // Arrange
        MusicLibraryPathMetadata firstMetadata = _musicLibraryPathMetadataFixture.Create(discNumber: 1);

        // Act
        MusicLibraryPathMetadata secondMetadata = _musicLibraryPathMetadataFixture.Create(discNumber: 2);

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }

    [Fact]
    public void Equals_WithDifferentTrackTitle_ShouldReturnFalse()
    {
        // Arrange
        MusicLibraryPathMetadata firstMetadata = _musicLibraryPathMetadataFixture.Create(trackTitle: "Bohemian Rhapsody");

        // Act
        MusicLibraryPathMetadata secondMetadata = _musicLibraryPathMetadataFixture.Create(trackTitle: "Love of My Life");

        // Assert
        Assert.NotEqual(firstMetadata, secondMetadata);
    }
}
