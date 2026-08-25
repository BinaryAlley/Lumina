#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;

/// <summary>
/// Validates the needed validation rules for <see cref="SetLibraryMetadataProviderEnabledCommand"/>.
/// </summary>
public class SetLibraryMetadataProviderEnabledCommandValidator : AbstractValidator<SetLibraryMetadataProviderEnabledCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryMetadataProviderEnabledCommandValidator"/> class.
    /// </summary>
    public SetLibraryMetadataProviderEnabledCommandValidator()
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
