#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Paths.Queries.GetPathParent;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.GetPathParent.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.GetPathParent;

/// <summary>
/// Contains unit tests for the <see cref="GetPathParentQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IMapper _mockMapper;
    private readonly IPathService _mockPathService;
    private readonly GetPathParentQueryHandler _sut;
    private readonly PathSegmentFixture _pathSegmentFixture;
    private readonly PathSegmentResponseFixture _pathSegmentResponseFixture;

    public GetPathParentQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPathService = Substitute.For<IPathService>();
        _mockMapper = Substitute.For<IMapper>();
        _sut = new GetPathParentQueryHandler(_mockPathService, _mockMapper);
        _pathSegmentFixture = new PathSegmentFixture();
        _pathSegmentResponseFixture = new PathSegmentResponseFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        GetPathParentQuery query = GetPathParentQueryFixture.CreateGetPathParentQuery();

        IEnumerable<PathSegment> pathSegments = _pathSegmentFixture.CreateMany();
        IEnumerable<PathSegmentResponse> pathSegmentResponses = _pathSegmentResponseFixture.CreateMany(pathSegments.Count());

        _mockPathService.GoUpOneLevel(query.Path)
            .Returns(ErrorOrFactory.From(pathSegments));

        _mockMapper.Map<IEnumerable<PathSegmentResponse>>(Arg.Is<IEnumerable<PathSegment>>(p => p == pathSegments))
            .Returns(pathSegmentResponses);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeAssignableTo<IEnumerable<PathSegmentResponse>>();
        result.Value.Should().BeEquivalentTo(pathSegmentResponses);
        _mockPathService.Received(1).GoUpOneLevel(query.Path);
        _mockMapper.Received(1).Map<IEnumerable<PathSegmentResponse>>(Arg.Is<IEnumerable<PathSegment>>(p => p == pathSegments));
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetPathParentQuery query = _fixture.Create<GetPathParentQuery>();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.GoUpOneLevel(query.Path)
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockPathService.Received(1).GoUpOneLevel(query.Path);
        _mockMapper.DidNotReceive().Map<IEnumerable<PathSegmentResponse>>(Arg.Any<IEnumerable<PathSegment>>());
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsEmptyList_ShouldReturnEmptySuccessResult()
    {
        // Arrange
        GetPathParentQuery query = _fixture.Create<GetPathParentQuery>();
        ErrorOr<IEnumerable<PathSegment>> emptyList = ErrorOrFactory.From(Enumerable.Empty<PathSegment>());
        _mockPathService.GoUpOneLevel(query.Path)
            .Returns(emptyList);

        _mockMapper.Map<IEnumerable<PathSegmentResponse>>(Arg.Is<IEnumerable<PathSegment>>(p => !p.Any()))
            .Returns([]);

        // Act
        ErrorOr<IEnumerable<PathSegmentResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockPathService.Received(1).GoUpOneLevel(query.Path);
        _mockMapper.Received(1).Map<IEnumerable<PathSegmentResponse>>(Arg.Is<IEnumerable<PathSegment>>(p => !p.Any()));
    }
}
