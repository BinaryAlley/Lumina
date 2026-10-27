#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Commands.ReportConsumptionProgress;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.ReportConsumptionProgress;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.ReportConsumptionProgress;

/// <summary>
/// Contains unit tests for the <see cref="ReportConsumptionProgressCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IMediaConsumptionRecordRepository _mockMediaConsumptionRecordRepository;
    private readonly IMediaResumePointRepository _mockMediaResumePointRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IValidator<ReportConsumptionProgressCommand> _mockValidator;
    private readonly ReportConsumptionProgressCommandHandler _sut;
    private readonly ReportConsumptionProgressCommandFixture _reportConsumptionProgressCommandFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly MediaResumePointEntityFixture _mediaResumePointEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportConsumptionProgressCommandHandlerTests"/> class.
    /// </summary>
    public ReportConsumptionProgressCommandHandlerTests()
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
        _mockMediaConsumptionRecordRepository.UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _mockValidator = Substitute.For<IValidator<ReportConsumptionProgressCommand>>();
        _mockValidator.Validate(Arg.Any<ReportConsumptionProgressCommand>()).Returns([]);

        _sut = new ReportConsumptionProgressCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockDomainEventsQueue, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadRecord()
    {
        // Arrange
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<ReportConsumptionProgressCommand>())
            .Returns([DomainErrors.MediaConsumption.InvalidProgressFraction]);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.InvalidProgressFraction, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadRecord()
    {
        // Arrange
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
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
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
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
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
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
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
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
    public async Task HandleAsync_WhenLibraryReadFails_ShouldReturnError()
    {
        // Arrange
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        Error error = Error.Failure("Persistence.Failure", "The library could not be read.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
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
    public async Task HandleAsync_WhenLibraryIsMissingForAMusicTrack_ShouldUseTheDefaultSettingsThatDoNotTrackProgress()
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
            status: ConsumptionStatus.InProgress);
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            progressFraction: 0.5,
            consumedDurationInSeconds: 300);
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
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), _userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsMissingForABook_ShouldUseTheDefaultSettingsThatTrackProgressAndUpsertTheResumePoint()
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
            consecutivePagesRead: 4);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1",
            consecutivePagesRead: 4);
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            positionKind: nameof(MediaPositionKind.Location),
            includePositionTimeOffsetInSeconds: false,
            positionLocation: "chapter-3",
            progressFraction: 0.5,
            consumedDurationInSeconds: 300,
            consecutivePagesRead: 4);
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
        await _mockMediaResumePointRepository.Received(1).UpsertForUserAsync(
            Arg.Is<MediaResumePointEntity>(resumePoint =>
                resumePoint.MediaKind == MediaKind.Book &&
                resumePoint.LastPositionKind == MediaPositionKind.Location &&
                resumePoint.LastPositionLocation == "chapter-3" &&
                resumePoint.LastProgressFraction == 0.5),
            _userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotTrackProgress_ShouldNotReadOrUpsertTheResumePoint()
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
            status: ConsumptionStatus.InProgress);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
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
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenProgressIsTrackedAndTheReportCarriesAPosition_ShouldCreateTheResumePointFromTheReportedValues()
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
            status: ConsumptionStatus.InProgress);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: 600,
            progressFraction: 0.4,
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
                resumePoint.LastPositionTimeOffsetMilliseconds == 600_000 &&
                resumePoint.LastProgressFraction == 0.4),
            _userId,
            Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.Received(1).GetForUserAndMediaItemAsync(
            _userId,
            storedRecord.MediaKind,
            storedRecord.MediaItemId,
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenProgressIsTrackedAndTheReportCarriesNoPosition_ShouldReuseTheStoredPosition()
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
            status: ConsumptionStatus.InProgress);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 0.4,
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
                resumePoint.LastProgressFraction == 0.4),
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
            status: ConsumptionStatus.InProgress,
            includeLastPositionKind: false);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            includePositionKind: false,
            progressFraction: 0.4,
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
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().UpsertForUserAsync(Arg.Any<MediaResumePointEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheResumePointAlreadyExists_ShouldAdvanceItInsteadOfReplacingIt()
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
            status: ConsumptionStatus.InProgress);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        MediaResumePointEntity existingResumePoint = _mediaResumePointEntityFixture.Create(
            userId: _userId,
            libraryId: storedRecord.LibraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: storedRecord.MediaItemId,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 1_000,
            lastProgressFraction: 0.1);
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: 600,
            progressFraction: 0.4,
            consumedDurationInSeconds: 300);
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.From<MediaConsumptionRecordEntity?>(storedRecord),
                Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaResumePointEntity?>(existingResumePoint));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaResumePointRepository.Received(1).UpsertForUserAsync(
            Arg.Is<MediaResumePointEntity>(resumePoint =>
                resumePoint.Id == existingResumePoint.Id &&
                resumePoint.CreatedOnUtc == existingResumePoint.CreatedOnUtc &&
                resumePoint.LastPositionTimeOffsetMilliseconds == 600_000 &&
                resumePoint.LastProgressFraction == 0.4),
            _userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenResumePointReadFails_ShouldReturnErrorAndNotUpdateTheRecord()
    {
        // Arrange
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        Error error = Error.Failure("Persistence.Failure", "The resume point could not be read.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(storedRecord));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
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
    public async Task HandleAsync_WhenRecordIsNotInProgress_ShouldReturnConsumptionIsNotInProgressAndNotPersist()
    {
        // Arrange
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Completed,
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
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionIsNotInProgress, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().UpdateForUserAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenResumePointUpsertFails_ShouldReturnErrorAndNotUpdateTheRecord()
    {
        // Arrange
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
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
    public async Task HandleAsync_WhenRecordUpdateFails_ShouldReturnErrorAndNotSaveChanges()
    {
        // Arrange
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
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
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFails_ShouldReturnErrorAndNotEnqueueEvents()
    {
        // Arrange
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            consumedDurationMilliseconds: 0);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
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
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReportProgressAndReturnPersistedResponse()
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
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 600_000,
            lastProgressFraction: 0.66,
            consumedDurationMilliseconds: 300_000);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: false);
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
            consumptionRecordId: recordId.ToString(),
            positionKind: nameof(MediaPositionKind.TimeOffset),
            positionTimeOffsetInSeconds: 600,
            progressFraction: 0.6,
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
        Assert.Equal(persistedRecord.Id, result.Value.Id);
        Assert.Equal(0.66, result.Value.LastProgressFraction);
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(
            Arg.Is<MediaConsumptionRecordEntity>(entity =>
                entity.Status == ConsumptionStatus.InProgress &&
                entity.LastProgressFraction == 0.6 &&
                entity.ConsumedDurationMilliseconds == 300_000 &&
                entity.LastPositionKind == MediaPositionKind.TimeOffset &&
                entity.LastPositionTimeOffsetMilliseconds == 600_000 &&
                !entity.EndedOnUtc.HasValue),
            _userId,
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionProgressedDomainEvent));
        await _mockMediaConsumptionRecordRepository.Received(2).GetByIdForUserAsync(recordId, _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenProgressFractionIsOne_ShouldKeepTheRecordInProgress()
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
            status: ConsumptionStatus.InProgress);
        LibraryEntity library = _libraryEntityFixture.Create(
            id: storedRecord.LibraryId,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
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
        await _mockMediaConsumptionRecordRepository.Received(1).UpdateForUserAsync(
            Arg.Is<MediaConsumptionRecordEntity>(entity => entity.Status == ConsumptionStatus.InProgress && !entity.EndedOnUtc.HasValue),
            _userId,
            Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionProgressedDomainEvent));
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionCompletedDomainEvent));
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
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
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
        ReportConsumptionProgressCommand command = _reportConsumptionProgressCommandFixture.Create(
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
