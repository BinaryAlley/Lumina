#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibraryPathTemplateParts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Management.GetLibraryPathTemplateParts;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryPathTemplatePartsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryPathTemplatePartsEndpointTests
{
    private readonly IQueryHandler<GetLibraryPathTemplatePartsQuery, Result<LibraryPathTemplateCatalogResponse>> _mockHandler;
    private readonly GetLibraryPathTemplatePartsEndpoint _sut;
    private readonly GetLibraryPathTemplatePartsRequestFixture _getLibraryPathTemplatePartsRequestFixture = new();
    private readonly LibraryPathTemplateCatalogResponseFixture _libraryPathTemplateCatalogResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryPathTemplatePartsEndpointTests"/> class.
    /// </summary>
    public GetLibraryPathTemplatePartsEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetLibraryPathTemplatePartsQuery, Result<LibraryPathTemplateCatalogResponse>>>();
        _sut = Factory.Create<GetLibraryPathTemplatePartsEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithLibraryPathTemplateCatalogResponse()
    {
        // Arrange
        GetLibraryPathTemplatePartsRequest request = _getLibraryPathTemplatePartsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        LibraryPathTemplateCatalogResponse expectedResponse = _libraryPathTemplateCatalogResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetLibraryPathTemplatePartsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<LibraryPathTemplateCatalogResponse> okResult = Assert.IsType<Ok<LibraryPathTemplateCatalogResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationError_ShouldReturnProblemResult()
    {
        // Arrange
        GetLibraryPathTemplatePartsRequest request = _getLibraryPathTemplatePartsRequestFixture.Create(libraryType: "NotARealLibraryType");
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Library.UnknownLibraryType", "UnknownLibraryType");
        _mockHandler.HandleAsync(Arg.Any<GetLibraryPathTemplatePartsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);

        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.NotNull(validationProblemDetails.Instance);
        Assert.NotNull(validationProblemDetails.Extensions["traceId"]);
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "UnknownLibraryType" }, validationProblemDetails.Errors["Library.UnknownLibraryType"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetLibraryPathTemplatePartsQueryToSender()
    {
        // Arrange
        GetLibraryPathTemplatePartsRequest request = _getLibraryPathTemplatePartsRequestFixture.Create(libraryType: "Music");
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetLibraryPathTemplatePartsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_libraryPathTemplateCatalogResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetLibraryPathTemplatePartsQuery>(query => query.LibraryType == request.LibraryType),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetLibraryPathTemplatePartsRequest request = _getLibraryPathTemplatePartsRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetLibraryPathTemplatePartsQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_libraryPathTemplateCatalogResponseFixture.Create());
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
