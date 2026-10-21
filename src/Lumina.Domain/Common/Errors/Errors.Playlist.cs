#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Playlist error types.
/// </summary>
public static partial class Errors
{
    public static class Playlists
    {
        public static Error PlaylistNotFound => Error.NotFound(description: nameof(PlaylistNotFound));
        public static Error PlaylistAlreadyExists => Error.Conflict(description: nameof(PlaylistAlreadyExists));
        public static Error PlaylistIdCannotBeEmpty => Error.Validation(description: nameof(PlaylistIdCannotBeEmpty));
        public static Error PlaylistEntryIdCannotBeEmpty => Error.Validation(description: nameof(PlaylistEntryIdCannotBeEmpty));
        public static Error PlaylistNameCannotBeEmpty => Error.Validation(description: nameof(PlaylistNameCannotBeEmpty));
        public static Error PlaylistNameMustBeMaximum255CharactersLong => Error.Validation(description: nameof(PlaylistNameMustBeMaximum255CharactersLong));
        public static Error PlaylistDescriptionMustBeMaximum2000CharactersLong => Error.Validation(description: nameof(PlaylistDescriptionMustBeMaximum2000CharactersLong));
        public static Error PlaylistTypeIsInvalid => Error.Validation(description: nameof(PlaylistTypeIsInvalid));
        public static Error PlaylistDoesNotAcceptMediaItems => Error.Forbidden(description: nameof(PlaylistDoesNotAcceptMediaItems));
        public static Error PlaylistEntryNotFound => Error.NotFound(description: nameof(PlaylistEntryNotFound));
        public static Error PlaylistEntryPositionsAreInvalid => Error.Validation(description: nameof(PlaylistEntryPositionsAreInvalid));
        public static Error PlaylistMoveTargetPositionIsInvalid => Error.Validation(description: nameof(PlaylistMoveTargetPositionIsInvalid));
        public static Error PlaylistEntryWindowOffsetIsInvalid => Error.Validation(description: nameof(PlaylistEntryWindowOffsetIsInvalid));
        public static Error PlaylistEntryWindowLimitIsInvalid => Error.Validation(description: nameof(PlaylistEntryWindowLimitIsInvalid));
    }
}
