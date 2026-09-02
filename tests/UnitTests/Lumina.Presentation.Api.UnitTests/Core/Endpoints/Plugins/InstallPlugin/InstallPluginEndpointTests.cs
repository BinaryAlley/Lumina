#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.Plugins.Commands.InstallPlugin;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.InstallPlugin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// Contains unit tests for the <see cref="InstallPluginEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginEndpointTests
{
    private readonly Application.Common.CQRS.ICommandHandler<InstallPluginCommand, Result<Success>> _mockHandler;
    private readonly InstallPluginEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpointTests"/> class.
    /// </summary>
    public InstallPluginEndpointTests()
    {
        _mockHandler = Substitute.For<Application.Common.CQRS.ICommandHandler<InstallPluginCommand, Result<Success>>>();
        _sut = Factory.Create<InstallPluginEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArchiveUploaded_ShouldReturnOkResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<InstallPluginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));
        ConfigureFormWithArchive([1, 2, 3, 4], "plugin.zip");

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArchiveUploaded_ShouldSendInstallPluginCommandWithUploadedStreamAndFileName()
    {
        // Arrange
        byte[] content = [1, 2, 3, 4];
        string fileName = "plugin.zip";
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<InstallPluginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));
        ConfigureFormWithArchive(content, fileName);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<InstallPluginCommand>(command =>
                command.FileName == fileName &&
                command.Archive != null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoArchiveUploaded_ShouldSendInstallPluginCommandWithNullArchiveAndFileName()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<InstallPluginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));
        ConfigureFormWithoutFiles();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.IsType<Ok>(result);
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<InstallPluginCommand>(command =>
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
        _mockHandler.HandleAsync(Arg.Any<InstallPluginCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        ConfigureFormWithArchive([1, 2, 3, 4], "plugin.zip");

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

        _mockHandler.HandleAsync(Arg.Any<InstallPluginCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Result.Success);
            }, callInfo.Arg<CancellationToken>()));
        ConfigureFormWithArchive([1, 2, 3, 4], "plugin.zip");

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheFormIsMalformed_ShouldTreatTheUploadAsMissingAndSendNullArchive()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<InstallPluginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));
        ConfigureMalformedForm();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.IsType<Ok>(result);
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<InstallPluginCommand>(command =>
                command.Archive == null &&
                command.FileName == null),
            Arg.Is(cancellationToken));
    }

    private void ConfigureFormWithArchive(byte[] content, string fileName)
    {
        // the stream stays open for the duration of the test, since the endpoint reads it through the form file during execution
        MemoryStream archiveStream = new(content);
        IFormFile formFile = new FormFile(archiveStream, 0, content.Length, "archive", fileName);
        FormFileCollection files = [formFile];
        IFormCollection form = new FormCollection([], files);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        // A real FormFeature is used instead of a substitute, because the endpoint execution reads the form through the
        // request machinery, which would bypass a mocked feature and parse the (empty) request body.
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }

    private void ConfigureFormWithoutFiles()
    {
        IFormCollection form = new FormCollection([]);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }

    private void ConfigureMalformedForm()
    {
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        IFormCollection malformedForm = Substitute.For<IFormCollection>();
        malformedForm.Files.Returns(_ => throw new InvalidDataException("Malformed multipart body."));
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(malformedForm));
    }
}
