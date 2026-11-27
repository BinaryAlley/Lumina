#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Commands.CompleteConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.CompleteConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.CompleteConsumption;

/// <summary>
/// Contains unit tests for the <see cref="CompleteConsumptionCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompleteConsumptionCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IMediaConsumptionRecordRepository _mockMediaConsumptionRecordRepository;
    private readonly IMediaResumePointRepository _mockMediaResumePointRepository;
    private readonly IValidator<CompleteConsumptionCommand> _mockValidator;
    private readonly CompleteConsumptionCommandHandler _sut;
    private readonly CompleteConsumptionCommandFixture _completeConsumptionCommandFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteConsumptionCommandHandlerTests"/> class.
    /// </summary>
    public CompleteConsumptionCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockMediaConsumptionRecordRepository = Substitute.For<IMediaConsumptionRecordRepository>();
        _mockMediaResumePointRepository = Substitute.For<IMediaResumePointRepository>();

        _mockUnitOfWork.MediaConsumptionRecordRepository.Returns(_mockMediaConsumptionRecordRepository);
        _mockUnitOfWork.MediaResumePointRepository.Returns(_mockMediaResumePointRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockMediaResumePointRepository.DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockMediaConsumptionRecordRepository.UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _mockValidator = Substitute.For<IValidator<CompleteConsumptionCommand>>();
        _mockValidator.Validate(Arg.Any<CompleteConsumptionCommand>()).Returns([]);

        _sut = new CompleteConsumptionCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockDomainEventsQueue, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadRecord()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<CompleteConsumptionCommand>())
            .Returns([DomainErrors.MediaConsumption.InvalidProgressFraction]);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.InvalidProgressFraction, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadRecord()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRecordDoesNotExist_ShouldReturnConsumptionRecordNotFound()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create();
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(null));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionRecordNotFound, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRecordReadFails_ShouldReturnError()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption record could not be read.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenStoredPositionDoesNotMatchTheMediaKind_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.InvalidMediaPosition, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRecordIsAlreadyTerminal_ShouldReturnConsumptionIsAlreadyTerminalAndNotPersist()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(progressFraction: 1, consumedDurationInSeconds: 120);
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 0);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionIsAlreadyTerminal, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldCompleteRecordDeleteResumePointAndReturnPersistedResponse()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 10_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 30_000,
            lastProgressFraction: 0.2,
            consecutivePagesRead: 0);
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 0.8,
            consumedDurationInSeconds: 120,
            includeConsecutivePagesRead: false);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 120_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 30_000,
            lastProgressFraction: 0.8,
            consecutivePagesRead: 0);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(persistedRecord.Id, result.Value.Id);
        Assert.Equal(ConsumptionStatus.Completed, result.Value.Status);
        Assert.Equal(0.8, result.Value.LastProgressFraction);
        await _mockMediaResumePointRepository.Received(1).DeleteForUserAndMediaItemAsync(
            _userId,
            storedRecord.MediaKind,
            storedRecord.MediaItemId,
            Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(
            Arg.Is<MediaConsumptionRecordEntity>(entity =>
                entity.Status == ConsumptionStatus.Completed &&
                entity.EndedOnUtc.HasValue &&
                entity.ConsumedDurationMilliseconds == 120_000 &&
                entity.LastProgressFraction == 0.8 &&
                entity.LastPositionKind == MediaPositionKind.TimeOffset &&
                entity.LastPositionTimeOffsetMilliseconds == 30_000 &&
                entity.ConsecutivePagesRead == 0),
            _userId,
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionCompletedDomainEvent));
        await _mockMediaConsumptionRecordRepository.Received(2).GetByIdForUserAsync(recordId, _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenConsecutivePagesReadIsSupplied_ShouldOverrideTheStoredCount()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1",
            consecutivePagesRead: 3);
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 0.9,
            consumedDurationInSeconds: 60,
            consecutivePagesRead: 7);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.Completed,
            consecutivePagesRead: 7,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(
            Arg.Is<MediaConsumptionRecordEntity>(entity => entity.ConsecutivePagesRead == 7),
            _userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenResumePointDeleteFails_ShouldReturnErrorAndNotUpdateOrSave()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consumedDurationInSeconds: 120);
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        Error error = Error.Failure("Persistence.Failure", "The resume point could not be deleted.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockMediaResumePointRepository.DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldReturnErrorAndNotSaveChanges()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consumedDurationInSeconds: 120);
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        Error error = Error.Failure("Persistence.Failure", "The consumption record could not be updated.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockMediaConsumptionRecordRepository.UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFailsWithoutConcurrencyConflict_ShouldReturnErrorAndNotRetry()
    {
        // Arrange
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(consumedDurationInSeconds: 120);
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockUnitOfWork.DidNotReceive().ClearTrackedEntities();
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenFirstSaveConflicts_ShouldClearAndRetryOnTheFreshRecord()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity firstStoredRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1",
            consecutivePagesRead: 2);
        MediaConsumptionRecordEntity freshStoredRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1",
            consecutivePagesRead: 5);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.Completed,
            consecutivePagesRead: 5,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1");
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 0.9,
            consumedDurationInSeconds: 60,
            includeConsecutivePagesRead: false);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(firstStoredRecord),
                Result.From<MediaConsumptionRecordEntity?>(freshStoredRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.ConcurrencyConflict, Result.Success);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ConsumptionStatus.Completed, result.Value.Status);
        _mockUnitOfWork.Received(1).ClearTrackedEntities();
        await _mockUnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(2).UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), _userId, Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(
            Arg.Is<MediaConsumptionRecordEntity>(entity => entity.Status == ConsumptionStatus.Completed && entity.ConsecutivePagesRead == 5),
            _userId,
            Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(2).GetByIdForUserAsync(recordId, _userId, Arg.Is<bool>(shouldTrack => shouldTrack), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).GetByIdForUserAsync(recordId, _userId, Arg.Is<bool>(shouldTrack => !shouldTrack), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionCompletedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenAllAttemptsConflict_ShouldReturnConcurrencyConflictAndNotEnqueueEvents()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity firstStoredRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        MediaConsumptionRecordEntity secondStoredRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        MediaConsumptionRecordEntity thirdStoredRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            consumedDurationInSeconds: 120);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(firstStoredRecord),
                Result.From<MediaConsumptionRecordEntity?>(secondStoredRecord),
                Result.From<MediaConsumptionRecordEntity?>(thirdStoredRecord));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(ApplicationErrors.Persistence.ConcurrencyConflict);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.ConcurrencyConflict, result.FirstError);
        await _mockUnitOfWork.Received(3).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockUnitOfWork.Received(2).ClearTrackedEntities();
        await _mockMediaConsumptionRecordRepository.Received(3).GetByIdForUserAsync(recordId, _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenPersistedRecordReadFails_ShouldReturnError()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            consumedDurationInSeconds: 120);
        Error error = Error.Failure("Persistence.Failure", "The persisted record could not be read.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenPersistedRecordIsMissing_ShouldReturnConsumptionRecordNotFound()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        CompleteConsumptionCommand command = _completeConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            consumedDurationInSeconds: 120);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(null));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionRecordNotFound, result.FirstError);
    }
}
