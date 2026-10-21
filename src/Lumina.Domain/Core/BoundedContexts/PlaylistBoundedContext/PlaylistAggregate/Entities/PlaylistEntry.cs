#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;

/// <summary>
/// Entity for an ordered entry of a playlist.
/// </summary>
[DebuggerDisplay("Id: {Id.Value}; MediaItemId: {MediaItemId.Value}; MediaItemType: {MediaItemType}; Position: {Position}")]
public sealed class PlaylistEntry : Entity<PlaylistEntryId>
{
    /// <summary>
    /// Gets the Id of the media item referenced by the entry.
    /// </summary>
    public MediaItemId MediaItemId { get; private set; }

    /// <summary>
    /// Gets the kind of media item referenced by the entry.
    /// </summary>
    public PlaylistMediaItemType MediaItemType { get; private set; }

    /// <summary>
    /// Gets the sparse order key of the entry in the ordered entries of the playlist.
    /// </summary>
    public long Position { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistEntry"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the entry.</param>
    /// <param name="mediaItemId">The Id of the media item referenced by the entry.</param>
    /// <param name="mediaItemType">The kind of media item referenced by the entry.</param>
    /// <param name="position">The sparse order key of the entry in the ordered entries of the playlist.</param>
    private PlaylistEntry(PlaylistEntryId id, MediaItemId mediaItemId, PlaylistMediaItemType mediaItemType, long position) : base(id)
    {
        Id = id;
        MediaItemId = mediaItemId;
        MediaItemType = mediaItemType;
        Position = position;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PlaylistEntry"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the entry.</param>
    /// <param name="mediaItemId">The Id of the media item referenced by the entry.</param>
    /// <param name="mediaItemType">The kind of media item referenced by the entry.</param>
    /// <param name="position">The sparse order key of the entry in the ordered entries of the playlist.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="PlaylistEntry"/>, or an error message.
    /// </returns>
    public static Result<PlaylistEntry> Create(PlaylistEntryId id, MediaItemId mediaItemId, PlaylistMediaItemType mediaItemType, long position)
    {
        return new PlaylistEntry(id, mediaItemId, mediaItemType, position);
    }
}
