#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Scheduling.Commands.StartScheduledJob;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.StartScheduledJob;
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

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.StartScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="StartScheduledJobCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly IValidator<StartScheduledJobCommand> _mockValidator;
    private readonly StartScheduledJobCommandHandler _sut;
    private readonly StartScheduledJobCommandFixture _startScheduledJobCommandFixture = new();
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartScheduledJobCommandHandlerTests"/> class.
    /// </summary>
    public StartScheduledJobCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _mockValidator = Substitute.For<IValidator<StartScheduledJobCommand>>();
        _mockValidator.Validate(Arg.Any<StartScheduledJobCommand>()).Returns([]);

        _sut = new StartScheduledJobCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobIsAdded_ShouldUpdateToActiveAndEnqueueCycleStartedEvent()
    {
        // Arrange
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Added);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ScheduledJobStatus.Active, result.Value.Status);
        await _mockScheduledJobRepository.Received(1).UpdateAsync(
            Arg.Is<ScheduledJobEntity>(updatedScheduledJob => updatedScheduledJob.Status == ScheduledJobStatus.Active),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is ScheduledJobCycleStartedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotPersist()
    {
        // Arrange
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<StartScheduledJobCommand>()).Returns([DomainErrors.Scheduling.ScheduledJobNotFound]);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotFound, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorized()
    {
        // Arrange
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();
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
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();
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
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(null));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotFound, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobCycleAlreadyStarted_ShouldReturnCycleAlreadyStartedError()
    {
        // Arrange
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobCycleAlreadyStarted, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldReturnErrorAndNotSaveChanges()
    {
        // Arrange
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Added);
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
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenGettingTheScheduledJobFails_ShouldReturnTheRepositoryError()
    {
        // Arrange
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();
        Error repositoryError = DomainErrors.Scheduling.ScheduledJobNotFound;
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(repositoryError);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(repositoryError, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheScheduledJobCannotBeConvertedToDomain_ShouldReturnTheMappingError()
    {
        // Arrange
        StartScheduledJobCommand command = _startScheduledJobCommandFixture.Create();
        // An interval schedule whose interval is not positive cannot be converted to its domain object.
        ScheduledJobEntity invalidScheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 0,
            status: ScheduledJobStatus.Added);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(invalidScheduledJob));

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.IntervalMinutesMustBePositive, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }
}
