#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Scheduling.UpdateSchedulerDisplayPreferences;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Scheduling.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSchedulerDisplayPreferencesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesEndpointTests
{
    private readonly Application.Common.CQRS.ICommandHandler<UpdateSchedulerDisplayPreferencesCommand, Result<Updated>> _mockHandler;
    private readonly UpdateSchedulerDisplayPreferencesEndpoint _sut;
    private readonly UpdateSchedulerDisplayPreferencesRequestFixture _updateSchedulerDisplayPreferencesRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesEndpointTests"/> class.
    /// </summary>
    public UpdateSchedulerDisplayPreferencesEndpointTests()
    {
        _mockHandler = Substitute.For<Application.Common.CQRS.ICommandHandler<UpdateSchedulerDisplayPreferencesCommand, Result<Updated>>>();
        _sut = Factory.Create<UpdateSchedulerDisplayPreferencesEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithUpdated()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesRequest request = _updateSchedulerDisplayPreferencesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateSchedulerDisplayPreferencesCommand>(), Arg.Any<CancellationToken>())
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
        UpdateSchedulerDisplayPreferencesRequest request = _updateSchedulerDisplayPreferencesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Scheduling.SchedulerDisplayTimeSpanMustBePositive;
        _mockHandler.HandleAsync(Arg.Any<UpdateSchedulerDisplayPreferencesCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorized_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        UpdateSchedulerDisplayPreferencesRequest request = _updateSchedulerDisplayPreferencesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateSchedulerDisplayPreferencesCommand>(), Arg.Any<CancellationToken>())
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
        UpdateSchedulerDisplayPreferencesRequest request = _updateSchedulerDisplayPreferencesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateSchedulerDisplayPreferencesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateSchedulerDisplayPreferencesCommand>(command =>
                command.JobTypeFilter == request.JobTypeFilter &&
                command.DisplayTimeSpan == request.DisplayTimeSpan &&
                command.DisplayTimeUnit == request.DisplayTimeUnit),
            Arg.Is(cancellationToken));
    }
}
