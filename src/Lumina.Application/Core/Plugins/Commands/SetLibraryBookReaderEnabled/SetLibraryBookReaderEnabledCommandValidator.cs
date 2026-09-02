#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;

/// <summary>
/// Validates the needed validation rules for <see cref="SetLibraryBookReaderEnabledCommand"/>.
/// </summary>
public class SetLibraryBookReaderEnabledCommandValidator : AbstractValidator<SetLibraryBookReaderEnabledCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryBookReaderEnabledCommandValidator"/> class.
    /// </summary>
    public SetLibraryBookReaderEnabledCommandValidator()
    {
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Plugins.LibraryIdCannotBeEmpty);

        RuleFor(command => command.PluginId)
            .NotEmpty()
            .WithError(Errors.Plugins.PluginIdCannotBeEmpty);
    }
}
