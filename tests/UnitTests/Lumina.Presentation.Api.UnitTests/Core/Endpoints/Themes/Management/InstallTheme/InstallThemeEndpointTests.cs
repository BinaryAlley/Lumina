#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.Themes.Management.Commands.InstallTheme;
using Lumina.Contracts.Fixtures.Core.Responses.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Themes.Management.InstallTheme;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Themes.Management.InstallTheme;

/// <summary>
/// Contains unit tests for the <see cref="InstallThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallThemeEndpointTests
{
    private readonly Application.Common.CQRS.ICommandHandler<InstallThemeCommand, Result<ThemeResponse>> _mockHandler;
    private readonly InstallThemeEndpoint _sut;
    private readonly ThemeResponseFixture _themeResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeEndpointTests"/> class.
    /// </summary>
    public InstallThemeEndpointTests()
    {
        _mockHandler = Substitute.For<Application.Common.CQRS.ICommandHandler<InstallThemeCommand, Result<ThemeResponse>>>();
        _sut = Factory.Create<InstallThemeEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArchiveUploaded_ShouldReturnOkResultWithInstalledTheme()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        ThemeResponse expectedResponse = _themeResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<InstallThemeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        ConfigureFormWithArchive([1, 2, 3, 4], "theme-pack.zip");

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<ThemeResponse> okResult = Assert.IsType<Ok<ThemeResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArchiveUploaded_ShouldSendInstallThemeCommandWithUploadedStreamAndFileName()
    {
        // Arrange
        byte[] content = [1, 2, 3, 4];
        string fileName = "theme-pack.zip";
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<InstallThemeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themeResponseFixture.Create()));
        ConfigureFormWithArchive(content, fileName);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<InstallThemeCommand>(command =>
                command.FileName == fileName &&
                command.Archive != null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoArchiveUploaded_ShouldSendInstallThemeCommandWithNullArchiveAndFileName()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        ThemeResponse expectedResponse = _themeResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<InstallThemeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        ConfigureFormWithoutFiles();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<ThemeResponse> okResult = Assert.IsType<Ok<ThemeResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<InstallThemeCommand>(command =>
                command.Archive == null &&
                command.FileName == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<InstallThemeCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        ConfigureFormWithArchive([1, 2, 3, 4], "theme-pack.zip");

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetailsBody = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetailsBody.Status);
        Assert.Equal("General.Unauthorized", problemDetailsBody.Title);
        Assert.Equal("NotAuthorized", problemDetailsBody.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetailsBody.Type);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<InstallThemeCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_themeResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        ConfigureFormWithArchive([1, 2, 3, 4], "theme-pack.zip");

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    private void ConfigureFormWithArchive(byte[] content, string fileName)
    {
        // the stream stays open for the duration of the test, since the endpoint reads it through the form file during execution
        MemoryStream archiveStream = new(content);
        IFormFile formFile = new FormFile(archiveStream, 0, content.Length, "archive", fileName);
        FormFileCollection files = [formFile];
        IFormCollection form = new FormCollection([], files);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        // a real FormFeature is used instead of a substitute, because the endpoint execution reads the form through the
        // request machinery, which would bypass a mocked feature and parse the (empty) request body
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }

    private void ConfigureFormWithoutFiles()
    {
        IFormCollection form = new FormCollection([]);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }
}
