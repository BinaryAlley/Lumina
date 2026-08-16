#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.UpdateUserSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Settings.UpdateUserSettings;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsEndpointTests
{
    private readonly ICommandHandler<UpdateUserSettingsCommand, Result<Updated>> _mockHandler;
    private readonly UpdateUserSettingsEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpointTests"/> class.
    /// </summary>
    public UpdateUserSettingsEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<UpdateUserSettingsCommand, Result<Updated>>>();
        _sut = FastEndpoints.Factory.Create<UpdateUserSettingsEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithUpdatedMarker()
    {
        // Arrange
        UpdateUserSettingsRequest request = new(true, 48, false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateUserSettingsCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<Updated> okResult = Assert.IsType<Ok<Updated>>(result);
        Assert.Equal(Result.Updated, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        UpdateUserSettingsRequest request = new(true, 0, false);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("UserSettings.ItemsPerPageMustBeGreaterThanZero", "ItemsPerPage must be greater than zero.");
        _mockHandler.HandleAsync(Arg.Any<UpdateUserSettingsCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.IsType<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal("General.Validation", problemResult.ProblemDetails.Title);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendUpdateUserSettingsCommandToHandler()
    {
        // Arrange
        UpdateUserSettingsRequest request = new(true, 24, true);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateUserSettingsCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateUserSettingsCommand>(command =>
                command.IsPaginationEnabled == request.IsPaginationEnabled &&
                command.ItemsPerPage == request.ItemsPerPage &&
                command.IgnoreThePrefixForAlphaPicker == request.IgnoreThePrefixForAlphaPicker),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<UpdateUserSettingsCommand>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Result.Updated);
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new UpdateUserSettingsRequest(true, 48, false), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
