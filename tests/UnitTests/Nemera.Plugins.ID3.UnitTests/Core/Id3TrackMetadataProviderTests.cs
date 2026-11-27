#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.ID3.Core;
using Lumina.Plugins.ID3.Core.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.ID3.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="Id3TrackMetadataProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class Id3TrackMetadataProviderTests
{
    private readonly TrackMetadataLookupDtoFixture _trackMetadataLookupDtoFixture = new();
    private readonly Id3TrackMetadataProvider _sut = new(new Id3TagReader());

    [Fact]
    public void Name_WhenCalled_ShouldReturnTheProviderDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("ID3", result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenCalled_ShouldReturnMusic()
    {
        // Act
        IReadOnlyList<LibraryType> result = _sut.SupportedLibraryTypes;

        // Assert
        Assert.Equal([LibraryType.Music], result);
    }

    [Fact]
    public void RequiresWebAccess_WhenCalled_ShouldReturnFalse()
    {
        // Act
        bool result = _sut.RequiresWebAccess;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenTheFileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.wav");
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: path, title: "The Track");

        // Act
        AudioMetadataDto? result = await _sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenTheFileDoesNotExist_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.wav");
        TrackMetadataLookupDto lookup = _trackMetadataLookupDtoFixture.Create(path: path, title: "The Track");

        // Act
        IReadOnlyList<AudioMetadataDto> result = await _sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
