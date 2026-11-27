#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.Scheduling.Queries.GetSchedulerDisplayPreferences;
using Nemera.Contracts.Fixtures.Core.Responses.Scheduling;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Core.Endpoints.Scheduling.GetSchedulerDisplayPreferences;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
#endregion

namespace Nemera.Presentation.Api.UnitTests.Core.Endpoints.Scheduling.GetSchedulerDisplayPreferences;

/// <summary>
/// Contains unit tests for the <see cref="GetSchedulerDisplayPreferencesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetSchedulerDisplayPreferencesEndpointTests
{
    private readonly IQueryHandler<GetSchedulerDisplayPreferencesQuery, Result<SchedulerDisplayPreferencesResponse>> _mockHandler;
    private readonly GetSchedulerDisplayPreferencesEndpoint _sut;
    private readonly SchedulerDisplayPreferencesResponseFixture _schedulerDisplayPreferencesResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesEndpointTests"/> class.
    /// </summary>
    public GetSchedulerDisplayPreferencesEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetSchedulerDisplayPreferencesQuery, Result<SchedulerDisplayPreferencesResponse>>>();
        _sut = Factory.Create<GetSchedulerDisplayPreferencesEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithDisplayPreferencesResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        SchedulerDisplayPreferencesResponse expectedResponse = _schedulerDisplayPreferencesResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetSchedulerDisplayPreferencesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<SchedulerDisplayPreferencesResponse> okResult = Assert.IsType<Ok<SchedulerDisplayPreferencesResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsUnauthorizedError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetSchedulerDisplayPreferencesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendQueryToSender()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetSchedulerDisplayPreferencesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_schedulerDisplayPreferencesResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Any<GetSchedulerDisplayPreferencesQuery>(),
            Arg.Is(cancellationToken));
    }
}
