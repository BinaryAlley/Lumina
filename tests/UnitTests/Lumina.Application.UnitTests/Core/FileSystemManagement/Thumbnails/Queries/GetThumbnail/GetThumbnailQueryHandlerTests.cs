#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Contains unit tests for the <see cref="GetThumbnailQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IThumbnailService _mockThumbnailService;
    private readonly GetThumbnailQueryHandler _sut;

    public GetThumbnailQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockThumbnailService = Substitute.For<IThumbnailService>();
        _sut = new GetThumbnailQueryHandler(_mockThumbnailService);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        GetThumbnailQuery query = _fixture.Create<GetThumbnailQuery>();
        Thumbnail thumbnail = _fixture.Create<Thumbnail>();

        _mockThumbnailService.GetThumbnailAsync(query.Path!, query.Quality, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(thumbnail));

        // Act
        ErrorOr<ThumbnailResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(thumbnail.Type, result.Value.Type);
        Assert.Equal(thumbnail.Bytes, result.Value.Bytes);
        await _mockThumbnailService.Received(1).GetThumbnailAsync(query.Path!, query.Quality, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenThumbnailServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetThumbnailQuery query = _fixture.Create<GetThumbnailQuery>();
        Error error = Error.Failure("ThumbnailService.Error", "An error occurred");
        _mockThumbnailService.GetThumbnailAsync(query.Path!, query.Quality, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<ThumbnailResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
        await _mockThumbnailService.Received(1).GetThumbnailAsync(query.Path!, query.Quality, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetThumbnailQuery query = _fixture.Create<GetThumbnailQuery>();
        CancellationTokenSource cts = new();
        cts.Cancel();

        _mockThumbnailService.GetThumbnailAsync(query.Path!, query.Quality, cts.Token)
            .Returns(Task.FromCanceled<ErrorOr<Thumbnail>>(cts.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.Handle(query, cts.Token).AsTask());
        await _mockThumbnailService.Received(1).GetThumbnailAsync(query.Path!, query.Quality, cts.Token);
    }
}
