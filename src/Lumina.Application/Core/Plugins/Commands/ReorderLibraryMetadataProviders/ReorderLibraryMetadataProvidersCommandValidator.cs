#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.SharedKernel.Common.Errors;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;

/// <summary>
/// Validates the needed validation rules for <see cref="ReorderLibraryMetadataProvidersCommand"/>.
/// </summary>
public class ReorderLibraryMetadataProvidersCommandValidator : AbstractValidator<ReorderLibraryMetadataProvidersCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryMetadataProvidersCommandValidator"/> class.
    /// </summary>
    public ReorderLibraryMetadataProvidersCommandValidator()
    {
        RuleFor(x => x.LibraryId)
            .NotEmpty().WithMessage(Errors.Plugins.LibraryIdCannotBeEmpty.Description);
        RuleFor(x => x.PluginIds)
            .NotNull().WithMessage(Errors.Plugins.PluginIdsListCannotBeNull.Description)
            .NotEmpty().WithMessage(Errors.Plugins.PluginIdsListCannotBeEmpty.Description);
    }
}
