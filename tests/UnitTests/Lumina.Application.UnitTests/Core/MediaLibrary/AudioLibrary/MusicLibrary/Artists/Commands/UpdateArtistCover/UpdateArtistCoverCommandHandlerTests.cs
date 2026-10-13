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
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateArtistCoverCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCoverCommandHandlerTests
{
    private const string STORED_COVER_PATH = "/media/music/library/artist/cover.jpg";

    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IMusicArtworkService _mockMusicArtworkService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<UpdateArtistCoverCommand> _mockValidator;
    private readonly UpdateArtistCoverCommandHandler _sut;
    private readonly Guid _userId;
    private readonly UpdateArtistCoverCommandFixture _updateArtistCoverCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCoverCommandHandlerTests"/> class.
    /// </summary>
    public UpdateArtistCoverCommandHandlerTests()
    {
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);
        _mockMusicArtworkService = Substitute.For<IMusicArtworkService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<UpdateArtistCoverCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access, the validator passes, the artist has no artwork yet, and the artwork storage succeeds.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<UpdateArtistCoverCommand>()).Returns([]);
        _mockMusicArtworkRepository.GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([]));
        _mockMusicArtworkRepository.UpsertCoverAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _mockMusicArtworkService.SaveArtistArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(STORED_COVER_PATH));
        _mockMusicArtworkService.DeleteStoredArtwork(Arg.Any<string>()).Returns(Result.From(Result.Deleted));

        _sut = new UpdateArtistCoverCommandHandler(_mockUnitOfWork, _mockMusicArtworkService, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorFails_ShouldReturnValidationErrorsWithoutStoringArtwork()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdateArtistCoverCommand>()).Returns([Errors.Music.CoverCannotBeNull]);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.CoverCannotBeNull, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(null));

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        // The stored artist belongs to a different library than the one named by the route.
        ArtistEntity existingArtist = _artistEntityFixture.Create(id: Guid.Parse(command.ArtistId!), libraryId: Guid.NewGuid());
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedErrorWithoutStoringArtwork()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkService.DidNotReceive().SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockMusicArtworkService.DidNotReceive().SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtworkLookupFails_ShouldReturnFailureResultWithoutStoringArtwork()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        SetupExistingLibrary(command);
        _mockMusicArtworkRepository.GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockMusicArtworkService.DidNotReceive().SaveArtistArtworkAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().UpsertCoverAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtworkStorageFails_ShouldReturnFailureResultWithoutUpsertingOrSaving()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        SetupExistingLibrary(command);
        _mockMusicArtworkService.SaveArtistArtworkAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Errors.FileSystemManagement.FileTooLarge);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileTooLarge, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().UpsertCoverAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenExistingCoverPathDiffersFromStoredPath_ShouldDeletePreviousArtwork()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        SetupExistingLibrary(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, artworkType: ArtworkType.Cover, fileName: "/media/music/library/artist/old-cover.jpg"));

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork("/media/music/library/artist/old-cover.jpg");
    }

    [Fact]
    public async Task HandleAsync_WhenExistingCoverPathEqualsStoredPath_ShouldNotDeletePreviousArtwork()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        SetupExistingLibrary(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, artworkType: ArtworkType.Cover, fileName: STORED_COVER_PATH));

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

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
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        SetupExistingLibrary(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Artist, artworkType: ArtworkType.Cover, fileName: existingCoverPath, includeFileName: existingCoverPath is not null));

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.DidNotReceive().DeleteStoredArtwork(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingPreviousArtworkFails_ShouldReturnFailureResultWithoutUpsertingOrSaving()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        SetupExistingLibrary(command);
        SetupExistingArtwork(_musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, artworkType: ArtworkType.Cover, fileName: "/media/music/library/artist/old-cover.jpg"));
        _mockMusicArtworkService.DeleteStoredArtwork(Arg.Any<string>()).Returns(Errors.FileSystemManagement.FileTooLarge);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

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
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        SetupExistingLibrary(command);
        _mockMusicArtworkRepository.UpsertCoverAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        SetupExistingArtist(command);
        SetupExistingLibrary(command);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockMusicArtworkRepository.Received(1).UpsertCoverAsync(
            MusicArtworkOwnerType.Artist, Guid.Parse(command.ArtistId!), STORED_COVER_PATH, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnUpdateArtistCoverResponseAndStoreArtwork()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();
        ArtistEntity existingArtist = SetupExistingArtist(command);
        LibraryEntity library = SetupExistingLibrary(command);

        // Act
        Result<UpdateArtistCoverResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        UpdateArtistCoverResponse response = Assert.IsType<UpdateArtistCoverResponse>(result.Value);
        Assert.Equal(STORED_COVER_PATH, response.CoverPath);
        await _mockMusicArtworkService.Received(1).SaveArtistArtworkAsync(
            existingArtist.LibraryId,
            library.Title,
            existingArtist.Name,
            Arg.Any<Stream>(),
            command.FileName!,
            Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.Received(1).UpsertCoverAsync(
            MusicArtworkOwnerType.Artist,
            existingArtist.Id,
            STORED_COVER_PATH,
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Sets up a stored artist that matches the library and artist identifiers of the command.
    /// </summary>
    /// <param name="command">The command whose route identifiers the stored artist must match.</param>
    /// <returns>The created <see cref="ArtistEntity"/>.</returns>
    private ArtistEntity SetupExistingArtist(UpdateArtistCoverCommand command)
    {
        ArtistEntity existingArtist = _artistEntityFixture.Create(
            id: Guid.Parse(command.ArtistId!),
            libraryId: Guid.Parse(command.LibraryId!));
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(existingArtist));
        return existingArtist;
    }

    /// <summary>
    /// Sets up a stored media library that matches the library of the command.
    /// </summary>
    /// <param name="command">The command whose library identifier the stored library must match.</param>
    /// <returns>The created <see cref="LibraryEntity"/>.</returns>
    private LibraryEntity SetupExistingLibrary(UpdateArtistCoverCommand command)
    {
        LibraryEntity library = _libraryEntityFixture.Create(id: Guid.Parse(command.LibraryId!));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        return library;
    }

    /// <summary>
    /// Sets up the artwork owned by the artist.
    /// </summary>
    /// <param name="artworks">The pieces of artwork owned by the artist.</param>
    private void SetupExistingArtwork(params MusicArtworkEntity[] artworks)
    {
        _mockMusicArtworkRepository.GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>(artworks));
    }
}
