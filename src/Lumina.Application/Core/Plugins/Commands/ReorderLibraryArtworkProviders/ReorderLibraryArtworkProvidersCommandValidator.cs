#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.ReorderLibraryArtworkProviders;

/// <summary>
/// Validates the needed validation rules for <see cref="ReorderLibraryArtworkProvidersCommand"/>.
/// </summary>
public class ReorderLibraryArtworkProvidersCommandValidator : AbstractValidator<ReorderLibraryArtworkProvidersCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryArtworkProvidersCommandValidator"/> class.
    /// </summary>
    public ReorderLibraryArtworkProvidersCommandValidator()
    {
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty);

        RuleFor(command => command.PluginIds)
            .NotNull()
            .WithError(Errors.Plugins.PluginIdsListCannotBeNull)
            .NotEmpty()
            .WithError(Errors.Plugins.PluginIdsListCannotBeEmpty);
    }
}
