#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.MediaConsumption;

/// <summary>
/// Data transfer object for the data and the proxy routes of the statistics page.
/// </summary>
/// <param name="Title">The localized title of the page.</param>
/// <param name="IsAdmin">Whether the current user is an administrator and can select another user or all users.</param>
/// <param name="CurrentUserId">The Id of the current user, or <see langword="null"/> when it cannot be determined.</param>
/// <param name="Culture">The culture of the page.</param>
/// <param name="GetStatisticsUrl">The route that returns the statistics of the selected user.</param>
/// <param name="GetAllUsersStatisticsUrl">The route that returns the statistics of every user.</param>
/// <param name="GetRecentlyConsumedUrl">The route that returns the recently consumed media items of the selected user.</param>
/// <param name="GetMostConsumedUrl">The route that returns the most consumed media items of the selected user.</param>
/// <param name="GetTagConsumptionUrl">The route that returns the tag consumption of the selected user.</param>
/// <param name="GetGenreConsumptionUrl">The route that returns the genre consumption of the selected user.</param>
/// <param name="GetLibrariesUrl">The route that returns the libraries available for the library filter.</param>
/// <param name="GetUsersUrl">The route that returns the users available for the administrator user selector, or an empty string for a regular user.</param>
/// <param name="DeleteConsumptionItemHistoryUrl">The route that deletes every consumption record of a media item.</param>
/// <param name="ClearConsumptionHistoryUrl">The route that deletes the consumption history of the selected user.</param>
/// <param name="ClearResumePointsUrl">The route that deletes the resume points of the selected user.</param>
[DebuggerDisplay("IsAdmin: {IsAdmin}; CurrentUserId: {CurrentUserId}")]
public sealed record StatisticsPageDto(
    string Title,
    bool IsAdmin,
    Guid? CurrentUserId,
    string Culture,
    string GetStatisticsUrl,
    string GetAllUsersStatisticsUrl,
    string GetRecentlyConsumedUrl,
    string GetMostConsumedUrl,
    string GetTagConsumptionUrl,
    string GetGenreConsumptionUrl,
    string GetLibrariesUrl,
    string GetUsersUrl,
    string DeleteConsumptionItemHistoryUrl,
    string ClearConsumptionHistoryUrl,
    string ClearResumePointsUrl
);
