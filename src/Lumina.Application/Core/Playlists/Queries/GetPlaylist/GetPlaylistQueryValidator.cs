#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Playlists.Queries.GetPlaylist;

/// <summary>
/// Validates the needed validation rules for <see cref="GetPlaylistQuery"/>.
/// </summary>
public class GetPlaylistQueryValidator : AbstractValidator<GetPlaylistQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistQueryValidator"/> class.
    /// </summary>
    public GetPlaylistQueryValidator()
    {
        RuleFor(query => query.PlaylistId)
            .NotEmpty()
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(query => query.PlaylistId)
            .Must(playlistId => Guid.TryParse(playlistId, out Guid parsedPlaylistId) && parsedPlaylistId != Guid.Empty)
            .When(query => query.PlaylistId is not null && query.PlaylistId.Length > 0)
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(query => query.Offset)
            .GreaterThanOrEqualTo(0)
            .WithError(Errors.Playlists.PlaylistEntryWindowOffsetIsInvalid);

        RuleFor(query => query.Limit)
            .InclusiveBetween(1, PlaylistWindow.MAXIMUM_ENTRY_WINDOW_SIZE)
            .WithError(Errors.Playlists.PlaylistEntryWindowLimitIsInvalid);
    }
}
