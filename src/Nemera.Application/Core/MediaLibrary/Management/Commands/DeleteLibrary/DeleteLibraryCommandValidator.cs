#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
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
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
