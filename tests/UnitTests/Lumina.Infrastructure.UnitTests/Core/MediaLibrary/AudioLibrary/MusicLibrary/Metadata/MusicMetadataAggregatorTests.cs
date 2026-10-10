#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Metadata;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="MusicMetadataAggregator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMetadataAggregatorTests
{
    private readonly ArtistMetadataDtoFixture _artistMetadataDtoFixture = new();
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly AudioMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();

    [Fact]
    public void Merge_WhenMergingArtistMetadata_ShouldGivePriorityToFirstAndFillMissingValuesFromSecond()
    {
        // Arrange
        MediaContributorDto firstContributor = _mediaContributorDtoFixture.Create();
        MediaContributorDto secondContributor = _mediaContributorDtoFixture.Create();
        ArtistMetadataDto first = _artistMetadataDtoFixture.Create(name: "First", includeCountry: false, contributors: [firstContributor], genres: []);
        ArtistMetadataDto second = _artistMetadataDtoFixture.Create(name: "Second", country: "CA", contributors: [secondContributor], genres: []);

        // Act
        ArtistMetadataDto merged = MusicMetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal("First", merged.Name);
        Assert.Equal("CA", merged.Country);
        Assert.Equal(2, merged.Contributors!.Count);
        Assert.Contains(merged.Contributors, contributor => contributor == firstContributor);
        Assert.Contains(merged.Contributors, contributor => contributor == secondContributor);
    }

    [Fact]
    public void Merge_WhenFirstArtistHasTheValue_ShouldNotTakeTheSecondValue()
    {
        // Arrange
        ArtistMetadataDto first = _artistMetadataDtoFixture.Create(name: "First", country: "US", genres: [], contributors: []);
        ArtistMetadataDto second = _artistMetadataDtoFixture.Create(name: "Second", country: "CA", genres: [], contributors: []);

        // Act
        ArtistMetadataDto merged = MusicMetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal("First", merged.Name);
        Assert.Equal("US", merged.Country);
    }

    [Fact]
    public void Merge_WhenMergingAlbumMetadata_ShouldGivePriorityToFirstAndFillMissingValuesFromSecond()
    {
        // Arrange
        AlbumMetadataDto first = _albumMetadataDtoFixture.Create(title: "First", includeBarcode: false, includeReleaseInfo: false);
        AlbumMetadataDto second = _albumMetadataDtoFixture.Create(title: "Second", barcode: "1234567890123");

        // Act
        AlbumMetadataDto merged = MusicMetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal("First", merged.Title);
        Assert.Equal("1234567890123", merged.Barcode);
        Assert.NotNull(merged.ReleaseInfo);
    }

    [Fact]
    public void Merge_WhenMergingAudioMetadata_ShouldGivePriorityToFirstAndFillMissingValuesFromSecond()
    {
        // Arrange
        AudioMetadataDto first = _audioMetadataDtoFixture.Create(title: "First", includeDurationInSeconds: false);
        AudioMetadataDto second = _audioMetadataDtoFixture.Create(title: "Second", durationInSeconds: 360);

        // Act
        AudioMetadataDto merged = MusicMetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal("First", merged.Title);
        Assert.Equal(360, merged.DurationInSeconds);
    }

    [Fact]
    public void Merge_WhenTheFirstMetadataHasOnlyAReleaseYearAndTheSecondHasAReleaseDate_ShouldDeriveTheYearFromTheDate()
    {
        // Arrange
        // The tags of a compilation provide a release year without a full date, while MusicBrainz provides the original release date of the recording.
        ReleaseInfoDto firstReleaseInfo = _releaseInfoDtoFixture.Create(originalReleaseYear: 1981);
        ReleaseInfoDto secondReleaseInfo = _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(1975, 10, 31), originalReleaseYear: 1975);
        AudioMetadataDto first = _audioMetadataDtoFixture.Create(releaseInfo: firstReleaseInfo);
        AudioMetadataDto second = _audioMetadataDtoFixture.Create(releaseInfo: secondReleaseInfo);

        // Act
        AudioMetadataDto merged = MusicMetadataAggregator.Merge(first, second);

        // Assert
        // The merged date and year must describe the same point in time, otherwise the release information would be rejected when applied.
        Assert.NotNull(merged.ReleaseInfo);
        Assert.Equal(new DateOnly(1975, 10, 31), merged.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(1975, merged.ReleaseInfo.OriginalReleaseYear);
    }

    [Fact]
    public void Merge_WhenBothAudioMetadataHaveIsrcs_ShouldUnionThemWithoutDuplicates()
    {
        // Arrange
        AudioMetadataDto first = _audioMetadataDtoFixture.Create();
        AudioMetadataDto second = _audioMetadataDtoFixture.Create();
        AudioMetadataDto mergedInput = first with { Isrcs = [.. first.Isrcs!.Concat(second.Isrcs!)] };

        // Act
        AudioMetadataDto merged = MusicMetadataAggregator.Merge(mergedInput, second);

        // Assert
        Assert.Equal(mergedInput.Isrcs!.Select(isrc => isrc.Value).Distinct(StringComparer.OrdinalIgnoreCase).Count(), merged.Isrcs!.Count);
    }
}
