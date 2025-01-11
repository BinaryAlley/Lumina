#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;

/// <summary>
/// Validates the needed validation rules for <see cref="DeleteLibraryCommand"/>.
/// </summary>
public class DeleteLibraryCommandValidator : AbstractValidator<DeleteLibraryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLibraryCommandValidator"/> class.
    /// </summary>
    public DeleteLibraryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description);
    }
}
