#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.CheckPathExists.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Common.Primitives;
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
        IValidator<CheckPathExistsQuery> mockValidator = Substitute.For<IValidator<CheckPathExistsQuery>>();
        mockValidator.Validate(Arg.Any<CheckPathExistsQuery>())
            .Returns([]);
        _sut = new CheckPathExistsQueryHandler(_mockPathService, mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenPathExistsAndIsNotHidden_ShouldReturnTrueResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        _mockPathService.Exists(query.Path!).Returns(true);

        // Act
        Result<PathExistsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.Value.Exists);
        _mockPathService.Received(1).Exists(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathExistsAndIsHiddenAndIncludeHiddenElementsIsTrue_ShouldReturnTrueResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery(true);
        _mockPathService.Exists(query.Path!).Returns(true);

        // Act
        Result<PathExistsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.Value.Exists);
        _mockPathService.Received(1).Exists(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathExistsAndIsHiddenAndIncludeHiddenElementsIsFalse_ShouldReturnFalseResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery(false);
        _mockPathService.Exists(query.Path!, false).Returns(false);

        // Act
        Result<PathExistsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Exists);
        _mockPathService.Received(1).Exists(query.Path!, false);
    }

    [Fact]
    public async Task HandleAsync_WhenPathDoesNotExist_ShouldReturnFalseResponse()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        _mockPathService.Exists(query.Path!).Returns(false);

        // Act
        Result<PathExistsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Exists);
        _mockPathService.Received(1).Exists(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithNullPath_ShouldStillCallPathService()
    {
        // Arrange
        CheckPathExistsQuery query = new(null!, false);
        _mockPathService.Exists(Arg.Any<string>(), Arg.Any<bool>()).Returns(false);

        // Act
        Result<PathExistsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Exists);
        _mockPathService.Received(1).Exists(Arg.Any<string>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationRequested_ShouldStillCompleteOperation()
    {
        // Arrange
        CheckPathExistsQuery query = CheckPathExistsQueryFixture.CreateCheckPathExistsQuery();
        _mockPathService.Exists(query.Path!).Returns(true);
        CancellationToken cancellationToken = new(true);

        // Act
        Result<PathExistsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.Value.Exists);
        _mockPathService.Received(1).Exists(query.Path!);
    }
}
