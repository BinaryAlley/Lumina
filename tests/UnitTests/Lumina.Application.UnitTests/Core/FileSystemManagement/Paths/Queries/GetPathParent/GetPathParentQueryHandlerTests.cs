#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;
using Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Queries.GetPathParent;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Paths.Queries.GetPathParent;

/// <summary>
/// Contains unit tests for the <see cref="GetPathParentQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IPathService _mockPathService;
    private readonly GetPathParentQueryHandler _sut;
    private readonly PathSegmentFixture _pathSegmentFixture;
    private readonly GetPathParentQueryFixture _getPathParentQueryFixture;

    public GetPathParentQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPathService = Substitute.For<IPathService>();
        IValidator<GetPathParentQuery> mockValidator = Substitute.For<IValidator<GetPathParentQuery>>();
        mockValidator.Validate(Arg.Any<GetPathParentQuery>())
            .Returns([]);
        _sut = new GetPathParentQueryHandler(_mockPathService, mockValidator);
        _pathSegmentFixture = new PathSegmentFixture();
        _getPathParentQueryFixture = new GetPathParentQueryFixture();
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        GetPathParentQuery query = _getPathParentQueryFixture.Create();

        IEnumerable<PathSegment> pathSegments = _pathSegmentFixture.CreateMany();

        // Create PathSegmentResponses based on the PathSegments
        IEnumerable<PathSegmentResponse> pathSegmentResponses = pathSegments.Select(segment => new PathSegmentResponse(segment.Name));

        _mockPathService.GoUpOneLevel(query.Path!)
            .Returns(Result.From(pathSegments));

        // Act
        Result<IEnumerable<PathSegmentResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.IsAssignableFrom<IEnumerable<PathSegmentResponse>>(result.Value);
        Assert.Equal(pathSegmentResponses, result.Value);
        _mockPathService.Received(1).GoUpOneLevel(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetPathParentQuery query = _getPathParentQueryFixture.Create();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.GoUpOneLevel(query.Path!)
            .Returns(error);

        // Act
        Result<IEnumerable<PathSegmentResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockPathService.Received(1).GoUpOneLevel(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetPathParentQuery query = _getPathParentQueryFixture.Create();
        Result<IEnumerable<PathSegment>> emptyList = Result.From(Enumerable.Empty<PathSegment>());
        _mockPathService.GoUpOneLevel(query.Path!)
            .Returns(emptyList);

        // Act
        Result<IEnumerable<PathSegmentResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
        _mockPathService.Received(1).GoUpOneLevel(query.Path!);
    }
}
