#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Fixtures.Core.Responses.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryTypeArtworkProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Plugins.GetLibraryTypeArtworkProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeArtworkProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeArtworkProvidersEndpointTests
{
    private readonly IQueryHandler<GetLibraryTypeArtworkProvidersQuery, Result<IReadOnlyList<LibraryArtworkProviderResponse>>> _mockHandler;
    private readonly GetLibraryTypeArtworkProvidersEndpoint _sut;
    private readonly GetLibraryTypeArtworkProvidersRequestFixture _getLibraryTypeArtworkProvidersRequestFixture = new();
    private readonly LibraryArtworkProviderResponseFixture _libraryArtworkProviderResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeArtworkProvidersEndpointTests"/> class.
    /// </summary>
    public GetLibraryTypeArtworkProvidersEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetLibraryTypeArtworkProvidersQuery, Result<IReadOnlyList<LibraryArtworkProviderResponse>>>>();
        _sut = Factory.Create<GetLibraryTypeArtworkProvidersEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithArtworkProviders()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersRequest request = _getLibraryTypeArtworkProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        IReadOnlyList<LibraryArtworkProviderResponse> expectedResponse = _libraryArtworkProviderResponseFixture.CreateMany(2);
        _mockHandler.HandleAsync(Arg.Any<GetLibraryTypeArtworkProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<IReadOnlyList<LibraryArtworkProviderResponse>> okResult = Assert.IsType<Ok<IReadOnlyList<LibraryArtworkProviderResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsFailureError_ShouldReturnProblemResult()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersRequest request = _getLibraryTypeArtworkProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Authorization.NotAuthorized", "NotAuthorized");
        _mockHandler.HandleAsync(Arg.Any<GetLibraryTypeArtworkProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.Equal("Authorization.NotAuthorized", problemResult.ProblemDetails.Title);
        Assert.Equal("NotAuthorized", problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetLibraryTypeArtworkProvidersQueryToSender()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersRequest request = _getLibraryTypeArtworkProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        IReadOnlyList<LibraryArtworkProviderResponse> response = _libraryArtworkProviderResponseFixture.CreateMany(2);
        _mockHandler.HandleAsync(Arg.Any<GetLibraryTypeArtworkProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetLibraryTypeArtworkProvidersQuery>(query => query.LibraryType == request.LibraryType),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersRequest request = _getLibraryTypeArtworkProvidersRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetLibraryTypeArtworkProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From<IReadOnlyList<LibraryArtworkProviderResponse>>([]);
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
