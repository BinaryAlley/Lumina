#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.CheckPathExists.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.CheckPathExists;

/// <summary>
/// Contains unit tests for the <see cref="CheckPathExistsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsQueryHandlerTests
{
    private readonly IPathService _mockPathService;
    private readonly CheckPathExistsQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsQueryHandlerTests"/> class.
    /// </summary>
    public CheckPathExistsQueryHandlerTests()
    {
        _mockPathService = Substitute.For<IPathService>();
        _sut = new CheckPathExistsQueryHandler(_mockPathService);
    }

    [Fact]
    public async Task Handle_WhenPathExistsAndIsNotHidden_ShouldReturnTrueResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        _mockPathService.Exists(query.Path!).Returns(true);

        // Act
        PathExistsResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Exists.Should().BeTrue();
        _mockPathService.Received(1).Exists(query.Path!);
    }

    [Fact]
    public async Task Handle_WhenPathExistsAndIsHiddenAndIncludeHiddenElementsIsTrue_ShouldReturnTrueResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery(true);
        _mockPathService.Exists(query.Path!).Returns(true);

        // Act
        PathExistsResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Exists.Should().BeTrue();
        _mockPathService.Received(1).Exists(query.Path!);
    }

    [Fact]
    public async Task Handle_WhenPathExistsAndIsHiddenAndIncludeHiddenElementsIsFalse_ShouldReturnFalseResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery(false);
        _mockPathService.Exists(query.Path!, false).Returns(false);

        // Act
        PathExistsResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Exists.Should().BeFalse();
        _mockPathService.Received(1).Exists(query.Path!, false);
    }

    [Fact]
    public async Task Handle_WhenPathDoesNotExist_ShouldReturnFalseResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        _mockPathService.Exists(query.Path!).Returns(false);

        // Act
        PathExistsResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Exists.Should().BeFalse();
        _mockPathService.Received(1).Exists(query.Path!);
    }

    [Fact]
    public async Task Handle_WhenCalledWithNullPath_ShouldStillCallPathService()
    {
        // Arrange
        CheckPathExistsQuery query = new(null!, false);
        _mockPathService.Exists(Arg.Any<string>(), Arg.Any<bool>()).Returns(false);

        // Act
        PathExistsResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Exists.Should().BeFalse();
        _mockPathService.Received(1).Exists(Arg.Any<string>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldStillCompleteOperation()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        _mockPathService.Exists(query.Path!).Returns(true);
        CancellationToken cancellationToken = new(true);

        // Act
        PathExistsResponse result = await _sut.Handle(query, cancellationToken);

        // Assert
        result.Exists.Should().BeTrue();
        _mockPathService.Received(1).Exists(query.Path!);
    }
}
