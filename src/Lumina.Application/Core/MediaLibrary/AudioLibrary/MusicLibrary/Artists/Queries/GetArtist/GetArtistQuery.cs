#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;

/// <summary>
/// Query for getting an artist by its Id.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist to get.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistQuery(
    Guid ArtistId
) : IQuery;
