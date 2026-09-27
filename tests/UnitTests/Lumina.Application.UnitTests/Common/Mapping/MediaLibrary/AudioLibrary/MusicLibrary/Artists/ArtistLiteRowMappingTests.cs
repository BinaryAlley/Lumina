#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="ArtistLiteRowMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistLiteRowMappingTests
{
    private readonly ArtistLiteRowFixture _artistLiteRowFixture = new();
    private readonly PaginatedResultDtoFixture<ArtistLiteRow> _paginatedResultDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingArtistLiteRow_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        ArtistLiteRow readModel = _artistLiteRowFixture.Create();

        // Act
        ArtistLiteResponse result = readModel.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(readModel.Id, result.Id);
        Assert.Equal(readModel.Name, result.Name);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleArtistLiteRows_ShouldMapAllCorrectly()
    {
        // Arrange
        List<ArtistLiteRow> readModels = _artistLiteRowFixture.CreateMany(2);

        // Act
        IReadOnlyList<ArtistLiteResponse> results = readModels.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(readModels.Count, results.Count);
        Assert.Equal(readModels.Select(readModel => readModel.Id), results.Select(response => response.Id));
        Assert.Equal(readModels.Select(readModel => readModel.Name), results.Select(response => response.Name));
    }

    [Fact]
    public void ToResponses_WhenMappingPaginatedArtistLiteRows_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        List<ArtistLiteRow> readModels = _artistLiteRowFixture.CreateMany(2);
        PaginatedResultDto<ArtistLiteRow> paginatedReadModels = _paginatedResultDtoFixture.Create(
            data: readModels,
            currentPage: 1,
            perPage: 10,
            count: 2,
            numberOfPages: 1);

        // Act
        PaginatedResponse<ArtistLiteResponse> result = paginatedReadModels.ToResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paginatedReadModels.Data.Count, result.Data.Count);
        Assert.Equal(paginatedReadModels.CurrentPage, result.CurrentPage);
        Assert.Equal(paginatedReadModels.PerPage, result.PerPage);
        Assert.Equal(paginatedReadModels.Count, result.Count);
        Assert.Equal(paginatedReadModels.NumberOfPages, result.NumberOfPages);
    }
}
