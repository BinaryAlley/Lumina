#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Scheduling.Commands.RemoveScheduledJob;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Scheduling.RemoveScheduledJob;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Scheduling.RemoveScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="RemoveScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobEndpointTests
{
    private readonly Application.Common.CQRS.ICommandHandler<RemoveScheduledJobCommand, Result<Success>> _mockHandler;
    private readonly RemoveScheduledJobEndpoint _sut;
    private readonly RemoveScheduledJobRequestFixture _removeScheduledJobRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveScheduledJobEndpointTests"/> class.
    /// </summary>
    public RemoveScheduledJobEndpointTests()
    {
        _mockHandler = Substitute.For<Application.Common.CQRS.ICommandHandler<RemoveScheduledJobCommand, Result<Success>>>();
        _sut = Factory.Create<RemoveScheduledJobEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResult()
    {
        // Arrange
        RemoveScheduledJobRequest request = _removeScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RemoveScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        IStatusCodeHttpResult statusCodeResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        RemoveScheduledJobRequest request = _removeScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Domain.Common.Errors.Errors.Scheduling.ScheduledJobNotFound;
        _mockHandler.HandleAsync(Arg.Any<RemoveScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorized_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        RemoveScheduledJobRequest request = _removeScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RemoveScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendMappedCommandToHandler()
    {
        // Arrange
        RemoveScheduledJobRequest request = _removeScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RemoveScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<RemoveScheduledJobCommand>(command => command.ScheduledJobId == request.ScheduledJobId),
            Arg.Is(cancellationToken));
    }
}
