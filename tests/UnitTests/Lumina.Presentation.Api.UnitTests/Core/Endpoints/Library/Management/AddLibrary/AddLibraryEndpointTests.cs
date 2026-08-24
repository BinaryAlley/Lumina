#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.AddLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Management.AddLibrary;

/// <summary>
/// Contains unit tests for the <see cref="AddLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryEndpointTests
{
    private readonly ICommandHandler<AddLibraryCommand, Result<LibraryResponse>> _mockHandler;
    private readonly AddLibraryEndpoint _sut;
    private readonly AddLibraryRequestFixture _addLibraryRequestFixture = new();
    private readonly LibraryResponseFixture _libraryResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryEndpointTests"/> class.
    /// </summary>
    public AddLibraryEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<AddLibraryCommand, Result<LibraryResponse>>>();
        _sut = FastEndpoints.Factory.Create<AddLibraryEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnCreatedResultWithLibraryResponse()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        LibraryResponse expectedResponse = _libraryResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<AddLibraryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Created<LibraryResponse> createdResult = Assert.IsType<Created<LibraryResponse>>(result);
        Assert.Equal(expectedResponse, createdResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Conflict("Library.AlreadyExists", "The media library already exists.");
        _mockHandler.HandleAsync(Arg.Any<AddLibraryCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Library.AlreadyExists", problemDetails.ProblemDetails.Title);
        Assert.Equal("The media library already exists.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.10", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendAddLibraryCommandToSender()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddLibraryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_libraryResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddLibraryCommand>(command =>
                command.Title == request.Title &&
                command.LibraryType == request.LibraryType &&
                command.ContentLocations == request.ContentLocations &&
                command.CoverImage == request.CoverImage &&
                command.IsEnabled == request.IsEnabled &&
                command.IsLocked == request.IsLocked &&
                command.CanDownloadMetadataFromWeb == request.CanDownloadMetadataFromWeb &&
                command.ShouldSaveMetadataInMediaDirectories == request.ShouldSaveMetadataInMediaDirectories &&
                command.ShouldSkipUnchangedDirectoriesDuringScan == request.ShouldSkipUnchangedDirectoriesDuringScan),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<AddLibraryCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_libraryResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
