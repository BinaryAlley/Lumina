#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.Scheduling.Queries.GetScheduledJobHistory;
using Nemera.Contracts.Fixtures.Core.Requests.Scheduling;
using Nemera.Contracts.Fixtures.Core.Responses.Scheduling;
using Nemera.Contracts.Requests.Scheduling;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Core.Endpoints.Scheduling.GetScheduledJobHistory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
#endregion

namespace Nemera.Presentation.Api.UnitTests.Core.Endpoints.Scheduling.GetScheduledJobHistory;

/// <summary>
/// Contains unit tests for the <see cref="GetScheduledJobHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryEndpointTests
{
    private readonly IQueryHandler<GetScheduledJobHistoryQuery, Result<IEnumerable<ScheduledJobExecutionResponse>>> _mockHandler;
    private readonly GetScheduledJobHistoryEndpoint _sut;
    private readonly GetScheduledJobHistoryRequestFixture _getScheduledJobHistoryRequestFixture = new();
    private readonly ScheduledJobExecutionResponseFixture _scheduledJobExecutionResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobHistoryEndpointTests"/> class.
    /// </summary>
    public GetScheduledJobHistoryEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetScheduledJobHistoryQuery, Result<IEnumerable<ScheduledJobExecutionResponse>>>>();
        _sut = Factory.Create<GetScheduledJobHistoryEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithExecutionResponses()
    {
        // Arrange
        GetScheduledJobHistoryRequest request = _getScheduledJobHistoryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<ScheduledJobExecutionResponse> expectedResponses = _scheduledJobExecutionResponseFixture.CreateMany(2);
        _mockHandler.HandleAsync(Arg.Any<GetScheduledJobHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ScheduledJobExecutionResponse>>(expectedResponses));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<IEnumerable<ScheduledJobExecutionResponse>> okResult = Assert.IsType<Ok<IEnumerable<ScheduledJobExecutionResponse>>>(result);
        Assert.Equal(2, okResult.Value.Count());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsUnauthorizedError_ShouldReturnProblemResult()
    {
        // Arrange
        GetScheduledJobHistoryRequest request = _getScheduledJobHistoryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetScheduledJobHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendMappedQueryToSender()
    {
        // Arrange
        GetScheduledJobHistoryRequest request = _getScheduledJobHistoryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetScheduledJobHistoryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ScheduledJobExecutionResponse>>([]));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetScheduledJobHistoryQuery>(query => query.From == request.From && query.To == request.To),
            Arg.Is(cancellationToken));
    }
}
