#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Fixtures.Core.Responses.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryMetadataProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Plugins.GetLibraryMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryMetadataProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryMetadataProvidersEndpointTests
{
    private readonly IQueryHandler<GetLibraryMetadataProvidersQuery, Result<IReadOnlyList<LibraryMetadataProviderResponse>>> _mockHandler;
    private readonly GetLibraryMetadataProvidersEndpoint _sut;
    private readonly GetLibraryMetadataProvidersRequestFixture _getLibraryMetadataProvidersRequestFixture = new();
    private readonly LibraryMetadataProviderResponseFixture _libraryMetadataProviderResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryMetadataProvidersEndpointTests"/> class.
    /// </summary>
    public GetLibraryMetadataProvidersEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetLibraryMetadataProvidersQuery, Result<IReadOnlyList<LibraryMetadataProviderResponse>>>>();
        _sut = Factory.Create<GetLibraryMetadataProvidersEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithLibraryMetadataProviderResponses()
    {
        // Arrange
        GetLibraryMetadataProvidersRequest request = _getLibraryMetadataProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        IReadOnlyList<LibraryMetadataProviderResponse> expectedResponses =
        [
            _libraryMetadataProviderResponseFixture.Create(name: "Provider A", isEnabled: true, rank: 1),
            _libraryMetadataProviderResponseFixture.Create(name: "Provider B", isEnabled: false, rank: 2)
        ];
        _mockHandler.HandleAsync(Arg.Any<GetLibraryMetadataProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponses));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<IReadOnlyList<LibraryMetadataProviderResponse>> okResult = Assert.IsType<Ok<IReadOnlyList<LibraryMetadataProviderResponse>>>(result);
        Assert.Equal(expectedResponses, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetLibraryMetadataProvidersRequest request = _getLibraryMetadataProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Library.NotFound", "The requested library was not found.");
        _mockHandler.HandleAsync(Arg.Any<GetLibraryMetadataProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Library.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The requested library was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetLibraryMetadataProvidersQueryToSender()
    {
        // Arrange
        GetLibraryMetadataProvidersRequest request = _getLibraryMetadataProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetLibraryMetadataProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<LibraryMetadataProviderResponse>>([]));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetLibraryMetadataProvidersQuery>(query => query.LibraryId == request.LibraryId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetLibraryMetadataProvidersRequest request = _getLibraryMetadataProvidersRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetLibraryMetadataProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From<IReadOnlyList<LibraryMetadataProviderResponse>>([]);
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
