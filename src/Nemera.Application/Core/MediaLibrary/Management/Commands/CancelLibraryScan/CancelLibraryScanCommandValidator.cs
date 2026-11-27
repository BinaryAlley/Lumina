#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

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
        RuleFor(command => command.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(command => command.ScanId)
            .NotEmpty()
            .WithError(Errors.LibraryScanning.ScanIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.LibraryScanning.ScanIdCannotBeEmpty);
    }
}
