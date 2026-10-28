#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetGenreConsumption;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetGenreConsumptionRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetGenreConsumptionRequestMappingTests
{
    private readonly GetGenreConsumptionRequestFixture _getGenreConsumptionRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenreConsumptionRequestMappingTests"/> class.
    /// </summary>
    public GetGenreConsumptionRequestMappingTests()
    {
        _getGenreConsumptionRequestFixture = new GetGenreConsumptionRequestFixture();
    }

    [Fact]
    public void ToQuery_WhenMappingPopulatedRequest_ShouldMapEveryRequestField()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        string libraryId = Guid.NewGuid().ToString();
        string mediaKind = nameof(MediaKind.Book);
        string fromUtc = "2025-01-02T03:04:05.0000000Z";
        string toUtc = "2025-02-03T04:05:06.0000000Z";
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: mediaKind,
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Act
        GetGenreConsumptionQuery result = request.ToQuery(targetUserId.ToString());

        // Assert
        Assert.Equal(targetUserId.ToString(), result.TargetUserId);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(mediaKind, result.MediaKind);
        Assert.Equal(fromUtc, result.FromUtc);
        Assert.Equal(toUtc, result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenTargetUserIdIsNull_ShouldMapNullTargetUserId()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create();

        // Act
        GetGenreConsumptionQuery result = request.ToQuery(null);

        // Assert
        Assert.Null(result.TargetUserId);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(includeLibraryId: false);

        // Act
        GetGenreConsumptionQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(includeMediaKind: false);

        // Act
        GetGenreConsumptionQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.MediaKind);
    }

    [Fact]
    public void ToQuery_WhenFromUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(includeFromUtc: false);

        // Act
        GetGenreConsumptionQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.FromUtc);
        Assert.NotNull(result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenToUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetGenreConsumptionRequest request = _getGenreConsumptionRequestFixture.Create(includeToUtc: false);

        // Act
        GetGenreConsumptionQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ToUtc);
        Assert.NotNull(result.FromUtc);
    }
}
