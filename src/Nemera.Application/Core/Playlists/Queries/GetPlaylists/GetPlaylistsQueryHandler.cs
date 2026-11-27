#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Playlists.Queries.GetPlaylists;

/// <summary>
/// Handler for the query to get the playlists of the current user.
/// </summary>
public class GetPlaylistsQueryHandler : IQueryHandler<GetPlaylistsQuery, Result<PlaylistSummaryResponse[]>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    public GetPlaylistsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handles the query to get the playlists of the current user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the playlists of the current user, or an error message.
    /// </returns>
    public async Task<Result<PlaylistSummaryResponse[]>> HandleAsync(GetPlaylistsQuery query, CancellationToken cancellationToken)
    {
        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        Result<IReadOnlyList<PlaylistSummaryDto>> getPlaylistsResult = await _unitOfWork.PlaylistRepository.GetAllForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getPlaylistsResult.IsFailure)
            return getPlaylistsResult.Errors;

        return Result.From(getPlaylistsResult.Value.Select(playlist => playlist.ToResponse()).ToArray());
    }
}
