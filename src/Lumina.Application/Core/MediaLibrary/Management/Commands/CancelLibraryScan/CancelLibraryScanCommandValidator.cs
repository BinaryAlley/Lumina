#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
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
