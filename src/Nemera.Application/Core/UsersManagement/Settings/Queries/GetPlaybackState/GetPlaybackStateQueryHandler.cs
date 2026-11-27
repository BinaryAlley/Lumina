#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Settings.Queries.GetPlaybackState;

/// <summary>
/// Handler for the query to get the playback state the player reported for the current user automatically.
/// </summary>
public class GetPlaybackStateQueryHandler : IQueryHandler<GetPlaybackStateQuery, Result<PlaybackStateResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateQueryHandler"/> class.
    /// </summary>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetPlaybackStateQueryHandler(ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get the playback state the player reported for the current user automatically.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully retrieved <see cref="PlaybackStateResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<PlaybackStateResponse>> HandleAsync(GetPlaybackStateQuery query, CancellationToken cancellationToken)
    {
        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Get the settings of the current user from the repository.
        Result<UserSettingsEntity?> getSettingsResult = await _unitOfWork.UserSettingsRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getSettingsResult.IsFailure)
            return getSettingsResult.Errors;

        // When no settings are stored for the current user yet, return the default playback state.
        if (getSettingsResult.Value is null)
        {
            Result<UserSettings> defaultSettingsResult = UserSettings.Create();
            if (defaultSettingsResult.IsFailure)
                return defaultSettingsResult.Errors;
            return new PlaybackStateResponse(defaultSettingsResult.Value.Volume, defaultSettingsResult.Value.IsMuted);
        }

        return new PlaybackStateResponse(getSettingsResult.Value.Volume, getSettingsResult.Value.IsMuted);
    }
}
