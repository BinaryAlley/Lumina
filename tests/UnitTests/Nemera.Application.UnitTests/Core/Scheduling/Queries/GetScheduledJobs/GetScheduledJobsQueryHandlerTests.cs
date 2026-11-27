#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Scheduling;
using Nemera.Application.Common.DataAccess.Repositories.Scheduling;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Core.Scheduling.Queries.GetScheduledJobs;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Nemera.Application.Fixtures.Common.DTO.Pagination;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.Scheduling.Queries.GetScheduledJobs;

/// <summary>
/// Contains unit tests for the <see cref="GetScheduledJobsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly GetScheduledJobsQueryHandler _sut;
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly PaginatedResultDtoFixture<ScheduledJobEntity> _paginatedResultDtoFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobsQueryHandlerTests"/> class.
    /// </summary>
    public GetScheduledJobsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _sut = new GetScheduledJobsQueryHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobsExist_ShouldReturnAllOfThemAsResponses()
    {
        // Arrange
        ScheduledJobEntity scheduledJob1 = _scheduledJobEntityFixture.Create(name: "Job 1");
        ScheduledJobEntity scheduledJob2 = _scheduledJobEntityFixture.Create(name: "Job 2");
        _mockScheduledJobRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: [scheduledJob1, scheduledJob2], currentPage: 1, perPage: 2, count: 2, numberOfPages: 1)));

        // Act
        Result<IEnumerable<ScheduledJobResponse>> result = await _sut.HandleAsync(new GetScheduledJobsQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        Assert.Contains(result.Value, scheduledJob => scheduledJob.Id == scheduledJob1.Id && scheduledJob.Name == "Job 1");
        Assert.Contains(result.Value, scheduledJob => scheduledJob.Id == scheduledJob2.Id && scheduledJob.Name == "Job 2");
    }

    [Fact]
    public async Task HandleAsync_WhenNoScheduledJobsExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        _mockScheduledJobRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 0, count: 0, numberOfPages: 1)));

        // Act
        Result<IEnumerable<ScheduledJobResponse>> result = await _sut.HandleAsync(new GetScheduledJobsQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorized()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IEnumerable<ScheduledJobResponse>> result = await _sut.HandleAsync(new GetScheduledJobsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdministrator_ShouldReturnNotAuthorized()
    {
        // Arrange
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<IEnumerable<ScheduledJobResponse>> result = await _sut.HandleAsync(new GetScheduledJobsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllReturnsError_ShouldReturnError()
    {
        // Arrange
        Error error = Error.Failure("Database.Error", "Failed to get the scheduled jobs");
        _mockScheduledJobRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<IEnumerable<ScheduledJobResponse>> result = await _sut.HandleAsync(new GetScheduledJobsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
