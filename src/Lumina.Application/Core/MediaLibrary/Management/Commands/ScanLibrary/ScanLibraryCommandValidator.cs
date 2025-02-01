#region ========================================================================= USING =====================================================================================
using FluentValidation;
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
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description);
    }
}
