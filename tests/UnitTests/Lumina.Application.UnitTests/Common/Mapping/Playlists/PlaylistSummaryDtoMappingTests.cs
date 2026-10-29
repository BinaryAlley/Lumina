#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Fixtures.Common.DTO.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistSummaryDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistSummaryDtoMappingTests
{
    private readonly PlaylistSummaryDtoFixture _playlistSummaryDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingTheSummary_ShouldMapEveryField()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        DateTime createdOnUtc = new(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc);
        DateTime updatedOnUtc = new(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc);
        PlaylistSummaryDto dto = _playlistSummaryDtoFixture.Create(
            id: id,
            name: "Commute",
            description: "Train rides",
            type: PlaylistType.Music,
            entryCount: 37,
            createdOnUtc: createdOnUtc,
            updatedOnUtc: updatedOnUtc,
            includeUpdatedOnUtc: true);

        // Act
        PlaylistSummaryResponse response = dto.ToResponse();

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal("Commute", response.Name);
        Assert.Equal("Train rides", response.Description);
        Assert.Equal(PlaylistType.Music, response.Type);
        Assert.Equal(37, response.EntryCount);
        Assert.Equal(createdOnUtc, response.CreatedOnUtc);
        Assert.True(response.UpdatedOnUtc.HasValue);
        Assert.Equal(updatedOnUtc, response.UpdatedOnUtc.Value);
    }

    [Fact]
    public void ToResponse_WhenTheOptionalMembersAreMissing_ShouldMapNulls()
    {
        // Arrange
        PlaylistSummaryDto dto = _playlistSummaryDtoFixture.Create(
            includeDescription: false,
            includeUpdatedOnUtc: false);

        // Act
        PlaylistSummaryResponse response = dto.ToResponse();

        // Assert
        Assert.Null(response.Description);
        Assert.Null(response.UpdatedOnUtc);
    }
}
