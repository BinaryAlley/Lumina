#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetTagConsumption;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetTagConsumptionRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTagConsumptionRequestMappingTests
{
    private readonly GetTagConsumptionRequestFixture _getTagConsumptionRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagConsumptionRequestMappingTests"/> class.
    /// </summary>
    public GetTagConsumptionRequestMappingTests()
    {
        _getTagConsumptionRequestFixture = new GetTagConsumptionRequestFixture();
    }

    [Fact]
    public void ToQuery_WhenMappingPopulatedRequest_ShouldMapEveryRequestField()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        string libraryId = Guid.NewGuid().ToString();
        string fromUtc = "2025-01-02T03:04:05Z";
        string toUtc = "2025-02-03T04:05:06Z";
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.Book),
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Act
        GetTagConsumptionQuery result = request.ToQuery(targetUserId.ToString());

        // Assert
        Assert.Equal(targetUserId.ToString(), result.TargetUserId);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.NotNull(result.MediaKind);
        Assert.Equal(nameof(MediaKind.Book), result.MediaKind);
        Assert.NotNull(result.FromUtc);
        Assert.Equal(fromUtc, result.FromUtc);
        Assert.NotNull(result.ToUtc);
        Assert.Equal(toUtc, result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenTargetUserIdIsNull_ShouldMapNullTargetUserId()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create();

        // Act
        GetTagConsumptionQuery result = request.ToQuery(null);

        // Assert
        Assert.Null(result.TargetUserId);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create(includeLibraryId: false);

        // Act
        GetTagConsumptionQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create(includeMediaKind: false);

        // Act
        GetTagConsumptionQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.MediaKind);
    }

    [Fact]
    public void ToQuery_WhenFromUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create(includeFromUtc: false);

        // Act
        GetTagConsumptionQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.FromUtc);
        Assert.NotNull(result.ToUtc);
    }

    [Fact]
    public void ToQuery_WhenToUtcIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create(includeToUtc: false);

        // Act
        GetTagConsumptionQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ToUtc);
        Assert.NotNull(result.FromUtc);
    }
}