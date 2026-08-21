#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;

/// <summary>
/// Aggregate root for the settings of a user.
/// </summary>
[DebuggerDisplay("Id: {Id}; UserId: {UserId}")]
public class UserSettings : AggregateRoot<UserSettingsId>
{
    private const bool DEFAULT_IS_PAGINATION_ENABLED = true;
    private const int DEFAULT_ITEMS_PER_PAGE = 48;
    private const bool DEFAULT_IGNORE_THE_PREFIX_FOR_ALPHA_PICKER = false;
    private const bool DEFAULT_IS_THEME_CACHING_ENABLED = true;

    /// <summary>
    /// Gets the object representing the unique identifier of the user that owns these settings.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets whether pagination is enabled for the user, or not.
    /// </summary>
    public bool IsPaginationEnabled { get; private set; }

    /// <summary>
    /// Gets the number of library items displayed per page when pagination is enabled.
    /// </summary>
    public int ItemsPerPage { get; private set; }

    /// <summary>
    /// Gets whether the "The" prefix of library item titles is ignored by the alpha picker, or not.
    /// </summary>
    public bool IgnoreThePrefixForAlphaPicker { get; private set; }

    /// <summary>
    /// Gets whether the theme data served to this user is cached, or not.
    /// </summary>
    public bool IsThemeCachingEnabled { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettings"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of these settings.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="ignoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
    private UserSettings(
        UserSettingsId id,
        UserId userId,
        bool isPaginationEnabled,
        int itemsPerPage,
        bool ignoreThePrefixForAlphaPicker,
        bool isThemeCachingEnabled) : base(id)
    {
        UserId = userId;
        IsPaginationEnabled = isPaginationEnabled;
        ItemsPerPage = itemsPerPage;
        IgnoreThePrefixForAlphaPicker = ignoreThePrefixForAlphaPicker;
        IsThemeCachingEnabled = isThemeCachingEnabled;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettings"/> class, with the default settings.
    /// </summary>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="UserSettings"/>, or an error message.
    /// </returns>
    public static Result<UserSettings> Create()
    {
        return Create(UserId.CreateUnique(), DEFAULT_IS_PAGINATION_ENABLED, DEFAULT_ITEMS_PER_PAGE, DEFAULT_IGNORE_THE_PREFIX_FOR_ALPHA_PICKER, DEFAULT_IS_THEME_CACHING_ENABLED);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettings"/> class, with the default settings.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that owns these settings.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="UserSettings"/>, or an error message.
    /// </returns>
    public static Result<UserSettings> Create(UserId userId)
    {
        return Create(userId, DEFAULT_IS_PAGINATION_ENABLED, DEFAULT_ITEMS_PER_PAGE, DEFAULT_IGNORE_THE_PREFIX_FOR_ALPHA_PICKER, DEFAULT_IS_THEME_CACHING_ENABLED);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettings"/> class.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="ignoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="UserSettings"/>, or an error message.
    /// </returns>
    public static Result<UserSettings> Create(
        UserId userId,
        bool isPaginationEnabled,
        int itemsPerPage,
        bool ignoreThePrefixForAlphaPicker,
        bool isThemeCachingEnabled)
    {
        if (itemsPerPage <= 0)
            return Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero;

        return new UserSettings(
            UserSettingsId.CreateUnique(),
            userId,
            isPaginationEnabled,
            itemsPerPage,
            ignoreThePrefixForAlphaPicker,
            isThemeCachingEnabled);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettings"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of these settings.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="ignoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="UserSettings"/>, or an error message.
    /// </returns>
    public static Result<UserSettings> Create(
        UserSettingsId id,
        UserId userId,
        bool isPaginationEnabled,
        int itemsPerPage,
        bool ignoreThePrefixForAlphaPicker,
        bool isThemeCachingEnabled)
    {
        if (itemsPerPage <= 0)
            return Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero;

        return new UserSettings(id, userId, isPaginationEnabled, itemsPerPage, ignoreThePrefixForAlphaPicker, isThemeCachingEnabled);
    }

    /// <summary>
    /// Updates the settings of the user.
    /// </summary>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="ignoreThePrefixForAlphaPicker">Whether the "The" prefix of library item titles is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached, or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public Result<Updated> UpdateSettings(
        bool isPaginationEnabled,
        int itemsPerPage,
        bool ignoreThePrefixForAlphaPicker,
        bool isThemeCachingEnabled)
    {
        if (itemsPerPage <= 0)
            return Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero;

        IsPaginationEnabled = isPaginationEnabled;
        ItemsPerPage = itemsPerPage;
        IgnoreThePrefixForAlphaPicker = ignoreThePrefixForAlphaPicker;
        IsThemeCachingEnabled = isThemeCachingEnabled;
        return Result.Updated;
    }
}
