#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Paths.Queries.GetPathRoot;

/// <summary>
/// Contains unit tests for the <see cref="GetPathRootQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootQueryHandlerTests
{
    private readonly IPathService _mockPathService;
    private readonly GetPathRootQueryHandler _sut;
    private readonly PathSegmentFixture _pathSegmentFixture;
    private readonly GetPathRootQueryFixture _getPathRootQueryFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootQueryHandlerTests"/> class.
    /// </summary>
    public GetPathRootQueryHandlerTests()
    {
        _mockPathService = Substitute.For<IPathService>();
        _getPathRootQueryFixture = new GetPathRootQueryFixture();
        IValidator<GetPathRootQuery> mockValidator = Substitute.For<IValidator<GetPathRootQuery>>();
        mockValidator.Validate(Arg.Any<GetPathRootQuery>())
            .Returns([]);
        _sut = new GetPathRootQueryHandler(_mockPathService, mockValidator);
        _pathSegmentFixture = new PathSegmentFixture();
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        GetPathRootQuery query = _getPathRootQueryFixture.Create();
        PathSegment pathSegment = _pathSegmentFixture.Create(isDrive: true);

        _mockPathService.GetPathRoot(query.Path!)
            .Returns(Result.From(pathSegment));

        // Act
        Result<PathSegmentResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(pathSegment.Name, result.Value.Path);
        _mockPathService.Received(1).GetPathRoot(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetPathRootQuery query = _getPathRootQueryFixture.Create();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.GetPathRoot(query.Path!)
            .Returns(error);

        // Act
        Result<PathSegmentResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockPathService.Received(1).GetPathRoot(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsRootPathSegment_ShouldReturnRootSuccessResult()
    {
        // Arrange
        GetPathRootQuery query = _getPathRootQueryFixture.Create();
        PathSegment rootPathSegment = _pathSegmentFixture.Create(name: "/", isDirectory: true, isDrive: true);

        _mockPathService.GetPathRoot(query.Path!)
            .Returns(Result.From(rootPathSegment));

        // Act
        Result<PathSegmentResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("/", result.Value.Path);
        _mockPathService.Received(1).GetPathRoot(query.Path!);
    }
}
