#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.GetPathRoot.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.GetPathRoot;

/// <summary>
/// Contains unit tests for the <see cref="GetPathRootQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootQueryHandlerTests
{
    private readonly IPathService _mockPathService;
    private readonly GetPathRootQueryHandler _sut;
    private readonly PathSegmentFixture _pathSegmentFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootQueryHandlerTests"/> class.
    /// </summary>
    public GetPathRootQueryHandlerTests()
    {
        _mockPathService = Substitute.For<IPathService>();
        _sut = new GetPathRootQueryHandler(_mockPathService);
        _pathSegmentFixture = new PathSegmentFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        PathSegment pathSegment = _pathSegmentFixture.CreatePathSegment(isDrive: true);

        _mockPathService.GetPathRoot(query.Path!)
            .Returns(ErrorOrFactory.From(pathSegment));

        // Act
        ErrorOr<PathSegmentResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(pathSegment.Name, result.Value.Path);
        _mockPathService.Received(1).GetPathRoot(query.Path!);
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.GetPathRoot(query.Path!)
            .Returns(error);

        // Act
        ErrorOr<PathSegmentResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
        _mockPathService.Received(1).GetPathRoot(query.Path!);
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsRootPathSegment_ShouldReturnRootSuccessResult()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        PathSegment rootPathSegment = _pathSegmentFixture.CreatePathSegment(name: "/", isDirectory: true, isDrive: true);

        _mockPathService.GetPathRoot(query.Path!)
            .Returns(ErrorOrFactory.From(rootPathSegment));

        // Act
        ErrorOr<PathSegmentResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal("/", result.Value.Path);
        _mockPathService.Received(1).GetPathRoot(query.Path!);
    }
}
