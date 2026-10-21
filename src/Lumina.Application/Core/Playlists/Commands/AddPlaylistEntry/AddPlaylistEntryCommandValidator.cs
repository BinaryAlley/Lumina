#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;

/// <summary>
/// Validates the needed validation rules for <see cref="AddPlaylistEntryCommand"/>.
/// </summary>
public class AddPlaylistEntryCommandValidator : AbstractValidator<AddPlaylistEntryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryCommandValidator"/> class.
    /// </summary>
    public AddPlaylistEntryCommandValidator()
    {
        RuleFor(command => command.PlaylistId)
            .NotEmpty()
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(command => command.PlaylistId)
            .Must(playlistId => Guid.TryParse(playlistId, out Guid parsedPlaylistId) && parsedPlaylistId != Guid.Empty)
            .When(command => command.PlaylistId is not null && command.PlaylistId.Length > 0)
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(command => command.TrackId)
            .NotNull()
            .WithError(Errors.Music.TrackIdCannotBeEmpty);

        RuleFor(command => command.TrackId)
            .NotEmpty()
            .WithError(Errors.Music.TrackIdCannotBeEmpty)
            .When(command => command.TrackId is not null);
    }
}
