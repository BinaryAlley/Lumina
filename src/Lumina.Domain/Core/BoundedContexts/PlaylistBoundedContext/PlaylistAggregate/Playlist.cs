#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;

/// <summary>
/// Aggregate root for a playlist.
/// </summary>
[DebuggerDisplay("Id: {Id.Value}; Name: {Name}; Type: {Type}")]
public sealed class Playlist : AggregateRoot<PlaylistId>
{
    private readonly List<PlaylistEntry> _entries;

    /// <summary>
    /// Gets the object representing the unique identifier of the user that owns the playlist.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the name of the playlist.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description of the playlist, if applicable.
    /// </summary>
    public Optional<string> Description { get; private set; }

    /// <summary>
    /// Gets the kind of media the playlist holds.
    /// </summary>
    public PlaylistType Type { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the playlist was marked for deletion in this unit of work.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Gets the ordered entries of the playlist.
    /// </summary>
    public IReadOnlyCollection<PlaylistEntry> Entries => _entries.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Playlist"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the playlist.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <param name="description">The optional description of the playlist.</param>
    /// <param name="type">The kind of media the playlist holds.</param>
    /// <param name="entries">The ordered entries of the playlist.</param>
    /// <param name="createdOnUtc">The date and time when the playlist was created.</param>
    /// <param name="updatedOnUtc">The date and time when the playlist was last updated.</param>
    private Playlist(
        PlaylistId id,
        UserId userId,
        string name,
        Optional<string> description,
        PlaylistType type,
        List<PlaylistEntry> entries,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc) : base(id)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Description = description;
        Type = type;
        _entries = entries;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
    }

    /// <summary>
    /// Creates a new playlist for the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <param name="description">The optional description of the playlist.</param>
    /// <param name="type">The kind of media the playlist holds.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Playlist"/>, or an error message.
    /// </returns>
    public static Result<Playlist> Create(UserId userId, string name, Optional<string> description, PlaylistType type)
    {
        DateTime occurredOnUtc = DateTime.UtcNow;
        Playlist playlist = new(
            PlaylistId.CreateUnique(),
            userId,
            name,
            description,
            type,
            [],
            occurredOnUtc,
            Optional<DateTime>.None());

        playlist._domainEvents.Add(new PlaylistCreatedDomainEvent(
            Guid.NewGuid(),
            playlist.Id,
            userId,
            name,
            type,
            occurredOnUtc));
        return playlist;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Playlist"/> class, with pre-existing data.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the playlist.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <param name="description">The optional description of the playlist.</param>
    /// <param name="type">The kind of media the playlist holds.</param>
    /// <param name="entries">The ordered entries of the playlist.</param>
    /// <param name="createdOnUtc">The date and time when the playlist was created.</param>
    /// <param name="updatedOnUtc">The date and time when the playlist was last updated.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Playlist"/>, or an error message.
    /// </returns>
    public static Result<Playlist> Create(
        PlaylistId id,
        UserId userId,
        string name,
        Optional<string> description,
        PlaylistType type,
        List<PlaylistEntry> entries,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc)
    {
        if (entries.Any(entry => !CanContain(type, entry.MediaItemType)))
            return Errors.Playlists.PlaylistDoesNotAcceptMediaItems;
        // The positions are sparse order keys, so they must be unique. The gaps between them are intentional and are not a reason to reject an otherwise readable playlist.
        if (!ArePositionsValid(entries))
            return Errors.Playlists.PlaylistEntryPositionsAreInvalid;

        return new Playlist(id, userId, name, description, type, entries, createdOnUtc, updatedOnUtc);
    }

    /// <summary>
    /// Updates the name and the description of the playlist, without touching its entries.
    /// </summary>
    /// <param name="name">The name of the playlist.</param>
    /// <param name="description">The optional description of the playlist.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public Result<Updated> UpdateDetails(string name, Optional<string> description)
    {
        bool hasChanged = Name != name
            || Description.HasValue != description.HasValue
            || (Description.HasValue && description.HasValue && Description.Value != description.Value);
        if (!hasChanged)
            return Result.Updated;

        Name = name;
        Description = description;
        DateTime occurredOnUtc = DateTime.UtcNow;
        _domainEvents.Add(new PlaylistDetailsUpdatedDomainEvent(
            Guid.NewGuid(),
            Id,
            UserId,
            name,
            occurredOnUtc));
        return Result.Updated;
    }

    /// <summary>
    /// Marks the playlist as deleted, raising the domain event that announces its deletion.
    /// </summary>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Deleted> Delete()
    {
        if (IsDeleted)
            return Result.Deleted;

        // Earlier events become irrelevant once the playlist is deleted.
        _domainEvents.Clear();
        IsDeleted = true;
        _domainEvents.Add(new PlaylistDeletedDomainEvent(
            Guid.NewGuid(),
            Id,
            UserId,
            DateTime.UtcNow));
        return Result.Deleted;
    }

    /// <summary>
    /// Determines whether a playlist of <paramref name="playlistType"/> can contain <paramref name="mediaItemType"/>.
    /// </summary>
    /// <param name="playlistType">The kind of media the playlist holds.</param>
    /// <param name="mediaItemType">The kind of media item referenced by an entry.</param>
    /// <returns><see langword="true"/> if the media item type belongs to the playlist type, <see langword="false"/> otherwise.</returns>
    public static bool CanContain(PlaylistType playlistType, PlaylistMediaItemType mediaItemType)
    {
        return playlistType switch
        {
            PlaylistType.Music => mediaItemType == PlaylistMediaItemType.MusicTrack,
            _ => false
        };
    }

    /// <summary>
    /// Determines whether the positions of <paramref name="entries"/> are unique.
    /// </summary>
    /// <param name="entries">The entries whose positions are checked.</param>
    /// <returns><see langword="true"/> if the positions are unique, <see langword="false"/> otherwise.</returns>
    private static bool ArePositionsValid(IReadOnlyCollection<PlaylistEntry> entries)
    {
        // The storage enforces unique order keys with a unique index, and the sparse keys are allowed to have wide gaps.
        HashSet<long> positions = [];
        foreach (PlaylistEntry entry in entries)
            if (!positions.Add(entry.Position))
                return false;
        return true;
    }
}
