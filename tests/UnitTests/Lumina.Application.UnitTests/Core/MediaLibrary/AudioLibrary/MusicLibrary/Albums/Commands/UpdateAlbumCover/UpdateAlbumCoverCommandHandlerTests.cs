#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbumCover;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbumCover;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbumCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateAlbumCoverCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumCoverCommandHandlerTests
{
    private const string STORED_COVER_PATH = "/media/music/library/artist/Album/album/cover.jpg";

    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IMusicArtworkService _mockMusicArtworkService;
    private readonly IPathService _mockPathService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<UpdateAlbumCoverCommand> _mockValidator;
    private readonly UpdateAlbumCoverCommandHandler _sut;
    private readonly Guid _userId;
    private readonly UpdateAlbumCoverCommandFixture _updateAlbumCoverCommandFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAlbumCoverCommandHandlerTests"/> class.
    /// </summary>
    public UpdateAlbumCoverCommandHandlerTests()
    {
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);
        _mockMusicArtworkService = Substitute.For<IMusicArtworkService>();
        _mockPathService = Substitute.For<IPathService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<UpdateAlbumCoverCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access, the validator passes, the album has no artwork yet, and the artwork storage succeeds.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<UpdateAlbumCoverCommand>()).Returns([]);
        // The stored artwork paths used by the tests are Unix separated, and sanitizing a name keeps it unchanged, as the real service does for valid names.
        _mockPathService.PathSeparator.Returns('/');
        _mockPathService.SanitizeSegment(Arg.Any<string>())
            .Returns(callInfo => PathSegment.Create(callInfo.Arg<string>(), isDirectory: true, isDrive: false));
        _mockMusicArtworkRepository.GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([]));
        _mockMusicArtworkRepository.UpsertCoverAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockMusicArtworkService.SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(STORED_COVER_PATH));
        _mockMusicArtworkService.DeleteStoredArtwork(Arg.Any<string>()).Returns(Result.From(Result.Deleted));

        _sut = new UpdateAlbumCoverCommandHandler(_mockUnitOfWork, _mockMusicArtworkService, _mockPathService, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorFails_ShouldReturnValidationErrorsWithoutStoringArtwork()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdateAlbumCoverCommand>()).Returns([Errors.Music.CoverCannotBeNull]);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.CoverCannotBeNull, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(null));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherLibrary_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        // The stored album belongs to a different library than the one named by the route.
        AlbumEntity existingAlbum = _albumEntityFixture.Create(id: Guid.Parse(command.AlbumId!), artistId: Guid.Parse(command.ArtistId!), libraryId: Guid.NewGuid());
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(existingAlbum));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        // The stored album belongs to a different artist than the one named by the route.
        AlbumEntity existingAlbum = _albumEntityFixture.Create(id: Guid.Parse(command.AlbumId!), artistId: Guid.NewGuid(), libraryId: Guid.Parse(command.LibraryId!));
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(existingAlbum));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedErrorWithoutStoringArtwork()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldPassEmptyArtistNameToArtworkService()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(null));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), string.Empty, Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtworkLookupFails_ShouldReturnFailureResultWithoutStoringArtwork()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        _mockMusicArtworkRepository.GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockMusicArtworkService.DidNotReceive().SaveAlbumArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().UpsertCoverAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtworkStorageFails_ShouldReturnFailureResultWithoutUpsertingOrSaving()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        _mockMusicArtworkService.SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Errors.FileSystemManagement.FileTooLarge);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileTooLarge, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().UpsertCoverAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenExistingCoverPathHasParseableReleaseType_ShouldPassThatReleaseTypeToArtworkService()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        // The album's release type is Album, but its stored cover already lives under an Ep directory, which wins.
        SetupExistingAlbum(command, releaseType: MusicReleaseType.Album);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, artworkType: ArtworkType.Cover, fileName: "/media/music/library/artist/Ep/album/cover.jpg"));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), "Ep", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheSegmentBeforeTheAlbumDirectoryIsTheArtistDirectoryNamedLikeAReleaseType_ShouldNotTreatItAsAReleaseType()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        // The artist is named "Live", which is also a release type name, but the album uses no release type, so the segment right before the album
        // directory is the artist directory and must not be mistaken for a release type directory.
        SetupExistingAlbum(command, includeReleaseType: false);
        SetupExistingLibrary(command);
        SetupExistingArtist(command, name: "Live");
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, artworkType: ArtworkType.Cover, fileName: "/media/music/library/Live/album/cover.jpg"));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), "Live", Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Is<string?>(releaseTypeName => releaseTypeName == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheSegmentBeforeTheAlbumDirectoryIsAReleaseTypeOfAnArtistNamedLikeAReleaseType_ShouldKeepIt()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        // The artist is named "Album" and its cover is stored under an "Ep" release type directory, which differs from the artist directory, so it is kept.
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        SetupExistingArtist(command, name: "Album");
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, artworkType: ArtworkType.Cover, fileName: "/media/music/library/Album/Ep/album/cover.jpg"));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), "Album", Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), "Ep", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenExistingCoverPathHasNonParseableReleaseType_ShouldFallBackToFirstReleaseType()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command, releaseType: MusicReleaseType.Single);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, artworkType: ArtworkType.Cover, fileName: "/media/music/library/artist/NotAReleaseType/album/cover.jpg"));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), "Single", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenExistingCoverPathHasTooFewSegments_ShouldFallBackToFirstReleaseType()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command, releaseType: MusicReleaseType.Single);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        // The stored cover path carries fewer than three segments, so it does not expose a release type directory.
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, artworkType: ArtworkType.Cover, fileName: "cover.jpg"));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), "Single", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumHasNoExistingCover_ShouldUseFirstReleaseType()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command, releaseType: MusicReleaseType.Single);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), "Single", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumHasNoExistingCoverAndNoReleaseTypes_ShouldPassNullReleaseTypeToArtworkService()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command, includeReleaseType: false);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Is<string?>(releaseTypeName => releaseTypeName == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenExistingCoverPathDiffersFromStoredPath_ShouldDeletePreviousArtwork()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, artworkType: ArtworkType.Cover, fileName: "/media/music/library/artist/Ep/album/old-cover.jpg"));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork("/media/music/library/artist/Ep/album/old-cover.jpg");
    }

    [Fact]
    public async Task HandleAsync_WhenExistingCoverPathEqualsStoredPath_ShouldNotDeletePreviousArtwork()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, artworkType: ArtworkType.Cover, fileName: STORED_COVER_PATH));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.DidNotReceive().DeleteStoredArtwork(Arg.Any<string>());
    }

    [Theory]
    [InlineData(null)] // no file name at all
    [InlineData("")] // empty file name
    [InlineData("   ")] // whitespace file name
    public async Task HandleAsync_WhenExistingCoverPathIsEmpty_ShouldNotDeletePreviousArtwork(string? existingCoverPath)
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, artworkType: ArtworkType.Cover, fileName: existingCoverPath, includeFileName: existingCoverPath is not null));

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.DidNotReceive().DeleteStoredArtwork(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingPreviousArtworkFails_ShouldReturnFailureResultWithoutUpsertingOrSaving()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, artworkType: ArtworkType.Cover, fileName: "/media/music/library/artist/Ep/album/old-cover.jpg"));
        _mockMusicArtworkService.DeleteStoredArtwork(Arg.Any<string>()).Returns(Errors.FileSystemManagement.FileTooLarge);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileTooLarge, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().UpsertCoverAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpsertCoverFails_ShouldReturnFailureResultWithoutSaving()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        _mockMusicArtworkRepository.UpsertCoverAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        SetupExistingAlbum(command);
        SetupExistingLibrary(command);
        SetupExistingArtist(command);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockMusicArtworkRepository.Received(1).UpsertCoverAsync(
            MusicArtworkOwnerType.Album, Guid.Parse(command.AlbumId!), STORED_COVER_PATH, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnUpdateAlbumCoverResponseAndStoreArtwork()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();
        AlbumEntity existingAlbum = SetupExistingAlbum(command, releaseType: MusicReleaseType.Album);
        LibraryEntity library = SetupExistingLibrary(command);
        ArtistEntity artist = SetupExistingArtist(command);

        // Act
        Result<UpdateAlbumCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        UpdateAlbumCoverResponse response = Assert.IsType<UpdateAlbumCoverResponse>(result.Value);
        Assert.Equal(STORED_COVER_PATH, response.CoverPath);
        await _mockMusicArtworkService.Received(1).SaveAlbumArtworkAsync(
            existingAlbum.LibraryId,
            existingAlbum.Id,
            library.Title,
            artist.Name,
            existingAlbum.Title,
            Arg.Any<Stream>(),
            command.FileName!,
            "Album",
            Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.Received(1).UpsertCoverAsync(
            MusicArtworkOwnerType.Album,
            existingAlbum.Id,
            STORED_COVER_PATH,
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Sets up a stored album that matches the library, artist and album identifiers of the command.
    /// </summary>
    /// <param name="command">The command whose route identifiers the stored album must match.</param>
    /// <param name="releaseType">Optional. The release type of the stored album.</param>
    /// <param name="includeReleaseType">Whether the stored album should carry a release type, or an empty collection.</param>
    /// <returns>The created <see cref="AlbumEntity"/>.</returns>
    private AlbumEntity SetupExistingAlbum(UpdateAlbumCoverCommand command, MusicReleaseType? releaseType = MusicReleaseType.Album, bool includeReleaseType = true)
    {
        AlbumEntity existingAlbum = _albumEntityFixture.Create(
            id: Guid.Parse(command.AlbumId!),
            artistId: Guid.Parse(command.ArtistId!),
            libraryId: Guid.Parse(command.LibraryId!),
            releaseType: releaseType,
            includeReleaseType: includeReleaseType);
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(existingAlbum));
        return existingAlbum;
    }

    /// <summary>
    /// Sets up a stored media library that matches the library of the command.
    /// </summary>
    /// <param name="command">The command whose library identifier the stored library must match.</param>
    /// <returns>The created <see cref="LibraryEntity"/>.</returns>
    private LibraryEntity SetupExistingLibrary(UpdateAlbumCoverCommand command)
    {
        LibraryEntity library = _libraryEntityFixture.Create(id: Guid.Parse(command.LibraryId!));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        return library;
    }

    /// <summary>
    /// Sets up a stored artist that matches the artist of the command.
    /// </summary>
    /// <param name="command">The command whose artist identifier the stored artist must match.</param>
    /// <param name="name">Optional. The name of the stored artist.</param>
    /// <returns>The created <see cref="ArtistEntity"/>.</returns>
    private ArtistEntity SetupExistingArtist(UpdateAlbumCoverCommand command, string? name = null)
    {
        ArtistEntity artist = _artistEntityFixture.Create(id: Guid.Parse(command.ArtistId!), libraryId: Guid.Parse(command.LibraryId!), name: name);
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        return artist;
    }

    /// <summary>
    /// Sets up the artwork owned by the album.
    /// </summary>
    /// <param name="artworks">The pieces of artwork owned by the album.</param>
    private void SetupExistingArtwork(params MusicArtworkEntity[] artworks)
    {
        _mockMusicArtworkRepository.GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>(artworks));
    }
}
