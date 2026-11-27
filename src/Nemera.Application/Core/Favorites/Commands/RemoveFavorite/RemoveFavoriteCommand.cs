#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Favorites.Commands.RemoveFavorite;

/// <summary>
/// Command for removing a media item from the favorites of the current user.
/// </summary>
/// <param name="MediaKind">The name of the kind of the favorited media item, taken from the route.</param>
/// <param name="MediaItemId">The unique identifier of the favorited media item, taken from the route.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public record RemoveFavoriteCommand(
    string? MediaKind,
    string? MediaItemId
) : ICommand;
