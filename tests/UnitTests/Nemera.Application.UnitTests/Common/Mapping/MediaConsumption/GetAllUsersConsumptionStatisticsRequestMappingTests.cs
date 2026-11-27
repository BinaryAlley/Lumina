#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetAllUsersConsumptionStatisticsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsRequestMappingTests
{
    private readonly GetAllUsersConsumptionStatisticsRequestFixture _getAllUsersConsumptionStatisticsRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsRequestMappingTests"/> class.
    /// </summary>
    public GetAllUsersConsumptionStatisticsRequestMappingTests()
    {
        _getAllUsersConsumptionStatisticsRequestFixture = new GetAllUsersConsumptionStatisticsRequestFixture();
    }

    [Fact]
    public void ToQuery_WhenMappingPopulatedRequest_ShouldMapEveryRequestField()
    {
        // Arrange
        string libraryId = Guid.NewGuid().ToString();
        string fromUtc = "2025-01-02T03:04:05Z";
        string toUtc = "2025-02-03T04:05:06Z";
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.Audiobook),
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Act
        GetAllUsersConsumptionStatisticsQuery result = request.ToQuery();

        // Assert
        Assert.Equal(libraryId, result.LibraryId);
        Assert.NotNull(result.MediaKind);
        Assert.Equal(nameof(MediaKind.Audiobook), result.MediaKind);
        Assert.NotNull(result.FromUtc);
        Assert.Equal(fromUtc, result.FromUtc);
        Assert.NotNull(result.ToUtc);
        Assert.Equal(toUtc, result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(includeLibraryId: false);

        // Act
        GetAllUsersConsumptionStatisticsQuery result = request.ToQuery();

        // Assert
        Assert.Null(result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(includeMediaKind: false);

        // Act
        GetAllUsersConsumptionStatisticsQuery result = request.ToQuery();

        // Assert
        Assert.Null(result.MediaKind);
    }

    [Fact]
    public void ToQuery_WhenFromUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(includeFromUtc: false);

        // Act
        GetAllUsersConsumptionStatisticsQuery result = request.ToQuery();

        // Assert
        Assert.Null(result.FromUtc);
        Assert.NotNull(result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenToUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(includeToUtc: false);

        // Act
        GetAllUsersConsumptionStatisticsQuery result = request.ToQuery();

        // Assert
        Assert.Null(result.ToUtc);
        Assert.NotNull(result.FromUtc);
    }
}