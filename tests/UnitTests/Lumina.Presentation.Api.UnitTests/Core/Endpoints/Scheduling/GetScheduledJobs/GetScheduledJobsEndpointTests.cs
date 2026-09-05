#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Scheduling.Queries.GetScheduledJobs;
using Lumina.Contracts.Fixtures.Core.Responses.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Scheduling.GetScheduledJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Scheduling.GetScheduledJobs;

/// <summary>
/// Contains unit tests for the <see cref="GetScheduledJobsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobsEndpointTests
{
    private readonly IQueryHandler<GetScheduledJobsQuery, Result<IEnumerable<ScheduledJobResponse>>> _mockHandler;
    private readonly GetScheduledJobsEndpoint _sut;
    private readonly ScheduledJobResponseFixture _scheduledJobResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobsEndpointTests"/> class.
    /// </summary>
    public GetScheduledJobsEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetScheduledJobsQuery, Result<IEnumerable<ScheduledJobResponse>>>>();
        _sut = Factory.Create<GetScheduledJobsEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithScheduledJobResponses()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        List<ScheduledJobResponse> expectedResponses = _scheduledJobResponseFixture.CreateMany(2);
        _mockHandler.HandleAsync(Arg.Any<GetScheduledJobsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ScheduledJobResponse>>(expectedResponses));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<IEnumerable<ScheduledJobResponse>> okResult = Assert.IsType<Ok<IEnumerable<ScheduledJobResponse>>>(result);
        Assert.Equal(2, okResult.Value.Count());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetScheduledJobsQuery>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendQueryToHandler()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetScheduledJobsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ScheduledJobResponse>>([]));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Any<GetScheduledJobsQuery>(),
            Arg.Is(cancellationToken));
    }
}
