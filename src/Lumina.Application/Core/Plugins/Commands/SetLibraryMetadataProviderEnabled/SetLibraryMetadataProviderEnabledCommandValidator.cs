#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.SharedKernel.Common.Errors;
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
        RuleFor(x => x.LibraryId)
            .NotEmpty().WithMessage(Errors.Plugins.LibraryIdCannotBeEmpty.Description);
        RuleFor(x => x.PluginId)
            .NotEmpty().WithMessage(Errors.Plugins.PluginIdCannotBeEmpty.Description);
    }
}
