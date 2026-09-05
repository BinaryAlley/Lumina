#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Scheduling;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Scheduling.Commands.RemoveScheduledJob;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Fixtures.Core.Scheduling.Commands.RemoveScheduledJob;
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
using ScheduledJobId = Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects.ScheduledJobId;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Commands.RemoveScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="RemoveScheduledJobCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IScheduledJobScheduler _mockScheduledJobScheduler;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly IScheduledJobExecutionRepository _mockScheduledJobExecutionRepository;
    private readonly IValidator<RemoveScheduledJobCommand> _mockValidator;
    private readonly RemoveScheduledJobCommandHandler _sut;
    private readonly RemoveScheduledJobCommandFixture _removeScheduledJobCommandFixture = new();
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveScheduledJobCommandHandlerTests"/> class.
    /// </summary>
    public RemoveScheduledJobCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockScheduledJobScheduler = Substitute.For<IScheduledJobScheduler>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();
        _mockScheduledJobExecutionRepository = Substitute.For<IScheduledJobExecutionRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockUnitOfWork.ScheduledJobExecutionRepository.Returns(_mockScheduledJobExecutionRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _mockValidator = Substitute.For<IValidator<RemoveScheduledJobCommand>>();
        _mockValidator.Validate(Arg.Any<RemoveScheduledJobCommand>()).Returns([]);

        _sut = new RemoveScheduledJobCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockScheduledJobScheduler, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobExists_ShouldDeleteItAndEnqueueRemovedEvent()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        _mockScheduledJobExecutionRepository.DeleteByScheduledJobIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);
        _mockScheduledJobRepository.DeleteByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        await _mockScheduledJobScheduler.Received(1).StopCycleAsync(
            Arg.Is<ScheduledJobId>(scheduledJobId => scheduledJobId.Value == command.ScheduledJobId),
            Arg.Any<CancellationToken>());
        await _mockScheduledJobExecutionRepository.Received(1).DeleteByScheduledJobIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>());
        await _mockScheduledJobRepository.Received(1).DeleteByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is ScheduledJobRemovedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotDelete()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<RemoveScheduledJobCommand>()).Returns([DomainErrors.Scheduling.ScheduledJobNotFound]);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotFound, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockScheduledJobRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorized()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdministrator_ShouldReturnNotAuthorized()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(null));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.ScheduledJobNotFound, result.FirstError);
        await _mockScheduledJobScheduler.DidNotReceive().StopCycleAsync(Arg.Any<ScheduledJobId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteExecutionsFails_ShouldReturnErrorAndNotDeleteScheduledJob()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        Error error = Error.Failure("Database.Error", "Failed to delete the executions");
        _mockScheduledJobExecutionRepository.DeleteByScheduledJobIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockScheduledJobRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteScheduledJobFails_ShouldReturnErrorAndNotSaveChanges()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        _mockScheduledJobExecutionRepository.DeleteByScheduledJobIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);
        _mockScheduledJobRepository.DeleteByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Scheduling.ScheduledJobNotFound);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

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
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();
        Error repositoryError = DomainErrors.Scheduling.ScheduledJobNotFound;
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(repositoryError);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(repositoryError, result.FirstError);
        await _mockScheduledJobScheduler.DidNotReceive().StopCycleAsync(Arg.Any<ScheduledJobId>(), Arg.Any<CancellationToken>());
        await _mockScheduledJobExecutionRepository.DidNotReceive().DeleteByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheScheduledJobCannotBeConvertedToDomain_ShouldReturnTheMappingError()
    {
        // Arrange
        RemoveScheduledJobCommand command = _removeScheduledJobCommandFixture.Create();
        // An interval schedule whose interval is not positive cannot be converted to its domain object.
        ScheduledJobEntity invalidScheduledJob = _scheduledJobEntityFixture.Create(
            id: command.ScheduledJobId,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 0,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(invalidScheduledJob));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Scheduling.IntervalMinutesMustBePositive, result.FirstError);
        await _mockScheduledJobScheduler.DidNotReceive().StopCycleAsync(Arg.Any<ScheduledJobId>(), Arg.Any<CancellationToken>());
        await _mockScheduledJobExecutionRepository.DidNotReceive().DeleteByScheduledJobIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
