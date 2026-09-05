#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Scheduling;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Scheduling.Commands.StopScheduledJob;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.StopScheduledJob;
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

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.StopScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="StopScheduledJobCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopScheduledJobCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IScheduledJobScheduler _mockScheduledJobScheduler;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly IValidator<StopScheduledJobCommand> _mockValidator;
    private readonly StopScheduledJobCommandHandler _sut;
    private readonly StopScheduledJobCommandFixture _stopScheduledJobCommandFixture = new();
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="StopScheduledJobCommandHandlerTests"/> class.
    /// </summary>
    public StopScheduledJobCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockScheduledJobScheduler = Substitute.For<IScheduledJobScheduler>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _mockValidator = Substitute.For<IValidator<StopScheduledJobCommand>>();
        _mockValidator.Validate(Arg.Any<StopScheduledJobCommand>()).Returns([]);

        _sut = new StopScheduledJobCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockScheduledJobScheduler, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobIsActive_ShouldUpdateToAddedAndStopCycleInScheduler()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ScheduledJobStatus.Added, result.Value.Status);
        await _mockScheduledJobRepository.Received(1).UpdateAsync(
            Arg.Is<ScheduledJobEntity>(updatedScheduledJob => updatedScheduledJob.Status == ScheduledJobStatus.Added),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockScheduledJobScheduler.Received(1).StopCycleAsync(
            Arg.Is<Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects.ScheduledJobId>(scheduledJobId => scheduledJobId.Value == command.ScheduledJobId),
            Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is ScheduledJobCycleStoppedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobIsRunning_ShouldEnqueueExecutionStoppedEvent()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Running,
            lastStartedOnUtc: DateTime.UtcNow.AddMinutes(-10));
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ScheduledJobStatus.Added, result.Value.Status);
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is ScheduledJobExecutionStoppedDomainEvent));
        await _mockScheduledJobScheduler.Received(1).StopCycleAsync(Arg.Any<Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects.ScheduledJobId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotPersist()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<StopScheduledJobCommand>()).Returns([DomainErrors.Scheduling.ScheduledJobNotFound]);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotFound, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockScheduledJobScheduler.DidNotReceive().StopCycleAsync(Arg.Any<Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects.ScheduledJobId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorized()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
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
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(null));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotFound, result.FirstError);
        await _mockScheduledJobScheduler.DidNotReceive().StopCycleAsync(Arg.Any<Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects.ScheduledJobId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobCycleWasNeverStarted_ShouldReturnNotStartedError()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Added);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotStarted, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
        await _mockScheduledJobScheduler.DidNotReceive().StopCycleAsync(Arg.Any<Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects.ScheduledJobId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldReturnErrorAndNotStopCycleInScheduler()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Scheduling.ScheduledJobNotFound);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockScheduledJobScheduler.DidNotReceive().StopCycleAsync(Arg.Any<Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects.ScheduledJobId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGettingTheScheduledJobFails_ShouldReturnTheRepositoryError()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
        Error repositoryError = DomainErrors.Scheduling.ScheduledJobNotFound;
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(repositoryError);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(repositoryError, result.FirstError);
        await _mockScheduledJobScheduler.DidNotReceive().StopCycleAsync(Arg.Any<Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects.ScheduledJobId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheScheduledJobCannotBeConvertedToDomain_ShouldReturnTheMappingError()
    {
        // Arrange
        StopScheduledJobCommand command = _stopScheduledJobCommandFixture.Create();
        // An interval schedule whose interval is not positive cannot be converted to its domain object.
        ScheduledJobEntity invalidScheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 0,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(invalidScheduledJob));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.IntervalMinutesMustBePositive, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockScheduledJobScheduler.DidNotReceive().StopCycleAsync(Arg.Any<Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects.ScheduledJobId>(), Arg.Any<CancellationToken>());
    }
}
