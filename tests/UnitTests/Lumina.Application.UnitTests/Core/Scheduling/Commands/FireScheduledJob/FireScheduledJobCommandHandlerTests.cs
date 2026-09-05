#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Scheduling.Commands.FireScheduledJob;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.FireScheduledJob;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.FireScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="FireScheduledJobCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly IValidator<FireScheduledJobCommand> _mockValidator;
    private readonly FireScheduledJobCommandHandler _sut;
    private readonly FireScheduledJobCommandFixture _fireScheduledJobCommandFixture = new();
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="FireScheduledJobCommandHandlerTests"/> class.
    /// </summary>
    public FireScheduledJobCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _mockValidator = Substitute.For<IValidator<FireScheduledJobCommand>>();
        _mockValidator.Validate(Arg.Any<FireScheduledJobCommand>()).Returns([]);

        _sut = new FireScheduledJobCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobIsAdded_ShouldEnqueueFiredEventAndReturnResponse()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(scheduledJob.Name, result.Value.Name);
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is ScheduledJobFiredDomainEvent));
        await _mockScheduledJobRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotQueueEvent()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<FireScheduledJobCommand>()).Returns([DomainErrors.Scheduling.ScheduledJobNotFound]);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotFound, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorized()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdministrator_ShouldReturnNotAuthorized()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdReturnsError_ShouldReturnError()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get the scheduled job");
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create();
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(null));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotFound, result.FirstError);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenToDomainEntityFails_ShouldReturnError()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create();
        // A daily scheduled job with an out of range hour cannot convert to its domain aggregate.
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: 99,
            minute: 0);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobIsRunning_ShouldReturnAlreadyRunningError()
    {
        // Arrange
        FireScheduledJobCommand command = _fireScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Running);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobAlreadyRunning, result.FirstError);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }
}
