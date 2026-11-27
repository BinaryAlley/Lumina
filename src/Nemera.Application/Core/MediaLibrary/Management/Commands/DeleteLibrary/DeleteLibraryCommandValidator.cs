#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;

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
