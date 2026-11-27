#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Application.Fixtures.Common.DTO.Pagination;
using Nemera.Contracts.Responses.Common;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="TrackLiteRowMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackLiteRowMappingTests
{
    private readonly TrackLiteRowFixture _trackLiteRowFixture = new();
    private readonly PaginatedResultDtoFixture<TrackLiteRow> _paginatedResultDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingTrackLiteRow_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        TrackLiteRow readModel = _trackLiteRowFixture.Create();

        // Act
        TrackLiteResponse result = readModel.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(readModel.Id, result.Id);
        Assert.Equal(readModel.Title, result.Title);
        Assert.Equal(readModel.TrackNumber, result.TrackNumber);
        Assert.Equal(readModel.DiscNumber, result.DiscNumber);
        Assert.Equal(readModel.DurationInSeconds, result.DurationInSeconds);
    }

    [Fact]
    public void ToResponse_WhenDiscNumberIsProvided_ShouldMapIt()
    {
        // Arrange
        TrackLiteRow readModel = _trackLiteRowFixture.Create(discNumber: 2);

        // Act
        TrackLiteResponse result = readModel.ToResponse();

        // Assert
        Assert.Equal(2, result.DiscNumber);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleTrackLiteRows_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TrackLiteRow> readModels = _trackLiteRowFixture.CreateMany(2);

        // Act
        IReadOnlyList<TrackLiteResponse> results = readModels.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(readModels.Count, results.Count);
        Assert.Equal(readModels.Select(readModel => readModel.Id), results.Select(response => response.Id));
        Assert.Equal(readModels.Select(readModel => readModel.TrackNumber), results.Select(response => response.TrackNumber));
    }

    [Fact]
    public void ToResponses_WhenMappingPaginatedTrackLiteRows_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        List<TrackLiteRow> readModels = _trackLiteRowFixture.CreateMany(2);
        PaginatedResultDto<TrackLiteRow> paginatedReadModels = _paginatedResultDtoFixture.Create(
            data: readModels,
            currentPage: 1,
            perPage: 10,
            count: 2,
            numberOfPages: 1);

        // Act
        PaginatedResponse<TrackLiteResponse> result = paginatedReadModels.ToResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paginatedReadModels.Data.Count, result.Data.Count);
        Assert.Equal(paginatedReadModels.CurrentPage, result.CurrentPage);
        Assert.Equal(paginatedReadModels.PerPage, result.PerPage);
        Assert.Equal(paginatedReadModels.Count, result.Count);
        Assert.Equal(paginatedReadModels.NumberOfPages, result.NumberOfPages);
    }
}
