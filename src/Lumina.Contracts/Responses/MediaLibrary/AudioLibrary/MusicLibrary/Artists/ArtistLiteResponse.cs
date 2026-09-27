#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a lightweight artist response, containing only the fields needed by the client for card-style navigation.
/// </summary>
/// <param name="Id">The Id of the artist.</param>
/// <param name="Name">The name of the artist.</param>
[DebuggerDisplay("Name: {Name}")]
public record ArtistLiteResponse(
    Guid Id,
    string Name
);
