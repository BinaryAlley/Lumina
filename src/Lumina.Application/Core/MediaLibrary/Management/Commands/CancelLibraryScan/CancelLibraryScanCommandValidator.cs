#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

/// <summary>
/// Validates the needed validation rules for <see cref="CancelLibraryScanCommand"/>.
/// </summary>
public class CancelLibraryScanCommandValidator : AbstractValidator<CancelLibraryScanCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanCommandValidator"/> class.
    /// </summary>
    public CancelLibraryScanCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description);
    }
}
