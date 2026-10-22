#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Favorite error types.
/// </summary>
public static partial class Errors
{
    public static class Favorites
    {
        public static Error FavoriteNotFound => Error.NotFound(description: nameof(FavoriteNotFound));
        public static Error FavoriteAlreadyExists => Error.Conflict(description: nameof(FavoriteAlreadyExists));
        public static Error FavoriteMediaKindIsInvalid => Error.Validation(description: nameof(FavoriteMediaKindIsInvalid));
        public static Error FavoriteMediaKindIsNotSupported => Error.Validation(description: nameof(FavoriteMediaKindIsNotSupported));
        public static Error FavoriteMediaItemIdCannotBeEmpty => Error.Validation(description: nameof(FavoriteMediaItemIdCannotBeEmpty));
        public static Error FavoriteMediaItemIdsCannotBeEmpty => Error.Validation(description: nameof(FavoriteMediaItemIdsCannotBeEmpty));
        public static Error FavoriteMediaItemIdsExceedMaximumCount => Error.Validation(description: nameof(FavoriteMediaItemIdsExceedMaximumCount));
    }
}
