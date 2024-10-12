#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Application.UnitTests.Core.FileManagement.Thumbnails.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Contains unit tests for the <see cref="GetThumbnailQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IThumbnailService _mockThumbnailService;
    private readonly IMapper _mockMapper;
    private readonly GetThumbnailQueryHandler _sut;

    public GetThumbnailQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockThumbnailService = Substitute.For<IThumbnailService>();
        _mockMapper = Substitute.For<IMapper>();
        _sut = new GetThumbnailQueryHandler(_mockThumbnailService, _mockMapper);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        GetThumbnailQuery query = _fixture.Create<GetThumbnailQuery>();
        Thumbnail thumbnail = _fixture.Create<Thumbnail>();
        ThumbnailResponse thumbnailResponse = ThumbnailResponseFixture.CreateThumbnailResponse();

        _mockThumbnailService.GetThumbnailAsync(query.Path, query.Quality, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(thumbnail));

        _mockMapper.Map<ThumbnailResponse>(Arg.Is<Thumbnail>(t => t == thumbnail))
            .Returns(thumbnailResponse);

        // Act
        ErrorOr<ThumbnailResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(thumbnailResponse);
        await _mockThumbnailService.Received(1).GetThumbnailAsync(query.Path, query.Quality, Arg.Any<CancellationToken>());
        _mockMapper.Received(1).Map<ThumbnailResponse>(Arg.Is<Thumbnail>(t => t == thumbnail));
    }

    [Fact]
    public async Task Handle_WhenThumbnailServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetThumbnailQuery query = _fixture.Create<GetThumbnailQuery>();
        Error error = Error.Failure("ThumbnailService.Error", "An error occurred");
        _mockThumbnailService.GetThumbnailAsync(query.Path, query.Quality, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<ThumbnailResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        await _mockThumbnailService.Received(1).GetThumbnailAsync(query.Path, query.Quality, Arg.Any<CancellationToken>());
        _mockMapper.DidNotReceive().Map<ThumbnailResponse>(Arg.Any<Thumbnail>());
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetThumbnailQuery query = _fixture.Create<GetThumbnailQuery>();
        CancellationTokenSource cts = new();
        cts.Cancel();

        _mockThumbnailService.GetThumbnailAsync(query.Path, query.Quality, cts.Token)
            .Returns(Task.FromCanceled<ErrorOr<Thumbnail>>(cts.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.Handle(query, cts.Token).AsTask());
        await _mockThumbnailService.Received(1).GetThumbnailAsync(query.Path, query.Quality, cts.Token);
        _mockMapper.DidNotReceive().Map<ThumbnailResponse>(Arg.Any<Thumbnail>());
    }
}
