#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;

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
        RuleFor(query => query.ScanId)
            .NotEmpty()
            .WithError(Errors.LibraryScanning.LibraryScanIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.LibraryScanning.LibraryScanIdCannotBeEmpty);
       
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
