#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Fixtures.Core.Responses.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryTypeMetadataProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Plugins.GetLibraryTypeMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeMetadataProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeMetadataProvidersEndpointTests
{
    private readonly IQueryHandler<GetLibraryTypeMetadataProvidersQuery, Result<IReadOnlyList<LibraryMetadataProviderResponse>>> _mockHandler;
    private readonly GetLibraryTypeMetadataProvidersEndpoint _sut;
    private readonly GetLibraryTypeMetadataProvidersRequestFixture _getLibraryTypeMetadataProvidersRequestFixture = new();
    private readonly LibraryMetadataProviderResponseFixture _libraryMetadataProviderResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeMetadataProvidersEndpointTests"/> class.
    /// </summary>
    public GetLibraryTypeMetadataProvidersEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetLibraryTypeMetadataProvidersQuery, Result<IReadOnlyList<LibraryMetadataProviderResponse>>>>();
        _sut = Factory.Create<GetLibraryTypeMetadataProvidersEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithMetadataProviders()
    {
        // Arrange
        GetLibraryTypeMetadataProvidersRequest request = _getLibraryTypeMetadataProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        IReadOnlyList<LibraryMetadataProviderResponse> expectedResponse = _libraryMetadataProviderResponseFixture.CreateMany(2);
        _mockHandler.HandleAsync(Arg.Any<GetLibraryTypeMetadataProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<IReadOnlyList<LibraryMetadataProviderResponse>> okResult = Assert.IsType<Ok<IReadOnlyList<LibraryMetadataProviderResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsFailureError_ShouldReturnProblemResult()
    {
        // Arrange
        GetLibraryTypeMetadataProvidersRequest request = _getLibraryTypeMetadataProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Authorization.NotAuthorized", "NotAuthorized");
        _mockHandler.HandleAsync(Arg.Any<GetLibraryTypeMetadataProvidersQuery>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetLibraryTypeMetadataProvidersQueryToSender()
    {
        // Arrange
        GetLibraryTypeMetadataProvidersRequest request = _getLibraryTypeMetadataProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        IReadOnlyList<LibraryMetadataProviderResponse> response = _libraryMetadataProviderResponseFixture.CreateMany(2);
        _mockHandler.HandleAsync(Arg.Any<GetLibraryTypeMetadataProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetLibraryTypeMetadataProvidersQuery>(query => query.LibraryType == request.LibraryType),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetLibraryTypeMetadataProvidersRequest request = _getLibraryTypeMetadataProvidersRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetLibraryTypeMetadataProvidersQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From<IReadOnlyList<LibraryMetadataProviderResponse>>([]);
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
