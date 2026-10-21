#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.CreatePlaylist;

/// <summary>
/// Validates the needed validation rules for <see cref="CreatePlaylistCommand"/>.
/// </summary>
public class CreatePlaylistCommandValidator : AbstractValidator<CreatePlaylistCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistCommandValidator"/> class.
    /// </summary>
    public CreatePlaylistCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithError(Errors.Playlists.PlaylistNameCannotBeEmpty);

        RuleFor(command => command.Name)
            .MaximumLength(255)
            .WithError(Errors.Playlists.PlaylistNameMustBeMaximum255CharactersLong);

        RuleFor(command => command.Description)
            .MaximumLength(2000)
            .WithError(Errors.Playlists.PlaylistDescriptionMustBeMaximum2000CharactersLong);

        RuleFor(command => command.Type)
            .NotNull()
            .WithError(Errors.Playlists.PlaylistTypeIsInvalid);

        RuleFor(command => command.Type)
            .IsInEnum()
            .WithError(Errors.Playlists.PlaylistTypeIsInvalid)
            .When(command => command.Type is not null);
    }
}
