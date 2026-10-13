#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbumCover;

/// <summary>
/// Handler for the command to update the cover image of an existing album.
/// </summary>
public class UpdateAlbumCoverCommandHandler : ICommandHandler<UpdateAlbumCoverCommand, Result<UpdateAlbumCoverResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMusicArtworkService _musicArtworkService;
    private readonly IPathService _pathService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateAlbumCoverCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAlbumCoverCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="musicArtworkService">Injected service for storing the artwork of a music library item.</param>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateAlbumCoverCommandHandler(IUnitOfWork unitOfWork, IMusicArtworkService musicArtworkService, IPathService pathService, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<UpdateAlbumCoverCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _musicArtworkService = musicArtworkService;
        _pathService = pathService;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update the cover image of an existing album.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the response carrying the relative path of the stored cover image, or an error message.
    /// </returns>
    public async Task<Result<UpdateAlbumCoverResponse>> HandleAsync(UpdateAlbumCoverCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifiers are non-empty Guids before this point.
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        Guid albumId = Guid.Parse(command.AlbumId!);

        Result<AlbumEntity?> getAlbumResult = await _unitOfWork.AlbumRepository.GetByIdAsync(albumId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getAlbumResult.IsFailure)
            return getAlbumResult.Errors;
        if (getAlbumResult.Value is null)
            return DomainErrors.Music.AlbumNotFound;
        AlbumEntity existingAlbum = getAlbumResult.Value;

        // Resource scoping: the album must belong to the library and the artist named by the route, so that an album can never be edited through
        // another library's or artist's route; the mismatch is reported as not found, without disclosing that the album exists elsewhere.
        if (existingAlbum.LibraryId != libraryId || existingAlbum.ArtistId != artistId)
            return DomainErrors.Music.AlbumNotFound;

        // Admins can update the albums of all libraries; for everyone else, only the albums of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingAlbum.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // The artwork is stored under the library and artist segments, so their names are resolved to keep the location consistent with the scanned artwork.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(existingAlbum.LibraryId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;
        string libraryName = getLibraryResult.Value.Title;

        Result<ArtistEntity?> getArtistResult = await _unitOfWork.ArtistRepository.GetByIdAsync(existingAlbum.ArtistId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getArtistResult.IsFailure)
            return getArtistResult.Errors;
        string artistName = getArtistResult.Value?.Name ?? string.Empty;

        // The artwork of the album is stored under the directory of its release type, mirroring the structure of the scanned files, so that a
        // re-uploaded cover replaces the cover stored for the album at its standardized location instead of writing a second copy elsewhere.
        Result<IReadOnlyList<MusicArtworkEntity>> getArtworkResult = await _unitOfWork.MusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, cancellationToken).ConfigureAwait(false);
        if (getArtworkResult.IsFailure)
            return getArtworkResult.Errors;
        string? existingCoverPath = getArtworkResult.Value
            .Where(artwork => artwork.ArtworkType == ArtworkType.Cover)
            .Select(artwork => artwork.FileName)
            .FirstOrDefault();
        string? releaseTypeName = ExtractReleaseTypeDirectoryName(existingCoverPath, artistName)
            ?? (existingAlbum.ReleaseTypes.Count > 0 ? existingAlbum.ReleaseTypes[0].ReleaseType.ToString() : null);

        Result<string> storeArtworkResult = await _musicArtworkService.SaveAlbumArtworkAsync(
            existingAlbum.LibraryId,
            existingAlbum.Id,
            libraryName,
            artistName,
            existingAlbum.Title,
            command.Cover!,
            command.FileName!,
            releaseTypeName,
            cancellationToken).ConfigureAwait(false);
        if (storeArtworkResult.IsFailure)
            return storeArtworkResult.Errors;

        // The stored cover of the album is replaced as a whole: when the new cover is stored under a different name than the previous one
        // (for example a png replacing a jpg), the previous file is removed, so that no orphaned artwork is left on disk without a reference.
        if (!string.IsNullOrWhiteSpace(existingCoverPath) && existingCoverPath != storeArtworkResult.Value)
        {
            Result<Deleted> deleteResult = _musicArtworkService.DeleteStoredArtwork(existingCoverPath);
            if (deleteResult.IsFailure)
                return deleteResult.Errors;
        }

        // Map the newly stored cover image onto the artwork of the album, replacing the existing cover or adding a new one.
        Result<Updated> upsertResult = await _unitOfWork.MusicArtworkRepository.UpsertCoverAsync(
            MusicArtworkOwnerType.Album, albumId, storeArtworkResult.Value, cancellationToken).ConfigureAwait(false);
        if (upsertResult.IsFailure)
            return upsertResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return new UpdateAlbumCoverResponse(storeArtworkResult.Value);
    }

    /// <summary>
    /// Resolves the name of the release type directory the artwork of an album is stored under, from the relative path of its existing cover.
    /// </summary>
    /// <param name="coverRelativePath">The relative path of the existing cover image of the album, or <see langword="null"/> when the album has no cover.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <returns>The name of the on-disk release type directory, or <see langword="null"/> when the album does not use one.</returns>
    private string? ExtractReleaseTypeDirectoryName(string? coverRelativePath, string artistName)
    {
        if (string.IsNullOrWhiteSpace(coverRelativePath))
            return null;
        // The path is media/music/{library}/{artist}[/{releaseType}]/{album}/{file}, so the segment right before the album directory is the release
        // type directory when the album uses one, and the artist directory otherwise. An artist whose name happens to match a release type, like
        // "Live", must not be mistaken for one, so the segment is only accepted as a release type when it is not the directory of the artist.
        string[] segments = coverRelativePath.Split(_pathService.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3)
            return null;
        string candidate = segments[^3];
        if (IsArtistDirectory(candidate, artistName))
            return null;
        return Enum.TryParse(candidate, ignoreCase: true, out MusicReleaseType releaseType) && Enum.IsDefined(releaseType) ? candidate : null;
    }

    /// <summary>
    /// Checks whether the provided path segment is the artwork directory of the artist of the album.
    /// </summary>
    /// <param name="pathSegment">The path segment to be checked.</param>
    /// <param name="artistName">The name of the artist of the album.</param>
    /// <returns><see langword="true"/> when the segment is the artwork directory of the artist, <see langword="false"/> otherwise.</returns>
    private bool IsArtistDirectory(string pathSegment, string artistName)
    {
        if (string.IsNullOrWhiteSpace(artistName))
            return false;
        Result<PathSegment> artistSegmentResult = _pathService.SanitizeSegment(artistName);
        return artistSegmentResult.IsSuccess
            && string.Equals(pathSegment, artistSegmentResult.Value.Name, StringComparison.OrdinalIgnoreCase);
    }
}
