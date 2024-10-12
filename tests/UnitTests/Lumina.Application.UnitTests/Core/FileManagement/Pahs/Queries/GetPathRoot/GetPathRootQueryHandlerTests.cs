#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Paths.Queries.GetPathRoot;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.GetPathRoot.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.GetPathRoot;

/// <summary>
/// Contains unit tests for the <see cref="GetPathRootQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IMapper _mockMapper;
    private readonly IPathService _mockPathService;
    private readonly GetPathRootQueryHandler _sut;
    private readonly PathSegmentFixture _pathSegmentFixture;
    private readonly PathSegmentResponseFixture _pathSegmentResponseFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootQueryHandlerTests"/> class.
    /// </summary>
    public GetPathRootQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPathService = Substitute.For<IPathService>();
        _mockMapper = Substitute.For<IMapper>();
        _sut = new GetPathRootQueryHandler(_mockPathService, _mockMapper);
        _pathSegmentFixture = new PathSegmentFixture();
        _pathSegmentResponseFixture = new PathSegmentResponseFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        PathSegment pathSegment = _pathSegmentFixture.CreatePathSegment(isDrive: true);
        PathSegmentResponse pathSegmentResponse = _pathSegmentResponseFixture.Create();

        _mockPathService.GetPathRoot(query.Path)
            .Returns(ErrorOrFactory.From(pathSegment));

        _mockMapper.Map<PathSegmentResponse>(Arg.Is<PathSegment>(p => p == pathSegment))
            .Returns(pathSegmentResponse);

        // Act
        ErrorOr<PathSegmentResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(pathSegmentResponse);
        _mockPathService.Received(1).GetPathRoot(query.Path);
        _mockMapper.Received(1).Map<PathSegmentResponse>(Arg.Is<PathSegment>(p => p == pathSegment));
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        Error error = Error.Failure("PathService.Error", "An error occurred");
        _mockPathService.GetPathRoot(query.Path)
            .Returns(error);

        // Act
        ErrorOr<PathSegmentResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        _mockPathService.Received(1).GetPathRoot(query.Path);
        _mockMapper.DidNotReceive().Map<PathSegmentResponse>(Arg.Any<PathSegment>());
    }

    [Fact]
    public async Task Handle_WhenPathServiceReturnsRootPathSegment_ShouldReturnRootSuccessResult()
    {
        // Arrange
        GetPathRootQuery query = GetPathRootQueryFixture.CreateGetPathRootQuery();
        PathSegment rootPathSegment = _pathSegmentFixture.CreatePathSegment(name: "/", isDirectory: true, isDrive: true);
        PathSegmentResponse rootResponse = _pathSegmentResponseFixture.Create();

        _mockPathService.GetPathRoot(query.Path)
            .Returns(ErrorOrFactory.From(rootPathSegment));

        _mockMapper.Map<PathSegmentResponse>(Arg.Is<PathSegment>(p => p == rootPathSegment))
            .Returns(rootResponse);

        // Act
        ErrorOr<PathSegmentResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(rootResponse);
        _mockPathService.Received(1).GetPathRoot(query.Path);
        _mockMapper.Received(1).Map<PathSegmentResponse>(Arg.Is<PathSegment>(p => p == rootPathSegment));
    }
}
