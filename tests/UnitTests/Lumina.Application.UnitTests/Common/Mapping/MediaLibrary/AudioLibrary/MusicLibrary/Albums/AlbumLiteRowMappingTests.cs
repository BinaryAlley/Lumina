#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="AlbumLiteRowMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumLiteRowMappingTests
{
    private readonly AlbumLiteRowFixture _albumLiteRowFixture = new();
    private readonly PaginatedResultDtoFixture<AlbumLiteRow> _paginatedResultDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingAlbumLiteRow_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AlbumLiteRow readModel = _albumLiteRowFixture.Create();

        // Act
        AlbumLiteResponse result = readModel.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(readModel.Id, result.Id);
        Assert.Equal(readModel.Title, result.Title);
        Assert.Equal(readModel.TotalTracks, result.TotalTracks);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleAlbumLiteRows_ShouldMapAllCorrectly()
    {
        // Arrange
        List<AlbumLiteRow> readModels = _albumLiteRowFixture.CreateMany(2);

        // Act
        IReadOnlyList<AlbumLiteResponse> results = readModels.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(readModels.Count, results.Count);
        Assert.Equal(readModels.Select(readModel => readModel.Id), results.Select(response => response.Id));
        Assert.Equal(readModels.Select(readModel => readModel.Title), results.Select(response => response.Title));
        Assert.Equal(readModels.Select(readModel => readModel.TotalTracks), results.Select(response => response.TotalTracks));
    }

    [Fact]
    public void ToResponses_WhenMappingPaginatedAlbumLiteRows_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        List<AlbumLiteRow> readModels = _albumLiteRowFixture.CreateMany(2);
        PaginatedResultDto<AlbumLiteRow> paginatedReadModels = _paginatedResultDtoFixture.Create(
            data: readModels,
            currentPage: 1,
            perPage: 10,
            count: 2,
            numberOfPages: 1);

        // Act
        PaginatedResponse<AlbumLiteResponse> result = paginatedReadModels.ToResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paginatedReadModels.Data.Count, result.Data.Count);
        Assert.Equal(paginatedReadModels.CurrentPage, result.CurrentPage);
        Assert.Equal(paginatedReadModels.PerPage, result.PerPage);
        Assert.Equal(paginatedReadModels.Count, result.Count);
        Assert.Equal(paginatedReadModels.NumberOfPages, result.NumberOfPages);
    }
}
