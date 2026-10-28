#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionStatistics;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetConsumptionStatisticsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionStatisticsRequestMappingTests
{
    private readonly GetConsumptionStatisticsRequestFixture _getConsumptionStatisticsRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionStatisticsRequestMappingTests"/> class.
    /// </summary>
    public GetConsumptionStatisticsRequestMappingTests()
    {
        _getConsumptionStatisticsRequestFixture = new GetConsumptionStatisticsRequestFixture();
    }

    [Fact]
    public void ToQuery_WhenMappingPopulatedRequest_ShouldMapEveryRequestField()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        string libraryId = Guid.NewGuid().ToString();
        string fromUtc = "2025-01-02T03:04:05Z";
        string toUtc = "2025-02-03T04:05:06Z";
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.MusicTrack),
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Act
        GetConsumptionStatisticsQuery result = request.ToQuery(targetUserId.ToString());

        // Assert
        Assert.Equal(targetUserId.ToString(), result.TargetUserId);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.NotNull(result.MediaKind);
        Assert.Equal(nameof(MediaKind.MusicTrack), result.MediaKind);
        Assert.NotNull(result.FromUtc);
        Assert.Equal(fromUtc, result.FromUtc);
        Assert.NotNull(result.ToUtc);
        Assert.Equal(toUtc, result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenTargetUserIdIsNull_ShouldMapNullTargetUserId()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create();

        // Act
        GetConsumptionStatisticsQuery result = request.ToQuery(null);

        // Assert
        Assert.Null(result.TargetUserId);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(includeLibraryId: false);

        // Act
        GetConsumptionStatisticsQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(includeMediaKind: false);

        // Act
        GetConsumptionStatisticsQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.MediaKind);
    }

    [Fact]
    public void ToQuery_WhenFromUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(includeFromUtc: false);

        // Act
        GetConsumptionStatisticsQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.FromUtc);
        Assert.NotNull(result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenToUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(includeToUtc: false);

        // Act
        GetConsumptionStatisticsQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ToUtc);
        Assert.NotNull(result.FromUtc);
    }
}