#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Scheduling.Commands.AddScheduledJob;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.AddScheduledJob;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Errors;
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

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.AddScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="AddScheduledJobCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly IValidator<AddScheduledJobCommand> _mockValidator;
    private readonly AddScheduledJobCommandHandler _sut;
    private readonly AddScheduledJobCommandFixture _addScheduledJobCommandFixture = new();
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobCommandHandlerTests"/> class.
    /// </summary>
    public AddScheduledJobCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _mockValidator = Substitute.For<IValidator<AddScheduledJobCommand>>();
        _mockValidator.Validate(Arg.Any<AddScheduledJobCommand>()).Returns([]);

        _sut = new AddScheduledJobCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValidWithIntervalSchedule_ShouldInsertScheduledJobAndReturnResponse()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: ScheduleType.WithIntervalInMinutes);
        _mockScheduledJobRepository.InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Created);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(command.Name, result.Value.Name);
        Assert.Equal(command.TaskType, result.Value.TaskType);
        Assert.Equal(ScheduleType.WithIntervalInMinutes, result.Value.ScheduleType);
        Assert.Equal(command.IntervalMinutes, result.Value.IntervalMinutes);
        Assert.Equal(ScheduledJobStatus.Added, result.Value.Status);
        await _mockScheduledJobRepository.Received(1).InsertAsync(
            Arg.Is<ScheduledJobEntity>(scheduledJob =>
                scheduledJob.Name == command.Name &&
                scheduledJob.TaskType == command.TaskType &&
                scheduledJob.ScheduleType == ScheduleType.WithIntervalInMinutes &&
                scheduledJob.IntervalMinutes == command.IntervalMinutes &&
                scheduledJob.OwnerUserId == _userId),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is ScheduledJobAddedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValidWithDailySchedule_ShouldInsertScheduledJobAndReturnResponse()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: ScheduleType.DailyAtHourAndMinute, hour: 6, minute: 30);
        _mockScheduledJobRepository.InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Created);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ScheduleType.DailyAtHourAndMinute, result.Value.ScheduleType);
        Assert.Equal(6, result.Value.Hour);
        Assert.Equal(30, result.Value.Minute);
        Assert.Null(result.Value.IntervalMinutes);
        await _mockScheduledJobRepository.Received(1).InsertAsync(
            Arg.Is<ScheduledJobEntity>(scheduledJob =>
                scheduledJob.ScheduleType == ScheduleType.DailyAtHourAndMinute &&
                scheduledJob.Hour == 6 &&
                scheduledJob.Minute == 30),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotPersist()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<AddScheduledJobCommand>()).Returns([DomainErrors.Scheduling.ScheduledJobNameCannotBeEmpty]);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNameCannotBeEmpty, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotPersist()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdministrator_ShouldReturnNotAuthorizedAndNotPersist()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenIntervalScheduleHasNonPositiveInterval_ShouldReturnIntervalError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 0);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.IntervalMinutesMustBePositive, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDailyScheduleHasOutOfRangeHour_ShouldReturnHourError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: 24,
            minute: 0);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.HourMustBeBetweenZeroAndTwentyThree, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduleTypeIsUnsupported_ShouldReturnInvalidScheduleTypeError()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: (ScheduleType)999);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.InvalidScheduleType, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobNameIsEmpty_ShouldReturnNameErrorAndNotPersist()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(name: "   ");

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Scheduling.ScheduledJobNameCannotBeEmpty, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertFails_ShouldReturnErrorAndNotSaveChanges()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create();
        _mockScheduledJobRepository.InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Scheduling.ScheduledJobAlreadyExists);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobAlreadyExists, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValidWithOnceAtStartupSchedule_ShouldInsertScheduledJobAndReturnResponse()
    {
        // Arrange
        AddScheduledJobCommand command = _addScheduledJobCommandFixture.Create(scheduleType: ScheduleType.OnceAtStartup);
        _mockScheduledJobRepository.InsertAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Created);

        // Act
        Result<ScheduledJobResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(command.Name, result.Value.Name);
        Assert.Equal(ScheduleType.OnceAtStartup, result.Value.ScheduleType);
        Assert.Null(result.Value.IntervalMinutes);
        Assert.Null(result.Value.Hour);
        Assert.Null(result.Value.Minute);
        await _mockScheduledJobRepository.Received(1).InsertAsync(
            Arg.Is<ScheduledJobEntity>(scheduledJob =>
                scheduledJob.Name == command.Name &&
                scheduledJob.ScheduleType == ScheduleType.OnceAtStartup),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is ScheduledJobAddedDomainEvent));
    }
}
