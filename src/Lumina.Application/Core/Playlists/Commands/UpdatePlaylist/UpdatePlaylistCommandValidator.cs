#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.UpdatePlaylist;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdatePlaylistCommand"/>.
/// </summary>
public class UpdatePlaylistCommandValidator : AbstractValidator<UpdatePlaylistCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistCommandValidator"/> class.
    /// </summary>
    public UpdatePlaylistCommandValidator()
    {
        RuleFor(command => command.PlaylistId)
            .NotEmpty()
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(command => command.PlaylistId)
            .Must(playlistId => Guid.TryParse(playlistId, out Guid parsedPlaylistId) && parsedPlaylistId != Guid.Empty)
            .When(command => command.PlaylistId is not null && command.PlaylistId.Length > 0)
            .WithError(Errors.Playlists.PlaylistIdCannotBeEmpty);

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithError(Errors.Playlists.PlaylistNameCannotBeEmpty);

        RuleFor(command => command.Name)
            .MaximumLength(255)
            .WithError(Errors.Playlists.PlaylistNameMustBeMaximum255CharactersLong);

        RuleFor(command => command.Description)
            .MaximumLength(2000)
            .WithError(Errors.Playlists.PlaylistDescriptionMustBeMaximum2000CharactersLong);
    }
}
