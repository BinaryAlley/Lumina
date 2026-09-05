#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.Scheduling.Commands.StartScheduledJob;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Fixtures.Core.Responses.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Scheduling.StartScheduledJob;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Scheduling.StartScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="StartScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobEndpointTests
{
    private readonly Application.Common.CQRS.ICommandHandler<StartScheduledJobCommand, Result<ScheduledJobResponse>> _mockHandler;
    private readonly StartScheduledJobEndpoint _sut;
    private readonly StartScheduledJobRequestFixture _startScheduledJobRequestFixture = new();
    private readonly ScheduledJobResponseFixture _scheduledJobResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartScheduledJobEndpointTests"/> class.
    /// </summary>
    public StartScheduledJobEndpointTests()
    {
        _mockHandler = Substitute.For<Application.Common.CQRS.ICommandHandler<StartScheduledJobCommand, Result<ScheduledJobResponse>>>();
        _sut = Factory.Create<StartScheduledJobEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithScheduledJobResponse()
    {
        // Arrange
        StartScheduledJobRequest request = _startScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ScheduledJobResponse expectedResponse = _scheduledJobResponseFixture.Create(
            id: request.ScheduledJobId,
            status: ScheduledJobStatus.Active);
        _mockHandler.HandleAsync(Arg.Any<StartScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ScheduledJobResponse> okResult = Assert.IsType<Ok<ScheduledJobResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        StartScheduledJobRequest request = _startScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Errors.Scheduling.ScheduledJobNotFound;
        _mockHandler.HandleAsync(Arg.Any<StartScheduledJobCommand>(), Arg.Any<CancellationToken>())
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
        StartScheduledJobRequest request = _startScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<StartScheduledJobCommand>(), Arg.Any<CancellationToken>())
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
        StartScheduledJobRequest request = _startScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<StartScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_scheduledJobResponseFixture.Create(id: request.ScheduledJobId)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<StartScheduledJobCommand>(command => command.ScheduledJobId == request.ScheduledJobId),
            Arg.Is(cancellationToken));
    }
}
