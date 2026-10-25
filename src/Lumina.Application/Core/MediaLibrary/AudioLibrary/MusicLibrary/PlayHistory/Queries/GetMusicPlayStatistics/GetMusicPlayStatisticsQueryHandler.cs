#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetMusicPlayStatistics;

/// <summary>
/// Handler for the query to get the listening statistics of the current user.
/// </summary>
public class GetMusicPlayStatisticsQueryHandler : IQueryHandler<GetMusicPlayStatisticsQuery, Result<MusicPlayStatisticsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicPlayStatisticsQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    public GetMusicPlayStatisticsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handles the query to get the listening statistics of the current user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the listening statistics of the current user, or an error message.
    /// </returns>
    public async Task<Result<MusicPlayStatisticsResponse>> HandleAsync(GetMusicPlayStatisticsQuery query, CancellationToken cancellationToken)
    {
        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        Result<IReadOnlyList<MusicPlayRecordEntity>> getRecordsResult = await _unitOfWork.MusicPlayRecordRepository.GetForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getRecordsResult.IsFailure)
            return getRecordsResult.Errors;

        IReadOnlyList<MusicPlayRecordEntity> records = getRecordsResult.Value;
        // The total listening time includes the listened duration of every recorded play, including the plays that do not count.
        long totalListeningDurationMilliseconds = records.Sum(record => record.ListenedDurationMilliseconds);
        // A play counts when it completed or when it reached the threshold. An in-progress play that already reached the threshold is counted,
        // so the counted total need not equal the completed and stopped plays together.
        int totalPlayCount = records.Count(record => MusicPlayRecord.DoesCountAsPlay(record.Status, record.ListenedDurationMilliseconds.ToClampedTimeSpan()));
        int completedPlayCount = records.Count(record => record.Status == PlaybackRecordStatus.Completed);
        int stoppedPlayCount = records.Count(record => record.Status == PlaybackRecordStatus.Stopped && MusicPlayRecord.DoesCountAsPlay(record.Status, record.ListenedDurationMilliseconds.ToClampedTimeSpan()));
        double completionRate = totalPlayCount == 0 ? 0 : (double)completedPlayCount / totalPlayCount;

        return new MusicPlayStatisticsDto(
            totalListeningDurationMilliseconds,
            totalPlayCount,
            completedPlayCount,
            stoppedPlayCount,
            completionRate).ToResponse();
    }
}
