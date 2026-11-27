#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Commands.ClearResumePoints;
using Lumina.Contracts.Requests.MediaConsumption;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="ClearResumePointsRequest"/>.
/// </summary>
public static class ClearResumePointsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="ClearResumePointsCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="targetUserId">The Id of the user whose resume points are deleted, taken from the route, or <see langword="null"/> for the current user.</param>
    /// <returns>The converted command.</returns>
    public static ClearResumePointsCommand ToCommand(this ClearResumePointsRequest request, string? targetUserId)
    {
        return new ClearResumePointsCommand(
            targetUserId,
            request.LibraryId
        );
    }
}
