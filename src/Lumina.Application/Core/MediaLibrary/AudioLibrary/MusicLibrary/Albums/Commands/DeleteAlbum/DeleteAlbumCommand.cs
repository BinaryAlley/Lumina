#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;

/// <summary>
/// Command for deleting an album by its Id.
/// </summary>
/// <param name="AlbumId">The unique identifier of the album to delete.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record DeleteAlbumCommand(
    Guid AlbumId
) : ICommand;
