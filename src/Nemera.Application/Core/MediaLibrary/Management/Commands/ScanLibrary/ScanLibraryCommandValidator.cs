#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;

/// <summary>
/// Validates the needed validation rules for <see cref="ScanLibraryCommand"/>.
/// </summary>
public class ScanLibraryCommandValidator : AbstractValidator<ScanLibraryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryCommandValidator"/> class.
    /// </summary>
    public ScanLibraryCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
