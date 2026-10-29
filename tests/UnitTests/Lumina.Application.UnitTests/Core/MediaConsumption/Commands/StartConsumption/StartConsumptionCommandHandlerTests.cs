#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Commands.StartConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.StartConsumption;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.StartConsumption;

/// <summary>
/// Contains unit tests for the <see cref="StartConsumptionCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IMediaConsumptionRecordRepository _mockMediaConsumptionRecordRepository;
    private readonly IMediaResumePointRepository _mockMediaResumePointRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IValidator<StartConsumptionCommand> _mockValidator;
    private readonly StartConsumptionCommandHandler _sut;
    private readonly StartConsumptionCommandFixture _startConsumptionCommandFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly MediaResumePointEntityFixture _mediaResumePointEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionCommandHandlerTests"/> class.
    /// </summary>
    public StartConsumptionCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockMediaConsumptionRecordRepository = Substitute.For<IMediaConsumptionRecordRepository>();
        _mockMediaResumePointRepository = Substitute.For<IMediaResumePointRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();

        _mockUnitOfWork.MediaConsumptionRecordRepository.Returns(_mockMediaConsumptionRecordRepository);
        _mockUnitOfWork.MediaResumePointRepository.Returns(_mockMediaResumePointRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaResumePointEntity?>(null));
        _mockMediaConsumptionRecordRepository.InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        _mockValidator = Substitute.For<IValidator<StartConsumptionCommand>>();
        _mockValidator.Validate(Arg.Any<StartConsumptionCommand>()).Returns([]);

        _sut = new StartConsumptionCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockDomainEventsQueue, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotAuthorizeOrRead()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<StartConsumptionCommand>())
            .Returns([DomainErrors.MediaConsumption.UnsupportedMediaKind]);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotAuthorizeOrRead()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryAccessIsDenied_ShouldReturnNotAuthorizedAndNotReadTheLibrary()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMediaKindIsNotSupportedByTheCurrentCatalogues_ShouldReturnUnsupportedMediaKind()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.Photo));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryReadFails_ShouldReturnError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The library could not be read.");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFound()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create();
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackReadFails_ShouldReturnError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        LibraryEntity library = _libraryEntityFixture.Create(id: Guid.Parse(command.LibraryId!));
        Error error = Error.Failure("Persistence.Failure", "The track could not be read.");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackDoesNotExist_ShouldReturnTrackNotFound()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        LibraryEntity library = _libraryEntityFixture.Create(id: Guid.Parse(command.LibraryId!));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(null));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackBelongsToAnotherLibrary_ShouldReturnTrackNotFound()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId);
        TrackEntity track = _trackEntityFixture.Create(id: Guid.Parse(command.MediaItemId!), libraryId: Guid.NewGuid());
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookReadFails_ShouldReturnError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.Book));
        LibraryEntity library = _libraryEntityFixture.Create(id: Guid.Parse(command.LibraryId!));
        Error error = Error.Failure("Persistence.Failure", "The book could not be read.");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnBookNotFound()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.Book));
        LibraryEntity library = _libraryEntityFixture.Create(id: Guid.Parse(command.LibraryId!));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(null));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.BookNotFound, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookBelongsToAnotherLibrary_ShouldReturnBookNotFound()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.Book));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId);
        BookEntity book = _bookEntityFixture.Create(id: Guid.Parse(command.MediaItemId!), libraryId: Guid.NewGuid());
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.BookNotFound, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenResumePointReadFails_ShouldReturnErrorAndNotInsert()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, shouldTrackConsumptionProgress: true, completionThresholdPercentage: 90);
        TrackEntity track = _trackEntityFixture.Create(id: mediaItemId, libraryId: libraryId);
        Error error = Error.Failure("Persistence.Failure", "The resume point could not be read.");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenResumePointCannotBeConverted_ShouldReturnInvalidMediaPositionAndNotInsert()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, shouldTrackConsumptionProgress: true, completionThresholdPercentage: 90);
        TrackEntity track = _trackEntityFixture.Create(id: mediaItemId, libraryId: libraryId);
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: _userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaResumePointEntity?>(resumePoint));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.InvalidMediaPosition, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessfulWithAResumePoint_ShouldStartFromTheStoredPositionAndReturnPersistedResponse()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, shouldTrackConsumptionProgress: true, completionThresholdPercentage: 90);
        TrackEntity track = _trackEntityFixture.Create(id: mediaItemId, libraryId: libraryId);
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: _userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 60_000,
            lastProgressFraction: 0.3);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 60_000,
            lastProgressFraction: 0.3);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaResumePointEntity?>(resumePoint));
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(persistedRecord));
        MediaConsumptionRecordEntity? insertedRecord = null;
        _mockMediaConsumptionRecordRepository.InsertAsync(
            Arg.Do<MediaConsumptionRecordEntity>(record => insertedRecord = record),
            Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(persistedRecord.Id, result.Value.Id);
        Assert.Equal(ConsumptionStatus.InProgress, result.Value.Status);
        await _mockMediaConsumptionRecordRepository.Received(1).InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>());
        Assert.NotNull(insertedRecord);
        Assert.Equal(_userId, insertedRecord!.UserId);
        Assert.Equal(libraryId, insertedRecord.LibraryId);
        Assert.Equal(mediaItemId, insertedRecord.MediaItemId);
        Assert.Equal(MediaKind.MusicTrack, insertedRecord.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, insertedRecord.Status);
        Assert.Equal(0L, insertedRecord.ConsumedDurationMilliseconds);
        Assert.True(insertedRecord.LastPositionKind.HasValue);
        Assert.Equal(MediaPositionKind.TimeOffset, insertedRecord.LastPositionKind.Value);
        Assert.True(insertedRecord.LastPositionTimeOffsetMilliseconds.HasValue);
        Assert.Equal(60_000L, insertedRecord.LastPositionTimeOffsetMilliseconds.Value);
        Assert.Equal(0.3, insertedRecord.LastProgressFraction);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is MediaConsumptionStartedDomainEvent));
        await _mockMediaConsumptionRecordRepository.Received(1).GetByIdForUserAsync(
            insertedRecord.Id,
            _userId,
            Arg.Is<bool>(shouldTrack => !shouldTrack),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessfulWithoutTracking_ShouldNotReadTheResumePointAndReturnPersistedResponse()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.Book));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, shouldTrackConsumptionProgress: false);
        BookEntity book = _bookEntityFixture.Create(id: mediaItemId, libraryId: libraryId);
        MediaConsumptionRecordEntity persistedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Book,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.InProgress,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-1");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(persistedRecord));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(persistedRecord.Id, result.Value.Id);
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).InsertAsync(
            Arg.Is<MediaConsumptionRecordEntity>(record => record.MediaKind == MediaKind.Book && record.LastProgressFraction == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertFails_ShouldReturnErrorAndNotSaveChanges()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, shouldTrackConsumptionProgress: false);
        TrackEntity track = _trackEntityFixture.Create(id: mediaItemId, libraryId: libraryId);
        Error error = Error.Failure("Persistence.Failure", "The record could not be inserted.");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMediaConsumptionRecordRepository.InsertAsync(Arg.Any<MediaConsumptionRecordEntity>(), Arg.Any<CancellationToken>())
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
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, shouldTrackConsumptionProgress: false);
        TrackEntity track = _trackEntityFixture.Create(id: mediaItemId, libraryId: libraryId);
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenPersistedRecordReadFails_ShouldReturnError()
    {
        // Arrange
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, shouldTrackConsumptionProgress: false);
        TrackEntity track = _trackEntityFixture.Create(id: mediaItemId, libraryId: libraryId);
        Error error = Error.Failure("Persistence.Failure", "The persisted record could not be read.");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

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
        StartConsumptionCommand command = _startConsumptionCommandFixture.Create(mediaKind: nameof(MediaKind.MusicTrack));
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, shouldTrackConsumptionProgress: false);
        TrackEntity track = _trackEntityFixture.Create(id: mediaItemId, libraryId: libraryId);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(null));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionRecordNotFound, result.FirstError);
    }
}
