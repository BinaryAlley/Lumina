#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;

/// <summary>
/// Validates the needed validation rules for <see cref="SetLibraryArtworkProviderEnabledCommand"/>.
/// </summary>
public class SetLibraryArtworkProviderEnabledCommandValidator : AbstractValidator<SetLibraryArtworkProviderEnabledCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryArtworkProviderEnabledCommandValidator"/> class.
    /// </summary>
    public SetLibraryArtworkProviderEnabledCommandValidator()
    {
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty);

        RuleFor(command => command.PluginId)
            .NotEmpty()
            .WithError(Errors.Plugins.PluginIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Plugins.PluginIdCannotBeEmpty);
    }
}
