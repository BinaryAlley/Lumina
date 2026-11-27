#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;

/// <summary>
/// Handler for the query to get the audio file of a track, for streaming.
/// </summary>
public class GetTrackStreamQueryHandler : IQueryHandler<GetTrackStreamQuery, Result<TrackStreamDataDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPathService _pathService;
    private readonly IValidator<GetTrackStreamQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackStreamQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="pathService">Injected service for file system path related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetTrackStreamQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IPathService pathService, IValidator<GetTrackStreamQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _pathService = pathService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the audio file of a track, for streaming.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the data of the audio file of the track, or an error message.
    /// </returns>
    public async Task<Result<TrackStreamDataDto>> HandleAsync(GetTrackStreamQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifiers are non-empty Guids before this point.
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);

        // Admins can stream the tracks of all libraries; for everyone else, only the tracks of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // The projection is filtered by the library, the album and the artist named by the route as well as by the track, so a null row
        // means the track either does not exist or is not in scope for the route, which is reported as not found without disclosing anything further.
        Result<TrackStreamDto?> getTrackResult = await _unitOfWork.TrackRepository.GetTrackStreamInfoAsync(libraryId, albumId, artistId, trackId, cancellationToken).ConfigureAwait(false);
        if (getTrackResult.IsFailure)
            return getTrackResult.Errors;
        if (getTrackResult.Value is null)
            return Errors.Music.TrackNotFound;
        TrackStreamDto track = getTrackResult.Value;

        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(track.LibraryId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return Errors.Library.LibraryNotFound;
        LibraryEntity library = getLibraryResult.Value;

        // The audio file is only served when it is stored inside one of the content locations of its library, so that a stored path can never
        // be used to read an arbitrary file of the host; the failure is reported as not found, and it never includes the path.
        if (!library.ContentLocations.Any(contentLocation => _pathService.IsPathWithin(track.Path, contentLocation.Path)))
            return Errors.Music.TrackFileNotFound;

        try
        {
            // The file is read once, before the response starts, so that the response can be streamed without any further database or file system work.
            FileInfo fileInformation = new(track.Path);
            if (!fileInformation.Exists)
                return Errors.Music.TrackFileNotFound;

            long length = fileInformation.Length;
            DateTimeOffset lastModified = new(fileInformation.LastWriteTimeUtc, TimeSpan.Zero);
            return new TrackStreamDataDto(track.Path, track.Path.ToAudioFileFormat(), length, lastModified);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            // A file that cannot be read is indistinguishable, to the caller, from a file that does not exist, so the same not found error is returned.
            return Errors.Music.TrackFileNotFound;
        }
    }
}
