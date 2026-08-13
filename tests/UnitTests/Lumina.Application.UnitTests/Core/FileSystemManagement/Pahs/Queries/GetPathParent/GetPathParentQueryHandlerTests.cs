#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.GetPathParent.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Application.Common.Infrastructure.Validation;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.GetPathParent;

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

    public GetPathParentQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPathService = Substitute.For<IPathService>();
        IValidator<GetPathParentQuery> mockValidator = Substitute.For<IValidator<GetPathParentQuery>>();
        mockValidator.Validate(Arg.Any<GetPathParentQuery>())
            .Returns([]);
        _sut = new GetPathParentQueryHandler(_mockPathService, mockValidator);
        _pathSegmentFixture = new PathSegmentFixture();
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        GetPathParentQuery query = GetPathParentQueryFixture.CreateGetPathParentQuery();

        IEnumerable<PathSegment> pathSegments = _pathSegmentFixture.CreateMany();

        // Create PathSegmentResponses based on the PathSegments
        IEnumerable<PathSegmentResponse> pathSegmentResponses = pathSegments.Select(segment => new PathSegmentResponse(segment.Name));

        _mockPathService.GoUpOneLevel(query.Path!)
            .Returns(ErrorOrFactory.From(pathSegments));

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.IsAssignableFrom<IEnumerable<PathSegmentResponse>>(result.Value);
        Assert.Equal(pathSegmentResponses, result.Value);
        _mockPathService.Received(1).GoUpOneLevel(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetPathParentQuery query = _fixture.Create<GetPathParentQuery>();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.GoUpOneLevel(query.Path!)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
        _mockPathService.Received(1).GoUpOneLevel(query.Path!);
    }

    [Fact]
    public async Task HandleAsync_WhenPathServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetPathParentQuery query = _fixture.Create<GetPathParentQuery>();
        ErrorOr<IEnumerable<PathSegment>> emptyList = ErrorOrFactory.From(Enumerable.Empty<PathSegment>());
        _mockPathService.GoUpOneLevel(query.Path!)
            .Returns(emptyList);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value);
        _mockPathService.Received(1).GoUpOneLevel(query.Path!);
    }
}
