#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Commands.StopConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.StopConsumption;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.StopConsumption;

/// <summary>
/// Contains unit tests for the <see cref="StopConsumptionCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopConsumptionCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IMediaConsumptionRecordRepository _mockMediaConsumptionRecordRepository;
    private readonly IMediaResumePointRepository _mockMediaResumePointRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IValidator<StopConsumptionCommand> _mockValidator;
    private readonly StopConsumptionCommandHandler _sut;
    private readonly StopConsumptionCommandFixture _stopConsumptionCommandFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly MediaResumePointEntityFixture _mediaResumePointEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="StopConsumptionCommandHandlerTests"/> class.
    /// </summary>
    public StopConsumptionCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockMediaConsumptionRecordRepository = Substitute.For<IMediaConsumptionRecordRepository>();
        _mockMediaResumePointRepository = Substitute.For<IMediaResumePointRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();

        _mockUnitOfWork.MediaConsumptionRecordRepository.Returns(_mockMediaConsumptionRecordRepository);
        _mockUnitOfWork.MediaResumePointRepository.Returns(_mockMediaResumePointRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaResumePointEntity?>(null));
        _mockMediaResumePointRepository.UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockMediaResumePointRepository.DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockMediaConsumptionRecordRepository.UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _mockValidator = Substitute.For<IValidator<StopConsumptionCommand>>();
        _mockValidator.Validate(Arg.Any<StopConsumptionCommand>()).Returns([]);

        _sut = new StopConsumptionCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockDomainEventsQueue, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadRecord()
    {
        // Arrange
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<StopConsumptionCommand>())
            .Returns([DomainErrors.MediaConsumption.InvalidProgressFraction]);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.InvalidProgressFraction, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadRecord()
    {
        // Arrange
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRecordDoesNotExist_ShouldReturnConsumptionRecordNotFound()
    {
        // Arrange
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create();
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(null));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionRecordNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRecordReadFails_ShouldReturnError()
    {
        // Arrange
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption record could not be read.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenStoredPositionDoesNotMatchTheMediaKind_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create();
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
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsMissingForABook_ShouldUseTheDefaultSettingsThatCompleteAtTheThreshold()
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
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.Completed,
            consecutivePagesRead: 3,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1");
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 1,
            consumedDurationInSeconds: 300,
            includeConsecutivePagesRead: false);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaResumePointRepository.Received(1).DeleteForUserAndMediaItemAsync(
            _userId,
            storedRecord.MediaKind,
            storedRecord.MediaItemId,
            Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(
            Arg.Is<MediaConsumptionRecordEntity>(entity => entity.Status == ConsumptionStatus.Completed && entity.EndedOnUtc.HasValue),
            _userId,
            Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(2).GetByIdForUserAsync(recordId, _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotTrackProgress_ShouldNeverCompleteTheSession()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Stopped);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            progressFraction: 1,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ConsumptionStatus.Stopped, result.Value.Status);
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenProgressIsAtTheCompletionThreshold_ShouldCompleteTheSession()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Completed);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            progressFraction: 0.9,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ConsumptionStatus.Completed, result.Value.Status);
        await _mockMediaResumePointRepository.Received(1).DeleteForUserAndMediaItemAsync(
            _userId,
            storedRecord.MediaKind,
            storedRecord.MediaItemId,
            Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionCompletedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenProgressIsBelowTheCompletionThreshold_ShouldStopTheSessionAndKeepTheResumePoint()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Stopped);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            progressFraction: 0.8999,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ConsumptionStatus.Stopped, result.Value.Status);
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.Received(1).UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), _userId, Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionStoppedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenProgressIsTrackedWithoutACompletionThreshold_ShouldStopTheSessionEvenAtFullProgress()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Stopped);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: null);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            progressFraction: 1,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ConsumptionStatus.Stopped, result.Value.Status);
        await _mockMediaResumePointRepository.Received(1).UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), _userId, Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenProgressIsTrackedAndAPositionIsReported_ShouldUpsertTheReportedPosition()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 10_000);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Stopped);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: 600,
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        MediaResumePointEntity? upsertedResumePoint = null;
        _mockMediaResumePointRepository.UpsertForUserAsync(
            Arg.Do<MediaResumePointEntity>(resumePoint => upsertedResumePoint = resumePoint),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaResumePointRepository.Received(1).UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), _userId, Arg.Any<CancellationToken>());
        Assert.NotNull(upsertedResumePoint);
        Assert.Equal(MediaPositionKind.TimeOffset, upsertedResumePoint!.LastPositionKind);
        Assert.True(upsertedResumePoint.LastPositionTimeOffsetMilliseconds.HasValue);
        Assert.Equal(600_000L, upsertedResumePoint.LastPositionTimeOffsetMilliseconds.Value);
        Assert.Equal(0.5, upsertedResumePoint.LastProgressFraction);
        Assert.True(upsertedResumePoint.LastConsumedOnUtc > storedRecord.LastActivityOnUtc);
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(
            Arg.Is<MediaConsumptionRecordEntity>(entity => entity.LastActivityOnUtc == upsertedResumePoint.LastConsumedOnUtc),
            _userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenProgressIsTrackedAndNoPositionIsReported_ShouldUpsertTheStoredPosition()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 42_000);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Stopped);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaResumePointRepository.Received(1).UpsertForUserAsync(
            Arg.Is<MediaResumePointEntity>(resumePoint =>
                resumePoint.LastPositionKind == MediaPositionKind.TimeOffset &&
                resumePoint.LastPositionTimeOffsetMilliseconds == 42_000 &&
                resumePoint.LastProgressFraction == 0.5),
            _userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenProgressIsTrackedAndNoPositionIsKnown_ShouldNotUpsertTheResumePoint()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0,
            includeLastPositionKind: false);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Stopped,
            includeLastPositionKind: false);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaResumePointRepository.DidNotReceive().UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRecordIsAlreadyTerminal_ShouldReturnConsumptionIsAlreadyTerminalAndNotPersist()
    {
        // Arrange
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(progressFraction: 0.5, consumedDurationInSeconds: 300);
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Stopped,
            consumedDurationMilliseconds: 0);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionIsAlreadyTerminal, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenResumePointDeleteFails_ShouldReturnErrorAndNotUpdateOrSave()
    {
        // Arrange
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            progressFraction: 0.9,
            consumedDurationInSeconds: 300);
        Error error = Error.Failure("Persistence.Failure", "The resume point could not be deleted.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
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
    public async Task HandleAsync_WhenResumePointUpsertFails_ShouldReturnErrorAndNotUpdateOrSave()
    {
        // Arrange
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
        Error error = Error.Failure("Persistence.Failure", "The resume point could not be saved.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockMediaResumePointRepository.UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
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
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
        Error error = Error.Failure("Persistence.Failure", "The consumption record could not be updated.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
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
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
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
            status: ConsumptionStatus.Stopped,
            consecutivePagesRead: 5,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1");
        LibraryEntity library = _libraryEntityFixture.Create(
            id: firstStoredRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 0.5,
            consumedDurationInSeconds: 300,
            includeConsecutivePagesRead: false);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(firstStoredRecord),
                Result.From<MediaConsumptionRecordEntity?>(freshStoredRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.ConcurrencyConflict, Result.Success);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ConsumptionStatus.Stopped, result.Value.Status);
        _mockUnitOfWork.Received(1).ClearTrackedEntities();
        await _mockUnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(
            Arg.Is<MediaConsumptionRecordEntity>(entity => entity.ConsecutivePagesRead == 5),
            _userId,
            Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(2).GetByIdForUserAsync(recordId, _userId, Arg.Is<bool>(shouldTrack => shouldTrack), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).GetByIdForUserAsync(recordId, _userId, Arg.Is<bool>(shouldTrack => !shouldTrack), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionStoppedDomainEvent));
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
        LibraryEntity library = _libraryEntityFixture.Create(
            id: firstStoredRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(firstStoredRecord),
                Result.From<MediaConsumptionRecordEntity?>(secondStoredRecord),
                Result.From<MediaConsumptionRecordEntity?>(thirdStoredRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
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
    public async Task HandleAsync_WhenSuccessful_ShouldStopTheRecordAndReturnPersistedResponse()
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
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.Stopped,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1",
            consecutivePagesRead: 12,
            lastProgressFraction: 0.55);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 0.5,
            consumedDurationInSeconds: 300,
            consecutivePagesRead: 12);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(persistedRecord.Id, result.Value.Id);
        Assert.Equal(ConsumptionStatus.Stopped, result.Value.Status);
        Assert.Equal(0.55, result.Value.LastProgressFraction);
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(
            Arg.Is<MediaConsumptionRecordEntity>(entity =>
                entity.Status == ConsumptionStatus.Stopped &&
                entity.EndedOnUtc.HasValue &&
                entity.ConsumedDurationMilliseconds == 300_000 &&
                entity.ConsecutivePagesRead == 12),
            _userId,
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionStoppedDomainEvent));
        await _mockMediaConsumptionRecordRepository.Received(2).GetByIdForUserAsync(recordId, _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>());
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
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
        Error error = Error.Failure("Persistence.Failure", "The persisted record could not be read.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                error);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
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
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        StopConsumptionCommand command = _stopConsumptionCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(null));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionRecordNotFound, result.FirstError);
    }
}
