#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.Scheduling.Queries.GetSchedulerDisplayPreferences;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Queries.GetSchedulerDisplayPreferences;

/// <summary>
/// Contains unit tests for the <see cref="GetSchedulerDisplayPreferencesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetSchedulerDisplayPreferencesQueryHandlerTests
{
    private const int DEFAULT_DISPLAY_TIME_SPAN = 10;
    private const SchedulerDisplayTimeUnit DEFAULT_DISPLAY_TIME_UNIT = SchedulerDisplayTimeUnit.Minutes;

    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ISchedulerDisplayPreferencesRepository _mockSchedulerDisplayPreferencesRepository;
    private readonly GetSchedulerDisplayPreferencesQueryHandler _sut;
    private readonly SchedulerDisplayPreferencesEntityFixture _schedulerDisplayPreferencesEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesQueryHandlerTests"/> class.
    /// </summary>
    public GetSchedulerDisplayPreferencesQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockSchedulerDisplayPreferencesRepository = Substitute.For<ISchedulerDisplayPreferencesRepository>();

        _mockUnitOfWork.SchedulerDisplayPreferencesRepository.Returns(_mockSchedulerDisplayPreferencesRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _sut = new GetSchedulerDisplayPreferencesQueryHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenDisplayPreferencesExist_ShouldReturnThemAsResponse()
    {
        // Arrange
        SchedulerDisplayPreferencesEntity displayPreferences = _schedulerDisplayPreferencesEntityFixture.Create(
            userId: _userId,
            jobTypeFilter: ScheduledTaskType.ScanMediaLibraries,
            displayTimeSpan: 60,
            displayTimeUnit: SchedulerDisplayTimeUnit.Hours);
        _mockSchedulerDisplayPreferencesRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<SchedulerDisplayPreferencesEntity?>(displayPreferences));

        // Act
        Result<SchedulerDisplayPreferencesResponse> result = await _sut.HandleAsync(new GetSchedulerDisplayPreferencesQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(_userId, result.Value.UserId);
        Assert.Equal(ScheduledTaskType.ScanMediaLibraries, result.Value.JobTypeFilter);
        Assert.Equal(60, result.Value.DisplayTimeSpan);
        Assert.Equal(SchedulerDisplayTimeUnit.Hours, result.Value.DisplayTimeUnit);
    }

    [Fact]
    public async Task HandleAsync_WhenNoDisplayPreferencesExist_ShouldReturnDefaultResponse()
    {
        // Arrange
        _mockSchedulerDisplayPreferencesRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(Result.From<SchedulerDisplayPreferencesEntity?>(null));

        // Act
        Result<SchedulerDisplayPreferencesResponse> result = await _sut.HandleAsync(new GetSchedulerDisplayPreferencesQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(_userId, result.Value.UserId);
        Assert.Null(result.Value.JobTypeFilter);
        Assert.Equal(DEFAULT_DISPLAY_TIME_SPAN, result.Value.DisplayTimeSpan);
        Assert.Equal(DEFAULT_DISPLAY_TIME_UNIT, result.Value.DisplayTimeUnit);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorized()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<SchedulerDisplayPreferencesResponse> result = await _sut.HandleAsync(new GetSchedulerDisplayPreferencesQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockSchedulerDisplayPreferencesRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdministrator_ShouldReturnNotAuthorized()
    {
        // Arrange
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<SchedulerDisplayPreferencesResponse> result = await _sut.HandleAsync(new GetSchedulerDisplayPreferencesQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockSchedulerDisplayPreferencesRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByUserIdReturnsError_ShouldReturnError()
    {
        // Arrange
        Error error = Error.Failure("Database.Error", "Failed to get the display preferences");
        _mockSchedulerDisplayPreferencesRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<SchedulerDisplayPreferencesResponse> result = await _sut.HandleAsync(new GetSchedulerDisplayPreferencesQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
