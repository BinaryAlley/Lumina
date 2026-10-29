#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Commands.ClearResumePoints;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ClearResumePointsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsRequestMappingTests
{
    private readonly ClearResumePointsRequestFixture _clearResumePointsRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearResumePointsRequestMappingTests"/> class.
    /// </summary>
    public ClearResumePointsRequestMappingTests()
    {
        _clearResumePointsRequestFixture = new ClearResumePointsRequestFixture();
    }

    [Fact]
    public void ToCommand_WhenMappingPopulatedRequest_ShouldMapEveryRequestField()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        string libraryId = Guid.NewGuid().ToString();
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create(libraryId: libraryId);

        // Act
        ClearResumePointsCommand result = request.ToCommand(targetUserId.ToString());

        // Assert
        Assert.Equal(targetUserId.ToString(), result.TargetUserId);
        Assert.Equal(libraryId, result.LibraryId);
    }

    [Fact]
    public void ToCommand_WhenTargetUserIdIsNull_ShouldMapNullTargetUserId()
    {
        // Arrange
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create();

        // Act
        ClearResumePointsCommand result = request.ToCommand(null);

        // Assert
        Assert.Null(result.TargetUserId);
    }

    [Fact]
    public void ToCommand_WhenLibraryIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create(includeLibraryId: false);

        // Act
        ClearResumePointsCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.LibraryId);
    }
}
