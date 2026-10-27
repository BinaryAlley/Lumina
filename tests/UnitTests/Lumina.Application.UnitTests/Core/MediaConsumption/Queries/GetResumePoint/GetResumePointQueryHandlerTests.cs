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
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Queries.GetResumePoint;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetResumePoint;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetResumePoint;

/// <summary>
/// Contains unit tests for the <see cref="GetResumePointQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetResumePointQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IMediaResumePointRepository _mockMediaResumePointRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IValidator<GetResumePointQuery> _mockValidator;
    private readonly GetResumePointQueryHandler _sut;
    private readonly GetResumePointQueryFixture _getResumePointQueryFixture = new();
    private readonly MediaResumePointEntityFixture _mediaResumePointEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetResumePointQueryHandlerTests"/> class.
    /// </summary>
    public GetResumePointQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockMediaResumePointRepository = Substitute.For<IMediaResumePointRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();

        _mockUnitOfWork.MediaResumePointRepository.Returns(_mockMediaResumePointRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        // Default stubs: saving changes succeeds, the current user is authenticated and the stale resume point deletion succeeds.
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockMediaResumePointRepository.DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        // Default stubs: no resume point and no library exist, so that every test states the data it needs explicitly.
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaResumePointEntity?>(null));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        _mockValidator = Substitute.For<IValidator<GetResumePointQuery>>();
        _mockValidator.Validate(Arg.Any<GetResumePointQuery>()).Returns([]);

        _sut = new GetResumePointQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadTheResumePoint()
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetResumePointQuery>())
            .Returns([DomainErrors.MediaConsumption.UnsupportedMediaKind]);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadTheResumePoint()
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheMediaKindIsLowerCased_ShouldParseItCaseInsensitivelyAndReadTheMatchingResumePoint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(
            mediaKind: "musictrack",
            mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId);
        StubExistingTrack(mediaItemId, libraryId);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaResumePointRepository.Received(1).GetForUserAndMediaItemAsync(
            _userId,
            Arg.Is<MediaKind>(mediaKind => mediaKind == MediaKind.MusicTrack),
            mediaItemId,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoRuleExistsForTheMediaKind_ShouldReturnUnsupportedMediaKindWithoutReadingTheResumePoint()
    {
        // Arrange
        // MediaKind.None is a defined name, so the validator would accept it, but no consumption rule exists for it.
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaKind: nameof(MediaKind.None));

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheMediaKindCannotBeResumed_ShouldReturnMediaKindCannotResumeWithoutReadingTheResumePoint()
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaKind: nameof(MediaKind.Photo));

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.MediaKindCannotResume, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheMediaKindIsNotSupportedByTheCurrentCatalogues_ShouldReturnUnsupportedMediaKind()
    {
        // Arrange
        // A movie is resumable in principle, but its catalogue does not exist yet, so it is rejected before the stored resume point is read.
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaKind: nameof(MediaKind.Movie));

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheResumePointReadFails_ShouldReturnThatError()
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The resume point could not be read.");
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheResumePointDoesNotExist_ShouldReturnMediaResumePointNotFound()
    {
        // Arrange
        GetResumePointQuery query = _getResumePointQueryFixture.Create();

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.MediaResumePointNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheLibraryReadFails_ShouldReturnThatError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        Error error = Error.Failure("Persistence.Failure", "The library could not be read.");
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheLibraryDoesNotTrackConsumptionProgress_ShouldDeleteTheStaleResumePointAndReturnMediaResumePointNotFound()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId, shouldTrackConsumptionProgress: false);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.MediaResumePointNotFound, result.FirstError);
        await _mockMediaResumePointRepository.Received(1).DeleteForUserAndMediaItemAsync(_userId, MediaKind.MusicTrack, mediaItemId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheStaleResumePointDeletionFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId, shouldTrackConsumptionProgress: false);
        Error error = Error.Failure("Persistence.Failure", "The stale resume point could not be deleted.");
        _mockMediaResumePointRepository.DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSavingTheStaleResumePointDeletionConflicts_ShouldStillReturnMediaResumePointNotFound()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId, shouldTrackConsumptionProgress: false);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(ApplicationErrors.Persistence.ConcurrencyConflict);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.MediaResumePointNotFound, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSavingTheStaleResumePointDeletionFailsOtherwise_ShouldReturnThatError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId, shouldTrackConsumptionProgress: false);
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackReadFails_ShouldReturnThatErrorAndNotDeleteTheResumePoint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId);
        Error error = Error.Failure("Persistence.Failure", "The track could not be read.");
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackDoesNotExist_ShouldDeleteTheOrphanResumePointAndReturnTrackNotFound()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId);
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(null));

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockMediaResumePointRepository.Received(1).DeleteForUserAndMediaItemAsync(_userId, MediaKind.MusicTrack, mediaItemId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackBelongsToAnotherLibrary_ShouldDeleteTheOrphanResumePointAndReturnTrackNotFound()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId);
        StubExistingTrack(mediaItemId, Guid.NewGuid());

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockMediaResumePointRepository.Received(1).DeleteForUserAndMediaItemAsync(_userId, MediaKind.MusicTrack, mediaItemId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingTheOrphanResumePointFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId);
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(null));
        Error error = Error.Failure("Persistence.Failure", "The orphan resume point could not be deleted.");
        _mockMediaResumePointRepository.DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSavingTheOrphanResumePointDeletionConflicts_ShouldStillReturnTrackNotFound()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId);
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(null));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(ApplicationErrors.Persistence.ConcurrencyConflict);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSavingTheOrphanResumePointDeletionFailsOtherwise_ShouldReturnThatError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubLibrary(libraryId);
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(null));
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackExistsInTheResumePointLibrary_ShouldReturnTheMappedResumePoint()
    {
        // Arrange
        Guid resumePointId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime lastConsumedOnUtc = new(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            id: resumePointId,
            userId: _userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 60_000,
            lastProgressFraction: 0.25,
            lastConsumedOnUtc: lastConsumedOnUtc);
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaResumePointEntity?>(resumePoint));
        StubLibrary(libraryId);
        StubExistingTrack(mediaItemId, libraryId);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ResumePointResponse response = result.Value;
        Assert.Equal(resumePointId, response.Id);
        Assert.Equal(libraryId, response.LibraryId);
        Assert.Equal(mediaItemId, response.MediaItemId);
        Assert.Equal(MediaKind.MusicTrack, response.MediaKind);
        Assert.Equal(MediaPositionKind.TimeOffset, response.LastPosition.Kind);
        Assert.Equal(60d, response.LastPosition.TimeOffsetSeconds!.Value);
        Assert.Null(response.LastPosition.Location);
        Assert.Equal(0.25, response.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, response.LastConsumedOnUtc);
        await _mockTrackRepository.Received(1).GetByIdAsync(
            mediaItemId,
            shouldIncludeNavigationProperties: false,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheLibraryOfTheResumePointDoesNotExist_ShouldStillServeTheResumePoint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.MusicTrack, mediaItemId, libraryId);
        StubExistingTrack(mediaItemId, libraryId);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheBookReadFails_ShouldReturnThatErrorAndNotDeleteTheResumePoint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(
            mediaKind: nameof(MediaKind.Book),
            mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.Book, mediaItemId, libraryId);
        StubLibrary(libraryId);
        Error error = Error.Failure("Persistence.Failure", "The book could not be read.");
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheBookDoesNotExist_ShouldDeleteTheOrphanResumePointAndReturnBookNotFound()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(
            mediaKind: nameof(MediaKind.Book),
            mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.Book, mediaItemId, libraryId);
        StubLibrary(libraryId);
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(null));

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.BookNotFound, result.FirstError);
        await _mockMediaResumePointRepository.Received(1).DeleteForUserAndMediaItemAsync(_userId, MediaKind.Book, mediaItemId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheBookBelongsToAnotherLibrary_ShouldDeleteTheOrphanResumePointAndReturnBookNotFound()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        GetResumePointQuery query = _getResumePointQueryFixture.Create(
            mediaKind: nameof(MediaKind.Book),
            mediaItemId: mediaItemId.ToString());
        StubResumePoint(MediaKind.Book, mediaItemId, libraryId);
        StubLibrary(libraryId);
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(_bookEntityFixture.Create(id: mediaItemId, libraryId: Guid.NewGuid())));

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.BookNotFound, result.FirstError);
        await _mockMediaResumePointRepository.Received(1).DeleteForUserAndMediaItemAsync(_userId, MediaKind.Book, mediaItemId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheBookExistsInTheResumePointLibrary_ShouldReturnTheMappedResumePoint()
    {
        // Arrange
        Guid resumePointId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime lastConsumedOnUtc = new(2026, 4, 5, 6, 7, 8, DateTimeKind.Utc);
        GetResumePointQuery query = _getResumePointQueryFixture.Create(
            mediaKind: nameof(MediaKind.Book),
            mediaItemId: mediaItemId.ToString());
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            id: resumePointId,
            userId: _userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Book,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionLocation: "chapter-2",
            lastProgressFraction: 0.75,
            lastConsumedOnUtc: lastConsumedOnUtc);
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaResumePointEntity?>(resumePoint));
        StubLibrary(libraryId);
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(_bookEntityFixture.Create(id: mediaItemId, libraryId: libraryId)));

        // Act
        Result<ResumePointResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ResumePointResponse response = result.Value;
        Assert.Equal(resumePointId, response.Id);
        Assert.Equal(libraryId, response.LibraryId);
        Assert.Equal(mediaItemId, response.MediaItemId);
        Assert.Equal(MediaKind.Book, response.MediaKind);
        Assert.Equal(MediaPositionKind.Location, response.LastPosition.Kind);
        Assert.Null(response.LastPosition.TimeOffsetSeconds);
        Assert.Equal("chapter-2", response.LastPosition.Location);
        Assert.Equal(0.75, response.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, response.LastConsumedOnUtc);
        await _mockBookRepository.Received(1).GetByIdAsync(
            mediaItemId,
            shouldIncludeNavigationProperties: false,
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Stubs the resume point repository so that reading the resume point of the current user returns a resume point of the provided media item.
    /// </summary>
    /// <param name="mediaKind">The kind of media of the stored resume point.</param>
    /// <param name="mediaItemId">The Id of the media item of the stored resume point.</param>
    /// <param name="libraryId">The Id of the media library of the stored resume point.</param>
    private void StubResumePoint(MediaKind mediaKind, Guid mediaItemId, Guid libraryId)
    {
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: _userId,
            libraryId: libraryId,
            mediaKind: mediaKind,
            mediaItemId: mediaItemId);
        _mockMediaResumePointRepository.GetForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaResumePointEntity?>(resumePoint));
    }

    /// <summary>
    /// Stubs the library repository so that reading the library returns a library with the provided tracking setting.
    /// </summary>
    /// <param name="libraryId">The Id of the returned library.</param>
    /// <param name="shouldTrackConsumptionProgress">Whether the returned library tracks the consumption progress of its items.</param>
    private void StubLibrary(Guid libraryId, bool shouldTrackConsumptionProgress = true)
    {
        LibraryEntity library = _libraryEntityFixture.Create(
            id: libraryId,
            shouldTrackConsumptionProgress: shouldTrackConsumptionProgress);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
    }

    /// <summary>
    /// Stubs the track repository so that reading the track returns a track that belongs to the provided library.
    /// </summary>
    /// <param name="trackId">The Id of the returned track.</param>
    /// <param name="libraryId">The Id of the library that contains the returned track.</param>
    private void StubExistingTrack(Guid trackId, Guid libraryId)
    {
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId);
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
    }
}
