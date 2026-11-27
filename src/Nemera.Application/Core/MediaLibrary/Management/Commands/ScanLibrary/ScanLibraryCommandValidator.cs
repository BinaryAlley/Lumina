#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;

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
