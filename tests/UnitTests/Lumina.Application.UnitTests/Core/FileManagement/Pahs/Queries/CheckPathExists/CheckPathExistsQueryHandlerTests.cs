#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Paths.Queries.CheckPathExists;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.CheckPathExists.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.CheckPathExists;

/// <summary>
/// Contains unit tests for the <see cref="CheckPathExistsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsQueryHandlerTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IPathService _mockPathService;
    private readonly CheckPathExistsQueryHandler _sut;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsQueryHandlerTests"/> class.
    /// </summary>
    public CheckPathExistsQueryHandlerTests()
    {
        _mockPathService = Substitute.For<IPathService>();
        _sut = new CheckPathExistsQueryHandler(_mockPathService);
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task Handle_WhenPathExists_ShouldReturnTrueResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        _mockPathService.Exists(query.Path).Returns(true);

        // Act
        PathExistsResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Exists.Should().BeTrue();
        _mockPathService.Received(1).Exists(query.Path);
    }

    [Fact]
    public async Task Handle_WhenPathDoesNotExist_ShouldReturnFalseResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        _mockPathService.Exists(query.Path).Returns(false);

        // Act
        PathExistsResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Exists.Should().BeFalse();
        _mockPathService.Received(1).Exists(query.Path);
    }

    [Fact]
    public async Task Handle_WhenCalledWithNullPath_ShouldStillCallPathService()
    {
        // Arrange
        CheckPathExistsQuery query = new(null!);
        _mockPathService.Exists(Arg.Any<string>()).Returns(false);

        // Act
        PathExistsResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Exists.Should().BeFalse();
        _mockPathService.Received(1).Exists(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldStillCompleteOperation()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        _mockPathService.Exists(query.Path).Returns(true);
        CancellationToken cancellationToken = new(true);

        // Act
        PathExistsResponse result = await _sut.Handle(query, cancellationToken);

        // Assert
        result.Exists.Should().BeTrue();
        _mockPathService.Received(1).Exists(query.Path);
    }
    #endregion
}
