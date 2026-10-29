#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.RemovePlaylistEntry;

/// <summary>
/// Validates the needed validation rules for <see cref="RemovePlaylistEntryCommand"/>.
/// </summary>
public class RemovePlaylistEntryCommandValidator : AbstractValidator<RemovePlaylistEntryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryCommandValidator"/> class.
    /// </summary>
    public RemovePlaylistEntryCommandValidator()
    {
        RuleFor(command => command.PlaylistId)
            .NotEmpty()
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(command => command.PlaylistId)
            .Must(playlistId => Guid.TryParse(playlistId, out Guid parsedPlaylistId) && parsedPlaylistId != Guid.Empty)
            .When(command => command.PlaylistId is not null && command.PlaylistId.Length > 0)
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(command => command.PlaylistEntryId)
            .NotEmpty()
            .WithError(Errors.Playlists.PlaylistEntryIdCannotBeEmpty);

        RuleFor(command => command.PlaylistEntryId)
            .Must(entryId => Guid.TryParse(entryId, out Guid parsedEntryId) && parsedEntryId != Guid.Empty)
            .When(command => command.PlaylistEntryId is not null && command.PlaylistEntryId.Length > 0)
            .WithError(Errors.Playlists.PlaylistEntryIdCannotBeEmpty);
    }
}
