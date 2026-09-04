#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.Scheduling.Queries.GetScheduledJobHistory;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Core.Scheduling.Queries.GetScheduledJobHistory;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Queries.GetScheduledJobHistory;

/// <summary>
/// Contains unit tests for the <see cref="GetScheduledJobHistoryQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IScheduledJobExecutionRepository _mockScheduledJobExecutionRepository;
    private readonly GetScheduledJobHistoryQueryHandler _sut;
    private readonly GetScheduledJobHistoryQueryFixture _getScheduledJobHistoryQueryFixture = new();
    private readonly ScheduledJobExecutionEntityFixture _scheduledJobExecutionEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobHistoryQueryHandlerTests"/> class.
    /// </summary>
    public GetScheduledJobHistoryQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockScheduledJobExecutionRepository = Substitute.For<IScheduledJobExecutionRepository>();

        _mockUnitOfWork.ScheduledJobExecutionRepository.Returns(_mockScheduledJobExecutionRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _sut = new GetScheduledJobHistoryQueryHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenExecutionsExistInTheRequestedRange_ShouldReturnThemAsResponses()
    {
        // Arrange
        GetScheduledJobHistoryQuery query = _getScheduledJobHistoryQueryFixture.Create();
        ScheduledJobExecutionEntity execution1 = _scheduledJobExecutionEntityFixture.Create(startedOnUtc: query.From);
        ScheduledJobExecutionEntity execution2 = _scheduledJobExecutionEntityFixture.Create(startedOnUtc: query.From!.Value.AddMinutes(1));
        _mockScheduledJobExecutionRepository.GetByTimeRangeAsync(query.From!.Value, query.To!.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ScheduledJobExecutionEntity>>([execution1, execution2]));

        // Act
        Result<IEnumerable<ScheduledJobExecutionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        Assert.Contains(result.Value, execution => execution.Id == execution1.Id);
        Assert.Contains(result.Value, execution => execution.Id == execution2.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenNoExecutionsExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        GetScheduledJobHistoryQuery query = _getScheduledJobHistoryQueryFixture.Create();
        _mockScheduledJobExecutionRepository.GetByTimeRangeAsync(query.From!.Value, query.To!.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ScheduledJobExecutionEntity>>([]));

        // Act
        Result<IEnumerable<ScheduledJobExecutionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenBoundsAreNotProvided_ShouldDefaultToThePastDay()
    {
        // Arrange
        GetScheduledJobHistoryQuery query = _getScheduledJobHistoryQueryFixture.Create(includeFrom: false, includeTo: false);
        DateTime beforeCall = DateTime.UtcNow;
        _mockScheduledJobExecutionRepository.GetByTimeRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<ScheduledJobExecutionEntity>>([]));

        // Act
        Result<IEnumerable<ScheduledJobExecutionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);
        DateTime afterCall = DateTime.UtcNow;

        // Assert
        Assert.False(result.IsFailure);
        await _mockScheduledJobExecutionRepository.Received(1).GetByTimeRangeAsync(
            Arg.Is<DateTime>(fromUtc => fromUtc <= afterCall && fromUtc >= beforeCall.AddDays(-1)),
            Arg.Is<DateTime>(toUtc => toUtc >= beforeCall && toUtc <= afterCall),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorized()
    {
        // Arrange
        GetScheduledJobHistoryQuery query = _getScheduledJobHistoryQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IEnumerable<ScheduledJobExecutionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobExecutionRepository.DidNotReceive().GetByTimeRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdministrator_ShouldReturnNotAuthorized()
    {
        // Arrange
        GetScheduledJobHistoryQuery query = _getScheduledJobHistoryQueryFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<IEnumerable<ScheduledJobExecutionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobExecutionRepository.DidNotReceive().GetByTimeRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByTimeRangeReturnsError_ShouldReturnError()
    {
        // Arrange
        GetScheduledJobHistoryQuery query = _getScheduledJobHistoryQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get the execution history");
        _mockScheduledJobExecutionRepository.GetByTimeRangeAsync(query.From!.Value, query.To!.Value, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IEnumerable<ScheduledJobExecutionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
