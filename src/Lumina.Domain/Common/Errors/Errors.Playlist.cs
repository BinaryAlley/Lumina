#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Playlist error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report playlist rule violations and missing playlists.
    /// </summary>
    public static class Playlists
    {
        /// <summary>
        /// Gets the error that reports that the requested playlist was not found.
        /// </summary>
        public static Error PlaylistNotFound => Error.NotFound(description: nameof(PlaylistNotFound));

        /// <summary>
        /// Gets the error that reports that a playlist with the same identifier already exists.
        /// </summary>
        public static Error PlaylistAlreadyExists => Error.Conflict(description: nameof(PlaylistAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the playlist identifier cannot be empty.
        /// </summary>
        public static Error PlaylistIdCannotBeEmpty => Error.Validation(description: nameof(PlaylistIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the playlist entry identifier cannot be empty.
        /// </summary>
        public static Error PlaylistEntryIdCannotBeEmpty => Error.Validation(description: nameof(PlaylistEntryIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the playlist name cannot be empty.
        /// </summary>
        public static Error PlaylistNameCannotBeEmpty => Error.Validation(description: nameof(PlaylistNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the playlist name exceeds the maximum length of 255 characters.
        /// </summary>
        public static Error PlaylistNameMustBeMaximum255CharactersLong => Error.Validation(description: nameof(PlaylistNameMustBeMaximum255CharactersLong));

        /// <summary>
        /// Gets the error that reports that the playlist description exceeds the maximum length of 2000 characters.
        /// </summary>
        public static Error PlaylistDescriptionMustBeMaximum2000CharactersLong => Error.Validation(description: nameof(PlaylistDescriptionMustBeMaximum2000CharactersLong));

        /// <summary>
        /// Gets the error that reports that the playlist type is not a defined value.
        /// </summary>
        public static Error PlaylistTypeIsInvalid => Error.Validation(description: nameof(PlaylistTypeIsInvalid));

        /// <summary>
        /// Gets the error that reports that the playlist type does not accept media items.
        /// </summary>
        public static Error PlaylistDoesNotAcceptMediaItems => Error.Forbidden(description: nameof(PlaylistDoesNotAcceptMediaItems));

        /// <summary>
        /// Gets the error that reports that the requested playlist entry was not found.
        /// </summary>
        public static Error PlaylistEntryNotFound => Error.NotFound(description: nameof(PlaylistEntryNotFound));

        /// <summary>
        /// Gets the error that reports that the positions of the playlist entries are not valid.
        /// </summary>
        public static Error PlaylistEntryPositionsAreInvalid => Error.Validation(description: nameof(PlaylistEntryPositionsAreInvalid));

        /// <summary>
        /// Gets the error that reports that the target position of the playlist entry move is not valid.
        /// </summary>
        public static Error PlaylistMoveTargetPositionIsInvalid => Error.Validation(description: nameof(PlaylistMoveTargetPositionIsInvalid));
    }
}
