#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
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
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty);
       
        RuleFor(command => command.PluginIds)
            .NotNull()
            .WithError(Errors.Plugins.PluginIdsListCannotBeNull)
            .NotEmpty()
            .WithError(Errors.Plugins.PluginIdsListCannotBeEmpty);
    }
}
