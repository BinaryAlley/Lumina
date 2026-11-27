#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Favorite error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report favorite rule violations and missing favorites.
    /// </summary>
    public static class Favorites
    {
        /// <summary>
        /// Gets the error that reports that the requested favorite was not found.
        /// </summary>
        public static Error FavoriteNotFound => Error.NotFound(description: nameof(FavoriteNotFound));

        /// <summary>
        /// Gets the error that reports that the user already has the media item in favorites.
        /// </summary>
        public static Error FavoriteAlreadyExists => Error.Conflict(description: nameof(FavoriteAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the favorite media kind is not a defined value.
        /// </summary>
        public static Error FavoriteMediaKindIsInvalid => Error.Validation(description: nameof(FavoriteMediaKindIsInvalid));

        /// <summary>
        /// Gets the error that reports that the media kind cannot be favorited because no library provides it yet.
        /// </summary>
        public static Error FavoriteMediaKindIsNotSupported => Error.Validation(description: nameof(FavoriteMediaKindIsNotSupported));

        /// <summary>
        /// Gets the error that reports that the media item identifier of the favorite cannot be empty.
        /// </summary>
        public static Error FavoriteMediaItemIdCannotBeEmpty => Error.Validation(description: nameof(FavoriteMediaItemIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the list of media item identifiers cannot be empty.
        /// </summary>
        public static Error FavoriteMediaItemIdsCannotBeEmpty => Error.Validation(description: nameof(FavoriteMediaItemIdsCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the list of media item identifiers exceeds the maximum allowed count.
        /// </summary>
        public static Error FavoriteMediaItemIdsExceedMaximumCount => Error.Validation(description: nameof(FavoriteMediaItemIdsExceedMaximumCount));
    }
}
