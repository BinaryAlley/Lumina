#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="StatisticsPageDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatisticsPageDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="StatisticsPageDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="title">Optional localized title of the page.</param>
    /// <param name="isAdmin">Optional value indicating whether the current user is an administrator.</param>
    /// <param name="currentUserId">Optional Id of the current user.</param>
    /// <param name="culture">Optional culture of the page.</param>
    /// <param name="getStatisticsUrl">Optional route that returns the statistics of the selected user.</param>
    /// <param name="getAllUsersStatisticsUrl">Optional route that returns the statistics of every user.</param>
    /// <param name="getRecentlyConsumedUrl">Optional route that returns the recently consumed media items.</param>
    /// <param name="getMostConsumedUrl">Optional route that returns the most consumed media items.</param>
    /// <param name="getTagConsumptionUrl">Optional route that returns the tag consumption.</param>
    /// <param name="getGenreConsumptionUrl">Optional route that returns the genre consumption.</param>
    /// <param name="getLibrariesUrl">Optional route that returns the libraries available for the library filter.</param>
    /// <param name="getUsersUrl">Optional route that returns the users available for the administrator user selector.</param>
    /// <param name="deleteConsumptionItemHistoryUrl">Optional route that deletes every consumption record of a media item.</param>
    /// <param name="clearConsumptionHistoryUrl">Optional route that deletes the consumption history.</param>
    /// <param name="clearResumePointsUrl">Optional route that deletes the resume points.</param>
    /// <param name="includeCurrentUserId">Whether the current user Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="StatisticsPageDto"/> instance.</returns>
    public StatisticsPageDto Create(
        string? title = null,
        bool? isAdmin = null,
        Guid? currentUserId = null,
        string? culture = null,
        string? getStatisticsUrl = null,
        string? getAllUsersStatisticsUrl = null,
        string? getRecentlyConsumedUrl = null,
        string? getMostConsumedUrl = null,
        string? getTagConsumptionUrl = null,
        string? getGenreConsumptionUrl = null,
        string? getLibrariesUrl = null,
        string? getUsersUrl = null,
        string? deleteConsumptionItemHistoryUrl = null,
        string? clearConsumptionHistoryUrl = null,
        string? clearResumePointsUrl = null,
        bool includeCurrentUserId = true)
    {
        return new StatisticsPageDto(
            Title: title ?? _faker.Lorem.Sentence(),
            IsAdmin: isAdmin ?? _faker.Random.Bool(),
            CurrentUserId: includeCurrentUserId ? (currentUserId ?? _faker.Random.Guid()) : null,
            Culture: culture ?? _faker.PickRandom("en-US", "de-DE", "fr-FR"),
            GetStatisticsUrl: getStatisticsUrl ?? _faker.Internet.Url(),
            GetAllUsersStatisticsUrl: getAllUsersStatisticsUrl ?? _faker.Internet.Url(),
            GetRecentlyConsumedUrl: getRecentlyConsumedUrl ?? _faker.Internet.Url(),
            GetMostConsumedUrl: getMostConsumedUrl ?? _faker.Internet.Url(),
            GetTagConsumptionUrl: getTagConsumptionUrl ?? _faker.Internet.Url(),
            GetGenreConsumptionUrl: getGenreConsumptionUrl ?? _faker.Internet.Url(),
            GetLibrariesUrl: getLibrariesUrl ?? _faker.Internet.Url(),
            GetUsersUrl: getUsersUrl ?? _faker.Internet.Url(),
            DeleteConsumptionItemHistoryUrl: deleteConsumptionItemHistoryUrl ?? _faker.Internet.Url(),
            ClearConsumptionHistoryUrl: clearConsumptionHistoryUrl ?? _faker.Internet.Url(),
            ClearResumePointsUrl: clearResumePointsUrl ?? _faker.Internet.Url()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="StatisticsPageDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="StatisticsPageDto"/> instances.</returns>
    public List<StatisticsPageDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
