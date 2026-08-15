#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Application.Fixtures.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
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
    private readonly GetThumbnailQueryFixture _getThumbnailQueryFixture;
    private readonly ThumbnailFixture _thumbnailFixture;

    public GetThumbnailQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockThumbnailService = Substitute.For<IThumbnailService>();
        _getThumbnailQueryFixture = new GetThumbnailQueryFixture();
        _thumbnailFixture = new ThumbnailFixture();
        IValidator<GetThumbnailQuery> mockValidator = Substitute.For<IValidator<GetThumbnailQuery>>();
        mockValidator.Validate(Arg.Any<GetThumbnailQuery>())
            .Returns([]);
        _sut = new GetThumbnailQueryHandler(_mockThumbnailService, mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        GetThumbnailQuery query = _getThumbnailQueryFixture.Create();
        Thumbnail thumbnail = _thumbnailFixture.Create();

        _mockThumbnailService.GetThumbnailAsync(query.Path!, query.Quality, Arg.Any<CancellationToken>())
            .Returns(Result.From(thumbnail));

        // Act
        Result<ThumbnailResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(thumbnail.Type, result.Value.Type);
        Assert.Equal(thumbnail.Bytes, result.Value.Bytes);
        await _mockThumbnailService.Received(1).GetThumbnailAsync(query.Path!, query.Quality, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThumbnailServiceReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetThumbnailQuery query = _getThumbnailQueryFixture.Create();
        Error error = Error.Failure("ThumbnailService.Error", "An error occurred");
        _mockThumbnailService.GetThumbnailAsync(query.Path!, query.Quality, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ThumbnailResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockThumbnailService.Received(1).GetThumbnailAsync(query.Path!, query.Quality, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetThumbnailQuery query = _getThumbnailQueryFixture.Create();
        CancellationTokenSource cts = new();
        cts.Cancel();

        _mockThumbnailService.GetThumbnailAsync(query.Path!, query.Quality, cts.Token)
            .Returns(Task.FromCanceled<Result<Thumbnail>>(cts.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.HandleAsync(query, cts.Token));
        await _mockThumbnailService.Received(1).GetThumbnailAsync(query.Path!, query.Quality, cts.Token);
    }
}
