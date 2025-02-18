#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;

/// <summary>
/// Validates the needed validation rules for <see cref="GetLibraryScanProgressQuery"/>.
/// </summary>
public class GetLibraryScanProgressQueryValidator : AbstractValidator<GetLibraryScanProgressQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryScanProgressQueryValidator"/> class.
    /// </summary>
    public GetLibraryScanProgressQueryValidator()
    {
        RuleFor(x => x.ScanId)
            .NotEmpty().WithMessage(Errors.LibraryScanning.LibraryScanIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.LibraryScanning.LibraryScanIdCannotBeEmpty.Description);
        RuleFor(x => x.LibraryId)
            .NotEmpty().WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Library.LibraryIdCannotBeEmpty.Description);
    }
}
